using System.Linq;
using TCAdminCustomMods.Configurations;
using TCAdminCustomMods.Providers;

namespace TCAdminCustomMods
{
    public class IconManager : TCAdmin.Interfaces.Web.IconManager
    {
        public bool CanDisplayIcon(string moduleId, int pageId, int iconId)
        {
            var game = TCAdmin.GameHosting.SDK.Objects.Game.GetSelectedGame();
            return CustomModBase.GetCustomModBases()
                .Select(customModBase =>
                    customModBase.GetConfigurationForGame(game).ToObject<CustomModProviderConfiguration>())
                .Any(config => config != null && config.Enabled);
        }
    }
}