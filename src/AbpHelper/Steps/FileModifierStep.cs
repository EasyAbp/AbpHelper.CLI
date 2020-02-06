using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AbpHelper.Models;
using Microsoft.Extensions.Logging;

namespace AbpHelper.Steps
{
    public class FileModifierStep : Step
    {
        public string File { get; set; } = string.Empty;
        public IList<Modification> Modifications { get; set; } = new List<Modification>();

        protected override async Task RunStep()
        {
            var targetFile = File.IsNullOrEmpty() ? GetParameter<string>("FilePathName") : File;
            LogInput(() => targetFile);

            var modifications = Modifications.IsNullOrEmpty() ? GetParameter<IList<Modification>>("Modifications") : Modifications;
            LogInput(() => modifications, $"Modifications count: {modifications.Count}");

            var errors = CheckModificationsOverlap(modifications).ToArray();
            if (errors.Length > 0)
            {
                foreach (var error in errors) Logger.LogError(error);

                throw new ModificationsOverlapException(errors);
            }

            var newFile = new StringBuilder();
            var lines = await System.IO.File.ReadAllLinesAsync(targetFile);
            var beforeContents = new StringBuilder();
            var afterContents = new StringBuilder();
            for (var line = 1; line <= lines.Length; line++)
            {
                var lineText = lines[line - 1];
                var lineModifications = modifications.Where(mod => mod.StartLine == line).ToArray();
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
                        case Deletion deletion:
                            line = deletion.EndLine;
                            goto NEXT_LINE;
                        case Replacement replacement:
                            newFile.Append(replacement.Contents);
                            line = replacement.EndLine;
                            goto NEXT_LINE;
                    }

                // We don't need these modifications anymore
                foreach (var modification in lineModifications) modifications.Remove(modification);

                newFile.Append(beforeContents).AppendLine(lineText).Append(afterContents);
                NEXT_LINE: ;
            }

            await System.IO.File.WriteAllTextAsync(targetFile, newFile.ToString());
        }

        private IEnumerable<string> CheckModificationsOverlap(IList<Modification> modifications)
        {
            var insertions = modifications.OfType<Insertion>().ToArray();
            var deletionsAndReplacements = modifications.OfType<Deletion>()
                    .Concat(modifications.OfType<Replacement>().Cast<IRange>())
                    .ToArray()
                ;

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

    public class ModificationsOverlapException : Exception
    {
        public ModificationsOverlapException(IEnumerable<string> errors)
        {
            Errors.AddRange(errors);
        }

        public List<string> Errors { get; } = new List<string>();
    }
}