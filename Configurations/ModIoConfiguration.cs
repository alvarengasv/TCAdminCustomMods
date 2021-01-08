namespace TCAdminCustomMods.Configurations
{
    public class ModIoConfiguration : CustomModProviderConfiguration
    {
        public override string CustomName { get; set; } = "Mod.IO Mods";

        public string ApiKey { get; set; } = "";

        public int SelectedGameId { get; set; } = 1;

        public int InstallScriptId { get; set; }
        
        public int UnInstallScriptId { get; set; }
    }
}