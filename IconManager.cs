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
            var generalConfig = GeneralConfiguration.GetConfigurationForGame(game);
            var providerid = iconId - 4890;

            if (generalConfig.SingleIcon)
            {
                return providerid ==1 && CustomModBase.GetCustomModBases()
                    .Select(customModBase =>
                        customModBase.GetConfigurationForGame(game).ToObject<CustomModProviderConfiguration>())
                    .Any(config => config != null && config.Enabled);
            }

            var providers = CustomModBase.GetCustomModBases();

            var provider = providers.SingleOrDefault(p => p.Id == providerid);

            if (provider != null)
            {
                var config = provider.GetConfigurationForGame(game).ToObject<CustomModProviderConfiguration>();
                return config != null && config.Enabled;
            }

            return false;
        }
    }
}