using Newtonsoft.Json;

namespace TCAdminCustomMods.Configurations
{
    public class GeneralConfiguration
    {
        public string CustomName { get; set; } = "Custom Mods Manager";
        
        public static GeneralConfiguration GetConfigurationForGame(TCAdmin.GameHosting.SDK.Objects.Game game)
        {
            var value = game.AppData.GetValue($"__CustomModsModule::General_Config", "{}").ToString();
            return JsonConvert.DeserializeObject<GeneralConfiguration>(value);
        }
        
        public bool SetConfigurationForGame(TCAdmin.GameHosting.SDK.Objects.Game game, GeneralConfiguration obj)
        {
            game.AppData[$"__CustomModsModule::General_Config"] = JsonConvert.SerializeObject(obj);
            return game.Save();
        }
    }
}