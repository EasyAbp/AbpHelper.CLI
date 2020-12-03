using System.Linq;

namespace EasyAbp.AbpHelper.Models
{
    public class ProjectInfo
    {
        public ProjectInfo(string baseDirectory, string fullName, TemplateType templateType, UIFramework uiFramework, bool tiered)
        {
            BaseDirectory = baseDirectory;
            TemplateType = templateType;
            UIFramework = uiFramework;
            Tiered = tiered;
            FullName = fullName;
        }

        public string BaseDirectory { get; }
        public string FullName { get; }
        public string Name => FullName.Split('.').Last();
        public TemplateType TemplateType { get; }
        public UIFramework UIFramework { get; }
        public bool Tiered { get; }

        public override string ToString()
        {
            return $"{nameof(BaseDirectory)}: {BaseDirectory}, {nameof(FullName)}: {FullName}, {nameof(Name)}: {Name}, {nameof(TemplateType)}: {TemplateType}, {nameof(UIFramework)}: {UIFramework}, {nameof(Tiered)}: {Tiered}";
        }
    }

    public enum TemplateType
    {
        Application,
        Module
    }

    public enum UIFramework
    {
        None,
        RazorPages,
        Angular,
        Blazor
    }
}