using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AbpHelper.Dtos;

namespace AbpHelper.Steps
{
    public class FileModifierStep : IStep<FileModifierStepInput, FileModifierStepOutput>
    {
        public Task<FileModifierStepOutput> Run(FileModifierStepInput input)
        {
            var filePathName = input.FilePathName;
            var modifications = input.Modifications;

            var newFile = new StringBuilder();
            var lines = File.ReadAllLines(filePathName);
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
                                newFile.Append(insertion.Content);
                                newFile.AppendLine(lines[line - 1]);
                            }
                            else
                            {
                                newFile.AppendLine(lines[line - 1]);
                                newFile.Append(insertion.Content);
                            }

                            appendLine = false;
                        }
                        else if (modification is Deletion deletion)
                        {
                            line = deletion.EndLine + 1;
                        }
                        else if (modification is Replacement replacement)
                        {
                            newFile.Append(replacement.Content);
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

            File.WriteAllText(filePathName, newFile.ToString());
            return Task.FromResult(new FileModifierStepOutput());
        }
    }

    public class FileModifierStepInput
    {
        public FileModifierStepInput(string filePathName, IList<Modification> modifications)
        {
            FilePathName = filePathName;
            Modifications = modifications;
        }

        public string FilePathName { get; }
        public IList<Modification> Modifications { get; }
    }

    public class FileModifierStepOutput
    {
    }
}