using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Alexr03.Common.TCAdmin.Objects;
using Alexr03.Common.Web.Extensions;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdmin.SDK.Misc;
using TCAdminCustomMods.Configurations;
using TCAdminCustomMods.Models.Curse;
using TCAdminCustomMods.Models.Generic;
using TCAdminCustomMods.Models.Mod.io.Mods;
using Server = TCAdmin.GameHosting.SDK.Objects.Server;

namespace TCAdminCustomMods.Providers
{
    public class CurseProvider : CustomModProvider
    {
        public override bool InstallMod(Service service, GenericMod gameMod)
        {
            List<string> installedmods;
            if (service.Variables.HasValue("Mods"))
                installedmods = new List<string>(service.Variables["Mods"].ToString().Split(','));
            else
                installedmods = new List<string>();

            if (!installedmods.Contains(gameMod.Id))
                installedmods.Add(gameMod.Id);
            service.Variables["Mods"] = string.Join(",", installedmods.ToArray()).Trim().Trim(',');
            service.Save();
            service.Configure();

            return true;
        }

        public override bool UnInstallMod(Service service, GenericMod gameMod)
        {
            var server = new Server(service.ServerId);
            var game = TCAdmin.GameHosting.SDK.Objects.Game.GetSelectedGame();
            var provider = DynamicTypeBase.GetCurrent<CustomModBase>("providerId");
            var config = provider.GetConfigurationForGame(game).ToObject<CurseConfiguration>();
            var fileSystem = server.FileSystemService;
            var files = CurseBrowser.GetFiles(int.Parse(gameMod.Id));
            var plugin = this.GetInstalledPlugins(service).First(p=>p== gameMod.Id || p.StartsWith(gameMod.Id + ":"));
            var parts = plugin.Split(':');
            var version = 0;
            var scriptvars = new TCAdmin.SDK.Database.XmlField();
            scriptvars["CurseGame"] = config.CurseGame;
            scriptvars["CurseModPath"] = config.ModPath;
            scriptvars["ModId"] = int.Parse(gameMod.Id);

            if (config.CurseGame == "432") {
                if (parts.Count() == 2)
                {
                    version = int.Parse(parts[1]);
                }
                else
                {
                    var mod = CurseBrowser.GetMod(int.Parse(gameMod.Id));
                    version = files.FirstOrDefault(x => x.FileName.EndsWith(".jar")).Id;
                }
                var file = files.FirstOrDefault(x => x.FileName.EndsWith(".jar") && x.Id == version);
                if (file == null)
                {
                    throw new NullReferenceException(string.Format("Could not find mod file with version id {0}", version));
                }
                var modsDirectory = FileSystem.CombinePath(server.OperatingSystem, service.RootDirectory, "mods");
                var saveTo = FileSystem.CombinePath(server.OperatingSystem, modsDirectory, file.FileName);

                fileSystem.DeleteFile(saveTo);
            }
            else
            {
                List<string> installedmods;
                if (service.Variables.HasValue("Mods"))
                    installedmods = new List<string>(service.Variables["Mods"].ToString().Split(','));
                else
                    installedmods = new List<string>();

                if (installedmods.Contains(gameMod.Id))
                    installedmods.Remove(gameMod.Id);
                service.Variables["Mods"] = string.Join(",", installedmods.ToArray()).Trim().Trim(',');
                service.Save();
                service.Configure();
            }

            server.GameHostingUtilitiesService.ExecuteEventScripts(service.ServiceId, (int)ServiceEvent.CustomModUninstall, scriptvars.ToString());
            this.RemoveInstalledPlugin(service, string.Format("{0}:{1}", gameMod.Id, version));
            return true;
        }

        public override DataSourceResult GetMods(DataSourceRequest request)
        {
            var service = Service.GetSelectedService();
            var game = TCAdmin.GameHosting.SDK.Objects.Game.GetSelectedGame();
            var provider = DynamicTypeBase.GetCurrent<CustomModBase>("providerId");
            var config = provider.GetConfigurationForGame(game).ToObject<CurseConfiguration>();


            var filters = request.GetAllFilterDescriptors();
            var query = "";
            var titleFilter = filters.FirstOrDefault(x => x.Member == "Name");
            if (titleFilter != null)
            {
                query = titleFilter.Value.ToString();
            }

            var mods = CurseBrowser.Search(query, request.Page, curseGameId: config.CurseGame);
            request.Filters = new List<IFilterDescriptor>();
            var dataSourceResult = mods.ToDataSourceResult(request);
            dataSourceResult.Total = 500;
            dataSourceResult.Data = mods;
            return dataSourceResult;
        }

        public override GenericMod GetMod(string s, ModSearchType modSearchType)
        {
            return CurseBrowser.GetMod(int.Parse(s));
        }

        public override int InstallModWithTask(Service service, GenericMod gameMod)
        {
            throw new NotImplementedException();
        }

        public override int UninstallModWithTask(Service service, GenericMod gameMod)
        {
            throw new NotImplementedException();
        }

        public override bool InstallMod(Service service, GenericMod gameMod, string versionId)
        {
            var game = TCAdmin.GameHosting.SDK.Objects.Game.GetSelectedGame();
            var provider = DynamicTypeBase.GetCurrent<CustomModBase>("providerId");
            var config = provider.GetConfigurationForGame(game).ToObject<CurseConfiguration>();
            var server = new Server(service.ServerId);
            var scriptvars = new TCAdmin.SDK.Database.XmlField();
            scriptvars["CurseGame"] = config.CurseGame;
            scriptvars["CurseModPath"] = config.ModPath;
            scriptvars["ModId"] = int.Parse(gameMod.Id);
            
            if (config.CurseGame == "432") //Minecraft
            {
                var fileSystem = server.FileSystemService;
                var files = CurseBrowser.GetFiles(int.Parse(gameMod.Id));
                var version = int.Parse(versionId);

                var file = files.FirstOrDefault(x => x.FileName.EndsWith(".jar") && x.Id == version);
                if (file == null)
                {
                    throw new NullReferenceException(string.Format("Could not find mod file with version id {0}", version));
                }
                var modsDirectory = FileSystem.CombinePath(server.OperatingSystem, service.RootDirectory, config.ModPath);
                var saveTo = FileSystem.CombinePath(server.OperatingSystem, modsDirectory, file.FileName);
                fileSystem.CreateDirectory(modsDirectory);
                fileSystem.DownloadFile(saveTo, file.DownloadUrl);
            }
            else if(config.CurseGame== "83374") // ARK SA
            {
                List<string> installedmods;
                if (service.Variables.HasValue("Mods"))
                    installedmods = new List<string>(service.Variables["Mods"].ToString().Split(','));
                else
                    installedmods = new List<string>();

                if (!installedmods.Contains(gameMod.Id))
                    installedmods.Add(gameMod.Id);
                service.Variables["Mods"] = string.Join(",", installedmods.ToArray()).Trim().Trim(',');
                service.Save();
                service.Configure();
            }
            else //Palworld (85196) and others
            {
                var fileSystem = server.FileSystemService;
                var files = CurseBrowser.GetFiles(int.Parse(gameMod.Id));
                var version = int.Parse(versionId);

                var file = files.FirstOrDefault(x => x.FileName.EndsWith(".zip") && x.Id == version);
                if (file == null)
                {
                    throw new NullReferenceException(string.Format("Could not find mod file with version id {0}", version));
                }
                var modDirectory = FileSystem.CombinePath(server.OperatingSystem, FileSystem.CombinePath(server.OperatingSystem, service.RootDirectory, config.ModPath), string.Format("{0}-{1}", gameMod.Id, TCAdmin.SDK.Misc.FileSystem.RemoveInvalidFileSystemCharacters(file.DisplayName)));
                var saveTo = FileSystem.CombinePath(server.OperatingSystem, modDirectory, file.FileName);
                fileSystem.CreateDirectory(modDirectory);
                fileSystem.DownloadFile(saveTo, file.DownloadUrl);
                scriptvars["ExtractPath"] = modDirectory;
                scriptvars["ZipFile"] = saveTo;
            }

            server.GameHostingUtilitiesService.ExecuteEventScripts(service.ServiceId, (int)ServiceEvent.CustomModInstall, scriptvars.ToString());

            return true;
        }
    }
}