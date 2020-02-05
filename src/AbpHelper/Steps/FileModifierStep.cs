using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AbpHelper.Models;

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
    }
}