using System.Linq;

namespace DosSEdo.AbpHelper.Models
{
    public class ProjectInfo
    {
        public ProjectInfo(string baseDirectory, string fullName, TemplateType templateType, UiFramework uiFramework, bool tiered)
        {
            BaseDirectory = baseDirectory;
            TemplateType = templateType;
            UiFramework = uiFramework;
            Tiered = tiered;
            FullName = fullName;
        }

        public string BaseDirectory { get; }
        public string FullName { get; }
        public string Name => FullName.Split('.').Last();
        public TemplateType TemplateType { get; }
        public UiFramework UiFramework { get; }
        public bool Tiered { get; }

        public override string ToString()
        {
            return $"{nameof(BaseDirectory)}: {BaseDirectory}, {nameof(FullName)}: {FullName}, {nameof(Name)}: {Name}, {nameof(TemplateType)}: {TemplateType}, {nameof(UiFramework)}: {UiFramework}, {nameof(Tiered)}: {Tiered}";
        }
    }

    public enum TemplateType
    {
        Application,
        Module
    }

    public enum UiFramework
    {
        None,
        RazorPages,
        Angular
    }
}