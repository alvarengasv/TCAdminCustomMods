using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Alexr03.Common.TCAdmin.Objects;
using Alexr03.Common.Web.Extensions;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdminCustomMods.Configurations;
using TCAdminCustomMods.Models.Generic;
using TCAdminCustomMods.Models.Nexus;
using FileSystem = TCAdmin.SDK.Misc.FileSystem;
using Server = TCAdmin.GameHosting.SDK.Objects.Server;

namespace TCAdminCustomMods.Providers
{
    public class NexusModsProvider : CustomModProvider
    {
        public const string ManifestFileName = "Nexus-Mods.dat";

        public override DataSourceResult GetMods(DataSourceRequest request)
        {
            var config = LoadConfig();
            if (string.IsNullOrEmpty(config.ApiKey) || string.IsNullOrEmpty(config.GameDomain))
            {
                return EmptyResult();
            }

            var httpRequest = HttpContext.Current?.Request;
            var searchText = httpRequest?["searchtext"] ?? string.Empty;
            var category = httpRequest?["category"] ?? string.Empty;
            var adultFlag = httpRequest?["adultContent"];
            var includeAdult = config.AllowAdultContent
                && !string.IsNullOrEmpty(adultFlag)
                && (adultFlag == "true" || adultFlag == "1" || adultFlag == "on");

            var sortField = config.DefaultSort;
            var sortRaw = httpRequest?["sortBy"];
            if (!string.IsNullOrEmpty(sortRaw))
            {
                NexusBrowser.SortField parsed;
                if (Enum.TryParse(sortRaw, true, out parsed))
                {
                    sortField = parsed;
                }
            }

            var pageSize = request.PageSize > 0 ? request.PageSize : 50;
            var page = request.Page > 0 ? request.Page : 1;
            var offset = (page - 1) * pageSize;

            var result = NexusBrowser.Search(
                config.GameDomain,
                searchText,
                sortField,
                category,
                includeAdult,
                offset,
                pageSize,
                config.ApiKey);

            request.Filters = new List<IFilterDescriptor>();
            var dataSourceResult = result.Mods.ToDataSourceResult(request);
            dataSourceResult.Total = result.TotalCount;
            dataSourceResult.Data = result.Mods;
            return dataSourceResult;
        }

        public override GenericMod GetMod(string s, ModSearchType modSearchType)
        {
            var config = LoadConfig();
            return NexusBrowser.GetMod(config.GameDomain, s, config.ApiKey);
        }

        public override bool InstallMod(Service service, GenericMod gameMod)
        {
            throw new InvalidOperationException("Nexus mods require a file (version) selection. Use the version-aware install flow.");
        }

        public override bool InstallMod(Service service, GenericMod gameMod, string versionId)
        {
            int fileId;
            if (!int.TryParse(versionId, out fileId))
            {
                throw new ArgumentException("Invalid Nexus file id: " + versionId);
            }

            var config = LoadConfig();
            if (string.IsNullOrEmpty(config.ApiKey) || string.IsNullOrEmpty(config.GameDomain))
            {
                throw new InvalidOperationException("Nexus Mods provider is not configured. Set the API key and game domain.");
            }

            var nexusMod = gameMod as NexusMod ?? NexusBrowser.GetMod(config.GameDomain, gameMod.Id, config.ApiKey);
            if (nexusMod == null)
            {
                throw new InvalidOperationException("Could not load mod metadata from Nexus.");
            }

            var files = NexusBrowser.GetFiles(config.GameDomain, nexusMod.ModId.ToString(), config.ApiKey);
            var file = files.FirstOrDefault(f => f.FileId == fileId);
            if (file == null)
            {
                throw new InvalidOperationException(string.Format("Nexus file id {0} was not found for mod {1}.", fileId, nexusMod.ModId));
            }

            var archivePaths = NexusBrowser.GetArchiveFilePaths(file.ContentPreviewLink);
            var unsafePath = archivePaths.FirstOrDefault(p => !NexusBrowser.IsSafeRelativePath(p));
            if (unsafePath != null)
            {
                throw new InvalidOperationException(string.Format(
                    "Refusing to install: archive contains an unsafe path '{0}'. Mods may only contain relative paths inside the archive.", unsafePath));
            }

            var server = new Server(service.ServerId);
            var fileSystem = server.FileSystemService;

            var rootDir = service.RootDirectory;
            var extractDir = string.IsNullOrEmpty(config.ModPath)
                ? rootDir
                : FileSystem.CombinePath(server.OperatingSystem, rootDir, config.ModPath);

            var tempDir = FileSystem.CombinePath(server.OperatingSystem, rootDir, ".nexus-tmp");
            var archivePath = FileSystem.CombinePath(server.OperatingSystem, tempDir, file.FileName);

            var downloadLink = NexusBrowser.GetDownloadLink(config.GameDomain, nexusMod.ModId.ToString(), fileId, config.ApiKey);
            if (downloadLink == null || string.IsNullOrEmpty(downloadLink.Uri))
            {
                throw new InvalidOperationException("Could not get a download URL from Nexus. A Premium Nexus API key is required to auto-download mods.");
            }

            // FileSystem.Extract is a Monitor web-service call whose third argument is an XML-serialized
            // VirtualDirectorySecurity that the Monitor deserializes via XmlSerializer. Passing
            // string.Empty made that deserialize throw "Root element is missing." Build it the way
            // TCAdmin's own file manager / plugin installers do: take the service's directory security
            // (boundary = the service root, which contains extractDir) and serialize it with ObjectXml.
            var directorySecurity = service.GetDirectorySecurityForCurrentUser();
            var serializedDirectorySecurity = TCAdmin.SDK.Misc.ObjectXml.ObjectToXml(directorySecurity);

            fileSystem.CreateDirectory(extractDir);
            fileSystem.CreateDirectory(tempDir);

            try
            {
                fileSystem.DownloadFile(archivePath, downloadLink.Uri);

                Console.WriteLine("[NexusMods] FileSystem.Extract parameters:");
                Console.WriteLine($"[NexusMods]   file                        = '{archivePath}'");
                Console.WriteLine($"[NexusMods]   extractPath                 = '{extractDir}'");
                Console.WriteLine($"[NexusMods]   serializedDirectorySecurity length = {(serializedDirectorySecurity?.Length ?? 0)} " +
                                  $"(empty={string.IsNullOrEmpty(serializedDirectorySecurity)})");
                Console.WriteLine($"[NexusMods]   server.OperatingSystem      = {server.OperatingSystem}");
                Console.WriteLine($"[NexusMods]   archive exists on target    = {fileSystem.FileExists(archivePath)}");

                fileSystem.Extract(archivePath, extractDir, serializedDirectorySecurity);
            }
            finally
            {
                try
                {
                    if (fileSystem.FileExists(archivePath)) fileSystem.DeleteFile(archivePath);
                }
                catch
                {
                    // best effort cleanup
                }
            }

            var entries = InstalledModsRegistry.Load<NexusInstalledMod>(service, ManifestFileName);
            entries.RemoveAll(e => string.Equals(e.ModId, nexusMod.ModId.ToString(), StringComparison.OrdinalIgnoreCase));
            entries.Add(NexusInstalledMod.FromMod(nexusMod, fileId, archivePaths));
            InstalledModsRegistry.Save(service, ManifestFileName, entries);

            var scriptvars = new TCAdmin.SDK.Database.XmlField();
            // Standardized cross-provider variable: every provider exposes the mod id as
            // [ModId] in the Custom Mod Install/Uninstall event scripts. The NexusModId /
            // NexusFileId aliases are kept for Nexus-specific scripts (the file id has no
            // cross-provider equivalent).
            scriptvars["ModId"] = gameMod.Id;
            scriptvars["NexusGameDomain"] = config.GameDomain;
            scriptvars["NexusModId"] = nexusMod.ModId;
            scriptvars["NexusFileId"] = fileId;
            scriptvars["ExtractPath"] = extractDir;
            server.GameHostingUtilitiesService.ExecuteEventScripts(service.ServiceId,
                (int)ServiceEvent.CustomModInstall, scriptvars.ToString());

            return true;
        }

        public override bool UnInstallMod(Service service, GenericMod gameMod)
        {
            var config = LoadConfig();
            var server = new Server(service.ServerId);
            var fileSystem = server.FileSystemService;

            var rootDir = service.RootDirectory;
            var extractDir = string.IsNullOrEmpty(config.ModPath)
                ? rootDir
                : FileSystem.CombinePath(server.OperatingSystem, rootDir, config.ModPath);

            var entries = InstalledModsRegistry.Load<NexusInstalledMod>(service, ManifestFileName);
            var entry = entries.FirstOrDefault(e => string.Equals(e.ModId, gameMod.Id, StringComparison.OrdinalIgnoreCase));

            var paths = new List<string>();
            if (entry != null && entry.InstalledFiles != null && entry.InstalledFiles.Count > 0)
            {
                paths = entry.InstalledFiles;
            }
            else if (entry != null && !string.IsNullOrEmpty(config.ApiKey) && !string.IsNullOrEmpty(config.GameDomain))
            {
                try
                {
                    var files = NexusBrowser.GetFiles(config.GameDomain, entry.ModId, config.ApiKey);
                    var file = files.FirstOrDefault(f => f.FileId == entry.FileId);
                    if (file != null)
                    {
                        paths = NexusBrowser.GetArchiveFilePaths(file.ContentPreviewLink);
                    }
                }
                catch
                {
                    // fall through — paths empty, registry-only cleanup below
                }
            }

            foreach (var rel in paths)
            {
                if (!NexusBrowser.IsSafeRelativePath(rel))
                {
                    // Defensive: skip anything that doesn't look like a safe relative path.
                    continue;
                }
                try
                {
                    var absolute = FileSystem.CombinePath(server.OperatingSystem, extractDir, rel);
                    if (fileSystem.FileExists(absolute))
                    {
                        fileSystem.DeleteFile(absolute);
                    }
                }
                catch
                {
                    // best-effort per file
                }
            }

            // Prune the folders the install created once their files are gone. Walk each
            // archive directory from the deepest level upward; DeleteEmptyDirectoryStructure
            // only removes empty directories and stops at the first non-empty one (and never
            // touches the boundary), so folders still holding another mod's files — or the
            // mod path / service root — are preserved.
            foreach (var relDir in GetAncestorDirectories(paths.Where(NexusBrowser.IsSafeRelativePath)))
            {
                try
                {
                    var absDir = FileSystem.CombinePath(server.OperatingSystem, extractDir, relDir);
                    fileSystem.DeleteEmptyDirectoryStructure(absDir, extractDir);
                }
                catch
                {
                    // best-effort per directory
                }
            }

            try
            {
                if (!string.Equals(extractDir, rootDir, StringComparison.OrdinalIgnoreCase))
                {
                    fileSystem.DeleteEmptyDirectoryStructure(extractDir, rootDir);
                }
            }
            catch
            {
                // best effort
            }

            var scriptvars = new TCAdmin.SDK.Database.XmlField();
            scriptvars["ModId"] = gameMod.Id;
            scriptvars["NexusGameDomain"] = config.GameDomain;
            scriptvars["NexusModId"] = gameMod.Id;
            if (entry != null)
            {
                scriptvars["NexusFileId"] = entry.FileId;
            }
            scriptvars["ExtractPath"] = extractDir;
            server.GameHostingUtilitiesService.ExecuteEventScripts(service.ServiceId,
                (int)ServiceEvent.CustomModUninstall, scriptvars.ToString());

            return true;
        }

        public override int InstallModWithTask(Service service, GenericMod gameMod)
        {
            throw new NotImplementedException();
        }

        public override int UninstallModWithTask(Service service, GenericMod gameMod)
        {
            throw new NotImplementedException();
        }

        public override void RecordInstalled(Service service, GenericMod gameMod, string versionId)
        {
            // Manifest is upserted inside InstallMod once the install succeeds.
        }

        public override void RecordUninstalled(Service service, string modId)
        {
            var entries = InstalledModsRegistry.Load<NexusInstalledMod>(service, ManifestFileName);
            var removed = entries.RemoveAll(e => string.Equals(e.ModId, modId, StringComparison.OrdinalIgnoreCase));
            if (removed > 0)
            {
                InstalledModsRegistry.Save(service, ManifestFileName, entries);
            }
        }

        public override DataSourceResult GetInstalledMods(Service service, DataSourceRequest request)
        {
            var config = LoadConfig();

            var modIds = GetInstalledPlugins(service)
                .Select(p => (p ?? string.Empty).Split(':')[0])
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var entries = InstalledModsRegistry.Load<NexusInstalledMod>(service, ManifestFileName);
            var changed = false;

            var idSet = new HashSet<string>(modIds, StringComparer.OrdinalIgnoreCase);
            var stale = entries.RemoveAll(e => !idSet.Contains(e.ModId ?? string.Empty));
            if (stale > 0) changed = true;

            var knownSet = new HashSet<string>(entries.Select(e => e.ModId ?? string.Empty), StringComparer.OrdinalIgnoreCase);
            var missing = modIds.Where(id => !knownSet.Contains(id)).ToList();

            if (missing.Count > 0 && !string.IsNullOrEmpty(config.ApiKey) && !string.IsNullOrEmpty(config.GameDomain))
            {
                var fetched = new ConcurrentBag<NexusInstalledMod>();
                Parallel.ForEach(missing, new ParallelOptions { MaxDegreeOfParallelism = 8 }, id =>
                {
                    try
                    {
                        var mod = NexusBrowser.GetMod(config.GameDomain, id, config.ApiKey);
                        if (mod == null) return;
                        var fileId = ResolveFileIdForInstalled(service, id);

                        var paths = new List<string>();
                        if (fileId > 0)
                        {
                            var files = NexusBrowser.GetFiles(config.GameDomain, id, config.ApiKey);
                            var file = files.FirstOrDefault(f => f.FileId == fileId);
                            if (file != null)
                            {
                                paths = NexusBrowser.GetArchiveFilePaths(file.ContentPreviewLink)
                                    .Where(NexusBrowser.IsSafeRelativePath)
                                    .ToList();
                            }
                        }

                        fetched.Add(NexusInstalledMod.FromMod(mod, fileId, paths));
                    }
                    catch
                    {
                        // skip — will retry next time
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

        private NexusModsConfiguration LoadConfig()
        {
            var game = TCAdmin.GameHosting.SDK.Objects.Game.GetSelectedGame();
            var provider = DynamicTypeBase.GetCurrent<CustomModBase>("providerId");
            return provider.GetConfigurationForGame(game).ToObject<NexusModsConfiguration>();
        }

        private int ResolveFileIdForInstalled(Service service, string modId)
        {
            var match = GetInstalledPlugins(service)
                .FirstOrDefault(p => p != null && p.StartsWith(modId + ":", StringComparison.OrdinalIgnoreCase));
            if (match == null) return 0;
            int fileId;
            return int.TryParse(match.Split(':').Skip(1).FirstOrDefault(), out fileId) ? fileId : 0;
        }

        /// <summary>
        /// Returns every distinct directory (relative to the extract root, '/'-normalised)
        /// that is an ancestor of one of the given archive-relative file paths, ordered
        /// deepest-first. These are exactly the folders the install may have created, so
        /// they can be checked for emptiness and pruned on uninstall.
        /// </summary>
        private static IEnumerable<string> GetAncestorDirectories(IEnumerable<string> relativeFilePaths)
        {
            var dirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var rel in relativeFilePaths)
            {
                if (string.IsNullOrEmpty(rel)) continue;
                var normalized = rel.Replace('\\', '/').TrimEnd('/');
                var slash = normalized.LastIndexOf('/');
                while (slash > 0)
                {
                    var dir = normalized.Substring(0, slash);
                    // Once a directory is already known, so are all of its ancestors.
                    if (!dirs.Add(dir)) break;
                    slash = dir.LastIndexOf('/');
                }
            }
            return dirs.OrderByDescending(d => d.Length);
        }

        private static DataSourceResult EmptyResult()
        {
            return new DataSourceResult { Data = new List<object>(), Total = 0 };
        }
    }
}
