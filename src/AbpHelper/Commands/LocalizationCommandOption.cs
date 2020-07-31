using EasyAbp.AbpHelper.Attributes;

namespace EasyAbp.AbpHelper.Commands
{
    public class LocalizationCommandOption : CommandOptionsBase
    {
        [Argument("names", Description = "The localization item names, separated by the space char")]
        public string[] Names { get; set; } = null!;
    }
}