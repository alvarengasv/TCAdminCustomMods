using System.Web.Mvc;
using Alexr03.Common.Misc.Strings;
using Alexr03.Common.TCAdmin.Objects;
using Alexr03.Common.Web.Extensions;
using Newtonsoft.Json;
using TCAdmin.SDK.Web.MVC.Controllers;
using TCAdminCustomMods.Providers;

namespace TCAdminCustomMods.Controllers
{
    public class CustomModsAdminController : BaseController
    {
        public override string CustomGameControllerName { get; } = "CustomModsAdmin";
        
        public ActionResult Index(int id)
        {
            return View();
        }

        [ParentAction("Index")]
        public ActionResult Configure(int id)
        {
            var customModBase = DynamicTypeBase.GetCurrent<CustomModBase>("providerId");
            var game = TCAdmin.GameHosting.SDK.Objects.Game.GetSelectedGame();
            var config = customModBase.GetConfigurationForGame(game);
            this.SetHtmlFieldPrefix(customModBase.Type.Name);
            return View(customModBase.Configuration.View, config.ToObject(customModBase.Configuration.Type));
        }

        [HttpPost]
        public ActionResult Configure(int id, FormCollection formCollection)
        {
            var game = TCAdmin.GameHosting.SDK.Objects.Game.GetSelectedGame();
            var customModProvider = DynamicTypeBase.GetCurrent<CustomModBase>("providerId");
            var bindModel = formCollection.Parse(ControllerContext, customModProvider.Configuration.Type,
                customModProvider.Type.Name);
            customModProvider.SetConfigurationForGame(game, bindModel);

            return customModProvider.SetConfigurationForGame(game, bindModel)
                ? this.SendSuccess("Successfully saved the mod settings")
                : this.SendError("Unable to save custom mod settings!");
        }
    }
}