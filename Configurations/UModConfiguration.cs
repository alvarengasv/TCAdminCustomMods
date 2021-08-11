namespace TCAdminCustomMods.Configurations
{
    public class UModConfiguration : CustomModProviderConfiguration
    {
        public override string CustomName { get; set; } = "UMod Mods";
        public override string CustomIcon { get; set; } = string.Empty;
        public string Category { get; set; } = "rust";
    }
}