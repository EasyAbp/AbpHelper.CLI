using System;
using System.Linq;

namespace EasyAbp.AbpHelper.Core.Models
{
    public class ProjectInfo
    {
        public ProjectInfo(string baseDirectory, string fullName, TemplateType templateType, UiFramework uiFramework,
            bool tiered, MapperType mapperType = MapperType.AutoMapper, string? name = null)
        {
            BaseDirectory = baseDirectory;
            TemplateType = templateType;
            UiFramework = uiFramework;
            Tiered = tiered;
            MapperType = mapperType;
            FullName = fullName;
            Name = name ?? FullName.Split('.').Last();
        }

        public string BaseDirectory { get; }
        public string FullName { get; }
        public string Name { get; }
        public TemplateType TemplateType { get; }
        public UiFramework UiFramework { get; }
        public bool Tiered { get; }
        public MapperType MapperType { get; }

        public override string ToString()
        {
            return
                $"{nameof(BaseDirectory)}: {BaseDirectory}, {nameof(FullName)}: {FullName}, {nameof(Name)}: {Name}, {nameof(TemplateType)}: {TemplateType}, {nameof(UiFramework)}: {UiFramework}, {nameof(Tiered)}: {Tiered},{nameof(MapperType)}: {MapperType}";
        }
    }

    public enum MapperType
    {
        None = 0,
        AutoMapper = 1,
        Mapperly = 2,
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