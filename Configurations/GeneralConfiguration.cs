using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Res = Resources.Settings;

namespace TCAdminCustomMods.Configurations
{
    public class GeneralConfiguration
    {
        [Display(Name = nameof(Res.GeneralModConfiguration.SingleIcon), ResourceType = typeof(Res.GeneralModConfiguration))]
        public bool SingleIcon { get; set; } = true;
        [Display(Name = nameof(Res.GeneralModConfiguration.CustomName), ResourceType = typeof(Res.GeneralModConfiguration))]
        public string CustomName { get; set; } = "Custom Mods Manager";
        [Display(Name = nameof(Res.GeneralModConfiguration.CustomIcon), ResourceType = typeof(Res.GeneralModConfiguration))]
        public string CustomIcon { get; set; } = "";
        
        public static GeneralConfiguration GetConfigurationForGame(TCAdmin.GameHosting.SDK.Objects.Game game)
        {
            var value = game.AppData.GetValue($"__CustomModsModule::General_Config", "{\"SingleIcon\":false}").ToString();
            return JsonConvert.DeserializeObject<GeneralConfiguration>(value);
        }
        
        public bool SetConfigurationForGame(TCAdmin.GameHosting.SDK.Objects.Game game, GeneralConfiguration obj)
        {
            game.AppData[$"__CustomModsModule::General_Config"] = JsonConvert.SerializeObject(obj);
            return game.Save();
        }
    }
}