using System.ComponentModel.DataAnnotations;
using Res = Resources.Settings;

namespace TCAdminCustomMods.Configurations
{
    public class MinecraftModpacksConfiguration : CustomModProviderConfiguration
    {
        public override string CustomName { get; set; } = "Minecraft Modpacks";
        public override string CustomIcon { get; set; } = string.Empty;
        [Display(Name = nameof(Res.MinecraftModpacksConfiguration.JarVariableName), ResourceType = typeof(Res.MinecraftModpacksConfiguration))]
        public string JarVariableName { get; set; } = "customjar";
    }
}