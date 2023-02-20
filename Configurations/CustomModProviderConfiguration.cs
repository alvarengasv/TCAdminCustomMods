using System.ComponentModel.DataAnnotations;
using Res = Resources.Settings;

namespace TCAdminCustomMods.Configurations
{
    public class CustomModProviderConfiguration
    {
        [Display(Name = nameof(Res.CustomModProviderConfiguration.Enabled), ResourceType = typeof(Res.CustomModProviderConfiguration))]
        public virtual bool Enabled { get; set; }

        [Display(Name = nameof(Res.CustomModProviderConfiguration.CustomName), ResourceType = typeof(Res.CustomModProviderConfiguration))]
        public virtual string CustomName { get; set; }
        [Display(Name = nameof(Res.CustomModProviderConfiguration.CustomIcon), ResourceType = typeof(Res.CustomModProviderConfiguration))]
        public virtual string CustomIcon { get; set; }
    }
}