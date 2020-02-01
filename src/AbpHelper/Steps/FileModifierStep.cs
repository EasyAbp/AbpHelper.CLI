using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AbpHelper.Models;

namespace AbpHelper.Steps
{
    public class FileModifierStep : StepBase
    {
        public FileModifierStep(WorkflowContext context) : base(context)
        {
        }

        public override Task Run()
        {
            var filePathName = GetParameter<string>("FilePathName");
            var modifications = GetParameter<IList<Modification>>("Modifications");

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

            return Task.CompletedTask;
        }
    }
}