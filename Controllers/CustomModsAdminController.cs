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
        public ActionResult Configure(int id, int gameId)
        {
            var customModBase = DynamicTypeBase.GetCurrent<CustomModBase>();
            var game = new TCAdmin.GameHosting.SDK.Objects.Game(gameId);
            var config = customModBase.GetConfigurationForGame(game);
            this.SetHtmlFieldPrefix(customModBase.Type.Name);
            return View(customModBase.Configuration.View, config.ToObject(customModBase.Configuration.Type));
        }

        [HttpPost]
        public ActionResult Configure(int id, int gameId, FormCollection formCollection)
        {
            var game = new TCAdmin.GameHosting.SDK.Objects.Game(gameId);
            var customModProvider = DynamicTypeBase.GetCurrent<CustomModBase>();
            var bindModel = formCollection.Parse(ControllerContext, customModProvider.Configuration.Type,
                customModProvider.Type.Name);
            customModProvider.SetConfigurationForGame(game, bindModel);

            return customModProvider.SetConfigurationForGame(game, bindModel)
                ? this.SendSuccess("Successfully saved the mod settings")
                : this.SendError("Unable to save custom mod settings!");
        }
    }
}