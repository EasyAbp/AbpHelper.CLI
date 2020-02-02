using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AbpHelper.Models;
using AbpHelper.Workflow;

namespace AbpHelper.Steps
{
    public class FileModifierStep : Step
    {
        public FileModifierStep(WorkflowContext workflowContext) : base(workflowContext)
        {
        }

        public string FilePathName { get; set; } = string.Empty;
        public IList<Modification> Modifications { get; set; } = new List<Modification>();

        protected override Task RunStep()
        {
            LogInput(() => FilePathName);
            LogInput(() => Modifications, $"Modifications count: {Modifications.Count}");

            var newFile = new StringBuilder();
            var lines = File.ReadAllLines(FilePathName);
            for (var line = 1; line <= lines.Length; line++)
            {
                var appendLine = true;
                var index = 0;
                while (index < Modifications.Count)
                {
                    var modification = Modifications[index];
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

                        Modifications.RemoveAt(index);
                    }
                    else
                    {
                        index++;
                    }
                }

                if (appendLine && line <= lines.Length) newFile.AppendLine(lines[line - 1]);
            }

            File.WriteAllText(FilePathName, newFile.ToString());

            return Task.CompletedTask;
        }
    }
}