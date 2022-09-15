using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Alexr03.Common.Web.Extensions;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdmin.SDK.Misc;
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
            throw new NotImplementedException();
        }

        public override bool UnInstallMod(Service service, GenericMod gameMod)
        {
            var server = new Server(service.ServerId);
            var fileSystem = server.FileSystemService;
            var files = CurseBrowser.GetFiles(int.Parse(gameMod.Id));
            var plugin = this.GetInstalledPlugins(service).SingleOrDefault(p=>p== gameMod.Id || p.StartsWith(gameMod.Id + ":"));
            var parts = plugin.Split(':');
            var version = 0;
            if(parts.Count() == 2)
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
            this.RemoveInstalledPlugin(service, string.Format("{0}:{1}", gameMod.Id, version));
            return true;
        }

        public override DataSourceResult GetMods(DataSourceRequest request)
        {
            var filters = request.GetAllFilterDescriptors();
            var query = "";
            var titleFilter = filters.FirstOrDefault(x => x.Member == "Name");
            if (titleFilter != null)
            {
                query = titleFilter.Value.ToString();
            }

            var mods = CurseBrowser.Search(query, request.Page);
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
            var server = new Server(service.ServerId);
            var fileSystem = server.FileSystemService;
            var files = CurseBrowser.GetFiles(int.Parse(gameMod.Id));
            var version = int.Parse(versionId);
            var file = files.FirstOrDefault(x => x.FileName.EndsWith(".jar") && x.Id == version);
            if (file == null)
            {
                throw new NullReferenceException(string.Format("Could not find mod file with version id {0}", version));
            }
            var modsDirectory = FileSystem.CombinePath(server.OperatingSystem, service.RootDirectory, "mods");
            var saveTo = FileSystem.CombinePath(server.OperatingSystem, modsDirectory, file.FileName);
            fileSystem.CreateDirectory(modsDirectory);
            fileSystem.DownloadFile(saveTo, file.DownloadUrl);
            return true;
        }
    }
}