using EasyAbp.AbpHelper.Core.Attributes;

namespace EasyAbp.AbpHelper.Core.Commands.Generate.Localization
{
    public class LocalizationCommandOption : GenerateCommandOption
    {
        [Argument("names", Description = "The localization item names, separated by the space char")]
        public string[] Names { get; set; } = null!;
    }
}