using System;
using System.Web.Mvc;
using Alexr03.Common.Misc.Strings;
using Alexr03.Common.TCAdmin.Extensions;
using Alexr03.Common.TCAdmin.Logging;
using Alexr03.Common.TCAdmin.Objects;
using Kendo.Mvc.UI;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdmin.SDK.Web.MVC.Controllers;
using TCAdminCustomMods.Providers;

namespace TCAdminCustomMods.Controllers
{
    public class CustomModsController : BaseServiceController
    {
        public ActionResult Index(int id)
        {
            this.EnforceFeaturePermission("ModManager");
            return View();
        }

        [ParentAction("Index")]
        public ActionResult PluginProvider(int id)
        {
            this.EnforceFeaturePermission("ModManager");
            var customModBase = DynamicTypeBase.GetCurrent<CustomModBase>("providerId");
            return View(customModBase.ViewName);
        }

        [ParentAction("Index")]
        public ActionResult GetPlugins(int id, [DataSourceRequest] DataSourceRequest request)
        {
            this.EnforceFeaturePermission("ModManager");
            var customModBase = DynamicTypeBase.GetCurrent<CustomModBase>("providerId");
            return Json(customModBase.Create<CustomModProvider>().GetMods(request));
        }

        [ParentAction("Index")]
        [HttpPost]
        public void InstallPlugin(int id, string modId)
        {
            this.EnforceFeaturePermission("ModManager");
            this.PrepareAjax();
            var logger = LogManager.Create<CustomModsController>(nameof(InstallPlugin));
            try
            {
                var customModBase = DynamicTypeBase.GetCurrent<CustomModBase>("providerId");
                var customModProvider = customModBase.Create<CustomModProvider>();
                var service = Service.GetSelectedService();
                var genericMod = customModProvider.GetMod(modId, ModSearchType.Id);
                this.WriteAjaxMessage($"Installing <strong>{genericMod.Name}</strong> via <strong>{customModBase.Name}</strong> provider", logger);
                customModProvider.PreInstallMod(service, genericMod);
                if (customModProvider.InstallMod(service, genericMod))
                {
                    customModProvider.AddInstalledPlugin(service, modId);
                    customModProvider.PostInstallMod(service, genericMod);
                    this.WriteAjaxMessage($"Successfully installed {genericMod.Name}".ToHtml().FontColor("green"), logger);
                    this.WriteAjaxSuccess();
                }
                else
                {
                    this.WriteAjaxMessage($"Could not install {genericMod.Name}".ToHtml().FontColor("red"), logger);
                    this.WriteAjaxError();
                }
            }
            catch (Exception exception)
            {
                logger.Fatal(exception);
                this.WriteAjaxMessage($"Failed to install mod - {exception.Message}", logger);
            }
        }

        [ParentAction("Index")]
        [HttpPost]
        public void UnInstallPlugin(int id, string modId)
        {
            this.EnforceFeaturePermission("ModManager");
            this.PrepareAjax();
            var logger = LogManager.Create<CustomModsController>(nameof(UnInstallPlugin));
            try
            {
                var customModProvider = DynamicTypeBase.GetCurrent<CustomModBase>("providerId").Create<CustomModProvider>();
                var service = Service.GetSelectedService();
                var genericMod = customModProvider.GetMod(modId, ModSearchType.Id);
                this.WriteAjaxMessage($"Uninstalling <strong>{genericMod.Name}</strong>", logger);
                customModProvider.PreUninstallMod(service, genericMod);
                if (customModProvider.UnInstallMod(service, genericMod))
                {
                    customModProvider.RemoveInstalledPlugin(service, modId);
                    customModProvider.PostUninstallMod(service, genericMod);
                    this.WriteAjaxMessage($"Successfully uninstalled {genericMod.Name}".ToHtml().FontColor("green"), logger);
                    this.WriteAjaxSuccess();
                }
                else
                {
                    this.WriteAjaxMessage($"Could not uninstall {genericMod.Name}".ToHtml().FontColor("red"), logger);
                    this.WriteAjaxError();
                }
            }
            catch (Exception exception)
            {
                logger.Fatal(exception);
                this.WriteAjaxMessage($"Failed to uninstall mod - {exception.Message}", logger);
            }
        }
    }
}