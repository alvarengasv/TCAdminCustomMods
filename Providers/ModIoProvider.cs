using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Alexr03.Common.Logging;
using Alexr03.Common.Misc.Strings;
using Alexr03.Common.Web.Extensions;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdmin.SDK.Web.References.ServerUtilities;
using TCAdminCustomMods.Configurations;
using TCAdminCustomMods.Models.Generic;
using TCAdminCustomMods.Models.Mod.io;
using TCAdminCustomMods.Models.Mod.io.Mods;

namespace TCAdminCustomMods.Providers
{
    public class ModIoProvider : CustomModProvider
    {
        private readonly Logger _logger = new Logger("ModIoProvider_logger");

        public override bool InstallMod(Service service, GenericMod gameMod)
        {
            try
            {
                var server = Server.GetSelectedServer();
                var modIoConfiguration = ModIoBrowser.GetModIoConfigurationForCurrentGame();
                var modForGame = MMod.GetModForGame(modIoConfiguration.SelectedGameId, int.Parse(gameMod.Id));
                DownloadMod(modForGame);
                var script = GenerateScript(service, modIoConfiguration, true);
                var modJson = JsonConvert.SerializeObject(modForGame);
                var type = gameMod.GetType();
                var variables = new List<JsonSerializedScriptVariable>
                {
                    new JsonSerializedScriptVariable
                    {
                        Name = "Mod",
                        Value = modJson,
                        Type = type.FullNameWithAssembly()
                    }
                };
                
                server.ServerUtilitiesService.ExecuteScript(script, 1, variables.ToArray());
                return true;
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                return false;
            }
        }

        private static void DownloadMod(MMod mod)
        {
            var downloadUrl = mod.Modfile.Download.BinaryUrl;
            var fileNameSavePath = Path.Combine(TCAdmin.SDK.Utility.GetTempPath(), mod.Modfile.Filename);
            using (var webClient = new WebClient())
            {
                webClient.DownloadFile(downloadUrl, fileNameSavePath);
            }
        }

        public override bool UnInstallMod(Service service, GenericMod gameMod)
        {
            try
            {
                var server = Server.GetSelectedServer();
                var modIoConfiguration = ModIoBrowser.GetModIoConfigurationForCurrentGame();
                var modForGame = MMod.GetModForGame(modIoConfiguration.SelectedGameId, int.Parse(gameMod.Id));
                var script = GenerateScript(service, modIoConfiguration, false);
                var modJson = JsonConvert.SerializeObject(modForGame);
                var type = gameMod.GetType();
                server.ServerUtilitiesService.ExecuteScript(script, 1, new[]
                {
                    new JsonSerializedScriptVariable
                    {
                        Name = "Mod",
                        Value = modJson,
                        Type = type.FullNameWithAssembly()
                    }
                });
                return true;
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                return false;
            }
        }

        public override DataSourceResult GetMods(DataSourceRequest request)
        {
            var modIoConfiguration = ModIoBrowser.GetModIoConfigurationForCurrentGame();
            var titleQuery = "";
            var categoryQuery = "";
            var sortQuery = "title";
            var filters = request.GetAllFilterDescriptors();
            var titleFilter = filters.FirstOrDefault(x => x.Member == "Name");
            if (titleFilter != null)
            {
                titleQuery = titleFilter.Value.ToString();
            }

            var categoryFilter = filters.FirstOrDefault(x => x.Member == "Category");
            if (categoryFilter != null)
            {
                categoryQuery = categoryFilter.Value.ToString();
            }

            var mods = MMods.GetModsForGame(modIoConfiguration.SelectedGameId, titleQuery, request.Page, request.PageSize);
            request.Filters = new List<IFilterDescriptor>();
            var dataSourceResult = mods.Data.ToDataSourceResult(request);
            dataSourceResult.Total = mods.ResultTotal;
            dataSourceResult.Data = mods.Data;
            return dataSourceResult;
        }

        public override GenericMod GetMod(string s, ModSearchType modSearchType)
        {
            var modIoConfiguration = ModIoBrowser.GetModIoConfigurationForCurrentGame();
            return MMod.GetModForGame(modIoConfiguration.SelectedGameId, int.Parse(s));
        }
        
        private static string GenerateScript(Service service, ModIoConfiguration modIoConfiguration,
            bool install)
        {
            var scriptId = install ? modIoConfiguration.InstallScriptId : modIoConfiguration.UnInstallScriptId;
            if (scriptId == 0)
            {
                throw new Exception(
                    "Script ID to execute is 0. Contact your hosting provider to correctly setup Custom Mods.");
            }
            return $@"
import clr
clr.AddReference('TCAdmin.GameHosting.SDK')
from TCAdmin.GameHosting.SDK.Objects import Server, Service, Game
Script.AddVariable('ThisService', Service({service.ServiceId}))
Script.AddVariable('ThisServer', Server(ThisService.ServerId))
Script.AddVariable('ThisGame', Game(ThisService.GameId))
Script.AddVariable('FileName', Mod.Modfile.Filename)
Script.Execute(ThisGame.GameId, {scriptId})
";
        }
    }
}