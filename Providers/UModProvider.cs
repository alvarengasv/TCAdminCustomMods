using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alexr03.Common.Web.Extensions;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdmin.SDK.Objects;
using TCAdminCustomMods.Models.Generic;
using TCAdminCustomMods.Models.Mod.io.Mods;
using TCAdminCustomMods.Models.UMod;
using Service = TCAdmin.GameHosting.SDK.Objects.Service;

namespace TCAdminCustomMods.Providers
{
    public class UModProvider : CustomModProvider
    {
        public const string ManifestFileName = "uMod-Mods.dat";

        public override bool InstallMod(Service service, GenericMod gameMod)
        {
            var server = new TCAdmin.GameHosting.SDK.Objects.Server(service.ServerId);
            var fileSystem = server.FileSystemService;
            var detailedModData = DetailedModData.GetDetailedModData(gameMod.Id);
            fileSystem.CreateDirectory(TCAdmin.SDK.Misc.FileSystem.CombinePath(server.OperatingSystem,
                service.RootDirectory, "oxide", "plugins"));
            var combinePath = TCAdmin.SDK.Misc.FileSystem.CombinePath(server.OperatingSystem, service.RootDirectory,
                "oxide", "plugins", detailedModData.Name + ".cs");
            fileSystem.DownloadFile(
                combinePath, detailedModData.DownloadUrl);

            var scriptvars = new TCAdmin.SDK.Database.XmlField();
            scriptvars["ModId"] = gameMod.Id;
            server.GameHostingUtilitiesService.ExecuteEventScripts(service.ServiceId, (int)ServiceEvent.CustomModInstall, scriptvars.ToString());

            return true;
        }

        public override bool UnInstallMod(Service service, GenericMod gameMod)
        {
            var server = new TCAdmin.GameHosting.SDK.Objects.Server(service.ServerId);
            var fileSystem = server.FileSystemService;
            var detailedModData = DetailedModData.GetDetailedModData(gameMod.Id);
            var combinePath = TCAdmin.SDK.Misc.FileSystem.CombinePath(server.OperatingSystem, service.RootDirectory,
                "oxide", "plugins", detailedModData.Name + ".cs");
            fileSystem.DeleteFile(combinePath);

            var scriptvars = new TCAdmin.SDK.Database.XmlField();
            scriptvars["ModId"] = gameMod.Id;
            server.GameHostingUtilitiesService.ExecuteEventScripts(service.ServiceId, (int)ServiceEvent.CustomModUninstall, scriptvars.ToString());

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
            throw new NotImplementedException();
        }

        public override void RecordInstalled(Service service, GenericMod gameMod, string versionId)
        {
            var detailed = gameMod as DetailedModData ?? DetailedModData.GetDetailedModData(gameMod.Id);
            if (detailed == null) return;

            var entries = InstalledModsRegistry.Load<UModInstalledMod>(service, ManifestFileName);
            entries.RemoveAll(e => string.Equals(e.Slug, gameMod.Id, StringComparison.OrdinalIgnoreCase));
            var entry = UModInstalledMod.FromDetailedModData(detailed);
            entry.Slug = gameMod.Id;
            entries.Add(entry);
            InstalledModsRegistry.Save(service, ManifestFileName, entries);
        }

        public override void RecordUninstalled(Service service, string modId)
        {
            var entries = InstalledModsRegistry.Load<UModInstalledMod>(service, ManifestFileName);
            var removed = entries.RemoveAll(e => string.Equals(e.Slug, modId, StringComparison.OrdinalIgnoreCase));
            if (removed > 0)
            {
                InstalledModsRegistry.Save(service, ManifestFileName, entries);
            }
        }

        public override DataSourceResult GetInstalledMods(Service service, DataSourceRequest request)
        {
            var slugs = GetInstalledPlugins(service)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var entries = InstalledModsRegistry.Load<UModInstalledMod>(service, ManifestFileName);
            var changed = false;

            var slugSet = new HashSet<string>(slugs, StringComparer.OrdinalIgnoreCase);
            var stale = entries.RemoveAll(e => !slugSet.Contains(e.Slug ?? string.Empty));
            if (stale > 0) changed = true;

            var knownSet = new HashSet<string>(entries.Select(e => e.Slug ?? string.Empty), StringComparer.OrdinalIgnoreCase);
            var missing = slugs.Where(s => !knownSet.Contains(s)).ToList();
            if (missing.Count > 0)
            {
                var fetched = new ConcurrentBag<UModInstalledMod>();
                Parallel.ForEach(missing, new ParallelOptions { MaxDegreeOfParallelism = 8 }, slug =>
                {
                    try
                    {
                        var data = DetailedModData.GetDetailedModData(slug);
                        if (data == null) return;
                        var entry = UModInstalledMod.FromDetailedModData(data);
                        entry.Slug = slug;
                        fetched.Add(entry);
                    }
                    catch
                    {
                        // skip on individual failure; entry will be retried next load
                    }
                });

                if (fetched.Count > 0)
                {
                    entries.AddRange(fetched);
                    changed = true;
                }
            }

            if (changed) InstalledModsRegistry.Save(service, ManifestFileName, entries);

            request.Filters = new List<IFilterDescriptor>();
            var dataSourceResult = entries.ToDataSourceResult(request);
            dataSourceResult.Total = entries.Count;
            dataSourceResult.Data = entries;
            return dataSourceResult;
        }
    }
}