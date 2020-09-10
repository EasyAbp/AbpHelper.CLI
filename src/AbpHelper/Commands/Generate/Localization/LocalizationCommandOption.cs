using EasyAbp.AbpHelper.Attributes;

namespace EasyAbp.AbpHelper.Commands.Generate.Localization
{
    public class LocalizationCommandOption : GenerateCommandOption
    {
        [Argument("names", Description = "The localization item names, separated by the space char")]
        public string[] Names { get; set; } = null!;
    }
}