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
        private void EnforceRequiredPermission()
        {
            if(Request.QueryString["mod_provider"] != null && Request.QueryString["mod_provider"] == "4")
            {
                this.EnforceFeaturePermission("Workshop");
            }else if(Request.QueryString["providerId"] != null && Request.QueryString["providerId"] == "4")
            {
                this.EnforceFeaturePermission("Workshop");
            }
            else if (Request.Form["providerId"] != null && Request.Form["providerId"] == "4")
            {
                this.EnforceFeaturePermission("Workshop");
            }
            else
            {
                this.EnforceFeaturePermission("ModManager");
            }
        }
        public ActionResult Index(int id)
        {
            EnforceRequiredPermission();
            return View();
        }

        [ParentAction("Index")]
        public ActionResult PluginProvider(int id)
        {
            EnforceRequiredPermission();
            var customModBase = DynamicTypeBase.GetCurrent<CustomModBase>("providerId");
            return View(customModBase.ViewName);
        }

        [ParentAction("Index")]
        public ActionResult GetPlugins(int id, [DataSourceRequest] DataSourceRequest request)
        {
            EnforceRequiredPermission();
            var customModBase = DynamicTypeBase.GetCurrent<CustomModBase>("providerId");
            return Json(customModBase.Create<CustomModProvider>().GetMods(request));
        }

        [HttpGet]
        [ParentAction("Index")]
        public ActionResult GetPlugin(int id, string modId)
        {
            EnforceRequiredPermission();
            var customModBase = DynamicTypeBase.GetCurrent<CustomModBase>("providerId");
            return Json(customModBase.Create<CustomModProvider>().GetMod(modId, ModSearchType.Id), JsonRequestBehavior.AllowGet);
        }

        [ParentAction("Index")]
        [HttpPost]
        public void InstallPlugin(int id, string modId, string versionId = "")
        {
            EnforceRequiredPermission();
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
                var result = false;
                if (string.IsNullOrEmpty(versionId))
                {
                    result = customModProvider.InstallMod(service, genericMod);
                }
                else
                {
                    result = customModProvider.InstallMod(service, genericMod, versionId);
                }
                if (result)
                {
                    if (string.IsNullOrEmpty(versionId))
                    {
                        customModProvider.AddInstalledPlugin(service, modId);
                    }
                    else
                    {
                        customModProvider.AddInstalledPlugin(service, string.Format("{0}:{1}", modId, versionId));
                    }
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
        public ActionResult InstallPluginWithTask(int id, string modId)
        {
            EnforceRequiredPermission();
            this.PrepareAjax();
            var logger = LogManager.Create<CustomModsController>(nameof(InstallPlugin));
            try
            {
                var customModBase = DynamicTypeBase.GetCurrent<CustomModBase>("providerId");
                var customModProvider = customModBase.Create<CustomModProvider>();
                var service = Service.GetSelectedService();
                var genericMod = customModProvider.GetMod(modId, ModSearchType.Id);
                customModProvider.PreInstallMod(service, genericMod);
                var taskid = customModProvider.InstallModWithTask(service, genericMod);
                return Json(new { TaskId = taskid, Status = "success" });
            }
            catch (Exception exception)
            {
                logger.Fatal(exception.ToString());
                return Json(new { TaskId = -1, Status = "error", Message = exception.Message });
            }
        }

        [ParentAction("Index")]
        [HttpPost]
        public void UninstallPlugin(int id, string modId)
        {
            EnforceRequiredPermission();
            this.PrepareAjax();
            var logger = LogManager.Create<CustomModsController>(nameof(UninstallPlugin));
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

        [ParentAction("Index")]
        [HttpPost]
        public ActionResult UninstallPluginWithTask(int id, string modId)
        {
            EnforceRequiredPermission();
            this.PrepareAjax();
            var logger = LogManager.Create<CustomModsController>(nameof(InstallPlugin));
            try
            {
                var customModBase = DynamicTypeBase.GetCurrent<CustomModBase>("providerId");
                var customModProvider = customModBase.Create<CustomModProvider>();
                var service = Service.GetSelectedService();
                var genericMod = customModProvider.GetMod(modId, ModSearchType.Id);
                customModProvider.PreUninstallMod(service, genericMod);
                var taskid = customModProvider.UninstallModWithTask(service, genericMod);
                return Json(new { TaskId = taskid, Status = "success" });
            }
            catch (Exception exception)
            {
                logger.Fatal(exception.ToString());
                return Json(new { TaskId = -1, Status = "error", Message = exception.Message });
            }
        }
    }
}