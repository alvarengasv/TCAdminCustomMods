using Alexr03.Common.Web.Extensions;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdminCustomMods.Models.Generic;
using TCAdminCustomMods.Models.MinecraftModPacks;
using TCAdminCustomMods.Models.UMod;

namespace TCAdminCustomMods.Providers
{
    public class MinecraftModpacksProvider : CustomModProvider
    {
        public override GenericMod GetMod(string s, ModSearchType modSearchType)
        {
            return DetailedModData.GetDetailedModData(s);
        }

        public override DataSourceResult GetMods([DataSourceRequest] DataSourceRequest request)
        {
            var filters = request.GetAllFilterDescriptors();
            var query = string.Empty;
            var termFilter = filters.FirstOrDefault(x => x.Member == "Term");
            var sortBy = (filters.FirstOrDefault(x => x.Member == "SortBy") ?? new Kendo.Mvc.FilterDescriptor("SortBy", Kendo.Mvc.FilterOperator.Contains, "updated")).Value.ToString();

            if (termFilter != null && !string.IsNullOrEmpty(termFilter.Value.ToString()))
            {
                sortBy = "search";
                query = termFilter.Value.ToString();
            }

            var mods = MinecraftModpacksBrowser.Search(sortBy, query, request.Page, 10);
            request.Filters = new List<IFilterDescriptor>();
            var dataSourceResult = mods.ToDataSourceResult(request);
            dataSourceResult.Total = 5000;
            dataSourceResult.Data = mods;
            return dataSourceResult;
        }

        public override bool InstallMod(Service service, GenericMod gameMod)
        {
            throw new NotImplementedException();
        }

        public override bool UnInstallMod(Service service, GenericMod gameMod)
        {
            throw new NotImplementedException();
        }
    }
}
