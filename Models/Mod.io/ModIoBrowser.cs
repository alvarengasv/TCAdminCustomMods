using System;
using System.Linq;
using TCAdminCustomMods.Configurations;
using TCAdminCustomMods.Providers;

namespace TCAdminCustomMods.Models.Mod.io
{
    public class ModIoBrowser
    {
        public const string BaseUrl = "https://api.mod.io/";
        
        public static ModIoConfiguration GetModIoConfigurationForCurrentGame()
        {
            var selectedGame = TCAdmin.GameHosting.SDK.Objects.Game.GetSelectedGame();
            var customModBase = CustomModBase.GetCustomModBases().FirstOrDefault(x => x.Name == "Mod.IO");
            if (customModBase == null)
            {
                throw new Exception("Mod.IO is not installed as a Custom Mod Provider");
            }

            var modIoConfiguration = customModBase.GetConfigurationForGame(selectedGame).ToObject<ModIoConfiguration>();
            if (modIoConfiguration == null)
            {
                throw new Exception("Mod.IO configuration is not set for this game.");
            }

            return modIoConfiguration;
        }
    }
}