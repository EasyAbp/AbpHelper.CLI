using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Extensions;
using EasyAbp.AbpHelper.Core.Models;
using Elsa;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Services.Models;
using Microsoft.Extensions.Logging;

namespace EasyAbp.AbpHelper.Core.Steps.Common
{
    [Activity(
        Category = "FileModifierStep",
        Description = "FileModifierStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class FileModifierStep : Step
    {
        [ActivityInput(
            Hint = "TargetFile",
            UIHint = ActivityInputUIHints.SingleLine,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript }
        )]
        public string? TargetFile
        {
            get => GetState<string?>();
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "Modifications",
            UIHint = ActivityInputUIHints.MultiLine,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript }
        )]
        public IList<Modification>? Modifications
        {
            get => GetState<IList<Modification>?>();
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "NewLine",
            UIHint = ActivityInputUIHints.SingleLine,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript }
        )]
        public string NewLine
        {
            get => GetState(() => "\r\n");
            set => SetState(value);
        }

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            TargetFile ??= context.GetVariable<string>(FileFinderStep.DefaultFileParameterName)!;
            Modifications ??= context.GetVariable<IList<Modification>>("Modifications")!;
            
            LogInput(() => TargetFile);
            LogInput(() => Modifications, $"Modifications count: {Modifications.Count}");

            var lines = await File.ReadAllLinesAsync(TargetFile);
            var errors = CheckModifications(Modifications, lines).ToArray();
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
                var lineModifications = Modifications
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
                foreach (var modification in lineModifications) Modifications.Remove(modification);

                newFile.AppendWithControlChar(beforeContents)
                    .AppendLineWithControlChar(lineText, NewLine)
                    .AppendWithControlChar(afterContents);
                NEXT_LINE: ;
            }

            await File.WriteAllTextAsync(TargetFile, newFile.ToString());

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
                var actualStartLine = modification.StartLine >= 0
                    ? modification.StartLine
                    : lines.Length + modification.StartLine;

                if (actualStartLine <= 0 || actualStartLine > lines.Length)
                    yield return
                        $"StartLine out of range: {modification}. {nameof(actualStartLine)}: {actualStartLine}";

                if (modification is IRange range)
                {
                    var actualEndLine = range.EndLine >= 0 ? range.EndLine : lines.Length + range.EndLine;
                    if (actualEndLine <= 0 || actualEndLine > lines.Length)
                        yield return $"EndLine out of range: {modification}. {nameof(actualEndLine)}: {actualEndLine}";

                    if (actualStartLine > actualEndLine)
                        yield return
                            $"StartLine grater than EndLine: {modification}. {nameof(actualStartLine)}: {actualStartLine} {nameof(actualEndLine)}: {actualEndLine}";
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

        public List<string> Errors { get; } = new();
    }
}