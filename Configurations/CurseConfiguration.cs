namespace TCAdminCustomMods.Configurations
{
    public class CurseConfiguration : CustomModProviderConfiguration
    {
        public override string CustomName { get; set; } = "Curse Mods";
        public override string CustomIcon { get; set; } = string.Empty;
        public string CurseGame { get; set; } = "432";
        public string ModPath { get; set; } = "mods";
    }
}