using System.ComponentModel.DataAnnotations;
using Res = Resources.Settings;

namespace TCAdminCustomMods.Configurations
{
    public class SteamWorkshopConfiguration : CustomModProviderConfiguration
    {
        public override string CustomName { get; set; } = "Steam Workshop";
        public override string CustomIcon { get; set; } = string.Empty;
        [Display(Name = nameof(Res.SteamWorkshopConfiguration.ShowDescription), ResourceType = typeof(Res.SteamWorkshopConfiguration))]
        public bool ShowDescription { get; set; } = true;
        [Display(Name = nameof(Res.SteamWorkshopConfiguration.ShowAuthorAndRating), ResourceType = typeof(Res.SteamWorkshopConfiguration))]
        public bool ShowAuthorAndRating { get; set; } = true;
    }
}