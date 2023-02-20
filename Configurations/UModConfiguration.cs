using System.ComponentModel.DataAnnotations;
using Res = Resources.Settings;

namespace TCAdminCustomMods.Configurations
{
    public class UModConfiguration : CustomModProviderConfiguration
    {
        public override string CustomName { get; set; } = "UMod Mods";
        public override string CustomIcon { get; set; } = string.Empty;
        [Display(Name = nameof(Res.UModConfiguration.Category), ResourceType = typeof(Res.UModConfiguration))]
        public string Category { get; set; } = "rust";
    }
}