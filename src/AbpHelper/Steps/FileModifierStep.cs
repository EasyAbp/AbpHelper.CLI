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
            for (var line = 1; line <= lines.Length; line++)
            {
                var appendLine = true;
                var index = 0;
                while (index < modifications.Count)
                {
                    var modification = modifications[index];
                    if (line == modification.StartLine)
                    {
                        if (modification is Insertion insertion)
                        {
                            if (insertion.InsertPosition == InsertPosition.Before)
                            {
                                newFile.Append(insertion.Contents);
                                newFile.AppendLine(lines[line - 1]);
                            }
                            else
                            {
                                newFile.AppendLine(lines[line - 1]);
                                newFile.Append(insertion.Contents);
                            }

                            appendLine = false;
                        }
                        else if (modification is Deletion deletion)
                        {
                            line = deletion.EndLine + 1;
                        }
                        else if (modification is Replacement replacement)
                        {
                            newFile.Append(replacement.Contents);
                            line = replacement.EndLine + 1;
                        }

                        modifications.RemoveAt(index);
                    }
                    else
                    {
                        index++;
                    }
                }

                if (appendLine && line <= lines.Length) newFile.AppendLine(lines[line - 1]);
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