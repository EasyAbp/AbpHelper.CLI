using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Extensions;
using EasyAbp.AbpHelper.Core.Models;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Scripting.JavaScript;
using Elsa.Services.Models;
using Microsoft.Extensions.Logging;

namespace EasyAbp.AbpHelper.Core.Steps.Common
{
    public class FileModifierStep : Step
    {
        public WorkflowExpression<string> TargetFile
        {
            get => GetState(() => new JavaScriptExpression<string>(FileFinderStep.DefaultFileParameterName));
            set => SetState(value);
        }

        public WorkflowExpression<IList<Modification>> Modifications
        {
            get => GetState(() => new JavaScriptExpression<IList<Modification>>("Modifications"));
            set => SetState(value);
        }

        public WorkflowExpression<string> NewLine
        {
            get => GetState(() => new LiteralExpression<string>("\r\n"));
            set => SetState(value);
        }

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            var targetFile = await context.EvaluateAsync(TargetFile, cancellationToken);
            LogInput(() => targetFile);

            var modifications = await context.EvaluateAsync(Modifications, cancellationToken);
            LogInput(() => modifications, $"Modifications count: {modifications.Count}");

            var newLine = await context.EvaluateAsync(NewLine, cancellationToken);

            var lines = await File.ReadAllLinesAsync(targetFile);
            var errors = CheckModifications(modifications, lines).ToArray();
            if (errors.Length > 0)
            {
                foreach (var error in errors) Logger.LogError(error);

                throw new InvalidModificationException(errors);
            }

            var newFile = new StringBuilder();
            var beforeContents = new StringBuilder();
            var afterContents = new StringBuilder();
            for (var line = 1; line <= lines.Length; line++)
            {
                var lineText = lines[line - 1];
                var lineModifications = modifications
                    .Where(mod => mod.StartLine == line || // Positive line number
                                  mod.StartLine == line - lines.Length - 1 // Negative  line number
                    )
                    .ToArray();
                beforeContents.Clear();
                afterContents.Clear();
                foreach (var modification in lineModifications)
                    switch (modification)
                    {
                        case Insertion insertion:
                        {
                            if (insertion.InsertPosition == InsertPosition.Before)
                                beforeContents.Append(insertion.Contents);
                            else
                                afterContents.Append(insertion.Contents);

                            break;
                        }
                        case IRange range:
                            line = range.EndLine > 0 ? range.EndLine : lines.Length + range.EndLine + 1;
                            if (range is Replacement replacement) newFile.Append(replacement.Contents);

                            goto NEXT_LINE;
                    }

                // We don't need these modifications anymore
                foreach (var modification in lineModifications) modifications.Remove(modification);

                newFile.AppendWithControlChar(beforeContents)
                    .AppendLineWithControlChar(lineText, newLine)
                    .AppendWithControlChar(afterContents);
                NEXT_LINE: ;
            }

            await File.WriteAllTextAsync(targetFile, newFile.ToString());

            return Done();
        }

        private IEnumerable<string> CheckModifications(IList<Modification> modifications, string[] lines)
        {
            var insertions = modifications.OfType<Insertion>().ToArray();
            var deletionsAndReplacements = modifications.OfType<Deletion>()
                    .Concat(modifications.OfType<Replacement>().Cast<IRange>())
                    .ToArray()
                ;

            var errors = CheckLinesInRange(modifications, lines).ToArray();
            foreach (var error in errors) yield return error;

            if (errors.Any()) yield break; // No need to perform following check if out of range

            foreach (var error in CheckOverlap(deletionsAndReplacements, insertions)) yield return error;
        }

        private static IEnumerable<string> CheckLinesInRange(IList<Modification> modifications, string[] lines)
        {
            // Check StartLine and EndLine are in range
            foreach (var modification in modifications)
            {
                var actualStartLine = modification.StartLine >= 0 ? modification.StartLine : lines.Length + modification.StartLine;

                if (actualStartLine <= 0 || actualStartLine > lines.Length) yield return $"StartLine out of range: {modification}. {nameof(actualStartLine)}: {actualStartLine}";

                if (modification is IRange range)
                {
                    var actualEndLine = range.EndLine >= 0 ? range.EndLine : lines.Length + range.EndLine;
                    if (actualEndLine <= 0 || actualEndLine > lines.Length) yield return $"EndLine out of range: {modification}. {nameof(actualEndLine)}: {actualEndLine}";

                    if (actualStartLine > actualEndLine) yield return $"StartLine grater than EndLine: {modification}. {nameof(actualStartLine)}: {actualStartLine} {nameof(actualEndLine)}: {actualEndLine}";
                }
            }
        }

        private static IEnumerable<string> CheckOverlap(IRange[] deletionsAndReplacements, Insertion[] insertions)
        {
            // Check if deletions and replacements overlap with insertion
            foreach (var range in deletionsAndReplacements)
            foreach (var insertion in insertions)
                if (insertion.StartLine >= range.StartLine && insertion.StartLine <= range.EndLine)
                    yield return $"Overlap modifications: [{range}] - [{insertion}]";

            // Check if deletions and replacements overlap with each other
            for (var i = 0; i < deletionsAndReplacements.Length; i++)
            {
                var range1 = deletionsAndReplacements[i];
                for (var j = i + 1; j < deletionsAndReplacements.Length; j++)
                {
                    var range2 = deletionsAndReplacements[j];
                    if (
                        range1.StartLine >= range2.StartLine && range1.StartLine <= range2.EndLine ||
                        range1.EndLine >= range2.StartLine && range1.EndLine <= range2.EndLine
                    )
                        yield return $"Overlap modifications: [{range1}] - [{range2}]";
                }
            }
        }
    }

    public class InvalidModificationException : Exception
    {
        public InvalidModificationException(IEnumerable<string> errors)
        {
            Errors.AddRange(errors);
        }

        public List<string> Errors { get; } = new List<string>();
    }
}