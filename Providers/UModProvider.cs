using System;
using System.Collections.Generic;
using System.Linq;
using Alexr03.Common.Web.Extensions;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using TCAdmin.SDK.Objects;
using TCAdminCustomMods.Models.Generic;
using TCAdminCustomMods.Models.UMod;
using Service = TCAdmin.GameHosting.SDK.Objects.Service;

namespace TCAdminCustomMods.Providers
{
    public class UModProvider : CustomModProvider
    {
        public override bool InstallMod(Service service, GenericMod gameMod)
        {
            var server = Server.GetSelectedServer();
            var fileSystem = server.FileSystemService;
            var detailedModData = DetailedModData.GetDetailedModData(gameMod.Id);
            fileSystem.CreateDirectory(TCAdmin.SDK.Misc.FileSystem.CombinePath(server.OperatingSystem,
                service.RootDirectory, "oxide", "plugins"));
            var combinePath = TCAdmin.SDK.Misc.FileSystem.CombinePath(server.OperatingSystem, service.RootDirectory,
                "oxide", "plugins", detailedModData.Name);
            combinePath = combinePath.Remove(combinePath.Length - 1, 1) + ".cs";
            fileSystem.DownloadFile(
                combinePath, detailedModData.DownloadUrl);

            return true;
        }

        public override bool UnInstallMod(Service service, GenericMod gameMod)
        {
            var server = Server.GetSelectedServer();
            var fileSystem = server.FileSystemService;
            var detailedModData = DetailedModData.GetDetailedModData(gameMod.Id);
            var combinePath = TCAdmin.SDK.Misc.FileSystem.CombinePath(server.OperatingSystem, service.RootDirectory,
                "oxide", "plugins", detailedModData.Name);
            combinePath = combinePath.Remove(combinePath.Length - 1, 1) + ".cs";
            fileSystem.DeleteFile(combinePath);

            return true;
        }

        public override DataSourceResult GetMods(DataSourceRequest request)
        {
            var titleQuery = "";
            var categoryQuery = "rust";
            var sortQuery = "title";
            var filters = request.GetAllFilterDescriptors();
            var titleFilter = filters.FirstOrDefault(x => x.Member == "Title");
            if (titleFilter != null)
            {
                titleQuery = titleFilter.Value.ToString();
            }

            var categoryFilter = filters.FirstOrDefault(x => x.Member == "Category");
            if (categoryFilter != null)
            {
                categoryQuery = categoryFilter.Value.ToString();
            }

            var sortFilter = filters.FirstOrDefault(x => x.Member == "SortBy");
            if (sortFilter != null)
            {
                sortQuery = sortFilter.Value.ToString();
            }

            var uModBrowser = UModBrowser.Search(titleQuery, request.Page, sort: sortQuery, category: categoryQuery);
            request.Filters = new List<IFilterDescriptor>();
            var dataSourceResult = uModBrowser.Mods.ToDataSourceResult(request);
            dataSourceResult.Total = uModBrowser.Total;
            dataSourceResult.Data = uModBrowser.Mods;
            return dataSourceResult;
        }

        public override GenericMod GetMod(string s, ModSearchType modSearchType)
        {
            return DetailedModData.GetDetailedModData(s);
        }
    }
}