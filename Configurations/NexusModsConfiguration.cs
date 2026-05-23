using System.ComponentModel.DataAnnotations;
using TCAdminCustomMods.Models.Nexus;
using Res = Resources.Settings;

namespace TCAdminCustomMods.Configurations
{
    public class NexusModsConfiguration : CustomModProviderConfiguration
    {
        public override string CustomName { get; set; } = "Nexus Mods";
        public override string CustomIcon { get; set; } = string.Empty;

        [Display(Name = nameof(Res.NexusModsConfiguration.ApiKey), ResourceType = typeof(Res.NexusModsConfiguration))]
        public string ApiKey { get; set; } = string.Empty;

        [Display(Name = nameof(Res.NexusModsConfiguration.GameDomain), ResourceType = typeof(Res.NexusModsConfiguration))]
        public string GameDomain { get; set; } = string.Empty;

        [Display(Name = nameof(Res.NexusModsConfiguration.ModPath), ResourceType = typeof(Res.NexusModsConfiguration))]
        public string ModPath { get; set; } = string.Empty;

        [Display(Name = nameof(Res.NexusModsConfiguration.DefaultSort), ResourceType = typeof(Res.NexusModsConfiguration))]
        public NexusBrowser.SortField DefaultSort { get; set; } = NexusBrowser.SortField.Endorsements;

        [Display(Name = nameof(Res.NexusModsConfiguration.AllowAdultContent), ResourceType = typeof(Res.NexusModsConfiguration))]
        public bool AllowAdultContent { get; set; } = false;
    }
}
