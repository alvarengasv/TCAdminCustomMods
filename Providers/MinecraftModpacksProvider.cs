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
using TCAdminCustomMods.Tasks.MinecraftModpacks;

namespace TCAdminCustomMods.Providers
{
    public class MinecraftModpacksProvider : CustomModProvider
    {
        public override GenericMod GetMod(string s, ModSearchType modSearchType)
        {
            var mod = MinecraftModpacksBrowser.GetPack(int.Parse(s));
            if (mod.Status != "success")
            {
                mod = MinecraftModpacksBrowser.GetCurseforgePack(int.Parse(s));
            }

            return mod;
        }

        public override DataSourceResult GetMods([DataSourceRequest] DataSourceRequest request)
        {
            var installed = this.GetInstalledPlugins(TCAdmin.GameHosting.SDK.Objects.Service.GetSelectedService());
            var filters = request.GetAllFilterDescriptors();
            var query = string.Empty;
            var termFilter = filters.FirstOrDefault(x => x.Member == "Term") ?? new Kendo.Mvc.FilterDescriptor();
            var sortBy = (filters.FirstOrDefault(x => x.Member == "SortBy") ?? new Kendo.Mvc.FilterDescriptor("SortBy", Kendo.Mvc.FilterOperator.Contains, installed.Count > 0 ? "installed" : "curseforge/1")).Value.ToString();

            //Support for mod files
            if (sortBy.StartsWith("mod/files/"))
            {
                var cursefiles = Models.Curse.CurseBrowser.GetFiles(int.Parse(termFilter.Value.ToString()));
                request.Filters = new List<IFilterDescriptor>();
                var dataSourceResult = cursefiles.ToDataSourceResult(request);
                dataSourceResult.Total = 5000;
                dataSourceResult.Data = cursefiles;
                return dataSourceResult;
            }else if (sortBy.StartsWith("curseforge/"))
            {
                var sort = sortBy.Split('/')[1];
                var cursepacks = Models.Curse.CurseBrowser.Search(query: termFilter.Value != null ? termFilter.Value.ToString() : string.Empty, page: request.Page, pageSize: request.PageSize, sectionId: "4471", sort: sort, sortOrder: sort == "4" ? "asc" : "desc");

                request.Filters = new List<IFilterDescriptor>();
                var dataSourceResult = cursepacks.ToDataSourceResult(request);
                dataSourceResult.Total = 5000;
                dataSourceResult.Data = cursepacks;
                return dataSourceResult;

                //List<MinecraftModpacksBrowser> mods = new List<MinecraftModpacksBrowser>();
                //cursepacks.ForEach(cursepack => {
                //    var mod = new MinecraftModpacksBrowser();
                //    mod.Type = "Curseforge";
                //    mod.Status = "success";
                //    mod.Id = cursepack.Id;
                //    mod.Name = cursepack.Name;
                //    mod.Description = cursepack.Summary;
                //    mod.Synopsis = cursepack.Summary;
                //    mod.Updated = (int)cursepack.DateModified.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                //    mod.Art = new MinecraftModpacksArt[] { new MinecraftModpacksArt() {
                //        Url = cursepack.Logo.ThumbnailUrl
                //    }, new MinecraftModpacksArt() {
                //        Url = cursepack.Links.WebsiteUrl
                //    } };
                //    mods.Add(mod);

                //    var versions = new List<MinecraftModpacksVersion>();
                //    foreach (var cursefile in cursepack.LatestFiles)
                //    {
                //        versions.Add(new MinecraftModpacksVersion()
                //        {
                //            Id = cursefile.Id,
                //            Name = cursefile.DisplayName.Replace(".zip", string.Empty),
                //            Type = "Release",
                //            Updated = (int)cursefile.FileDate.Subtract(new DateTime(1970, 1, 1)).TotalSeconds
                //        });
                //    }
                //    mod.Versions = versions.ToArray();
                //});

                //request.Filters = new List<IFilterDescriptor>();
                //var dataSourceResult = mods.ToDataSourceResult(request);
                //dataSourceResult.Total = 5000;
                //dataSourceResult.Data = mods;
                //return dataSourceResult;
            }
            else
            {
                List<MinecraftModpacksBrowser> mods = null;
                if (sortBy == "installed")
                {
                    mods = new List<MinecraftModpacksBrowser>();
                    foreach (var modpack in installed)
                    {
                        var pack = MinecraftModpacksBrowser.GetPack(int.Parse(modpack.Split(':')[0].Replace("MCMP", string.Empty)));
                        if (pack.Status == "success")
                        {
                            mods.Add(pack);
                        }
                        else
                        {
                            var cursepacks = new List<Models.Curse.CurseBrowser>();
                            cursepacks.Add(Models.Curse.CurseBrowser.GetMod(int.Parse(modpack.Split(':')[0].Replace("MCMP", string.Empty))));
                            request.Filters = new List<IFilterDescriptor>();
                            var curseSourceResult = cursepacks.ToDataSourceResult(request);
                            curseSourceResult.Total = 5000;
                            curseSourceResult.Data = cursepacks;
                            return curseSourceResult;

                            //pack = MinecraftModpacksBrowser.GetCurseforgePack(int.Parse(modpack.Split(':')[0].Replace("MCMP", string.Empty)));
                            //if (pack.Status == "success")
                            //{
                            //    mods.Add(pack);
                            //}
                        }
                    }
                }
                else
                {
                    if (termFilter != null && !string.IsNullOrEmpty(termFilter.Value.ToString()))
                    {
                        sortBy = "modpack/search";
                        query = termFilter.Value.ToString();
                    }

                    mods = MinecraftModpacksBrowser.Search(sortBy, query, request.Page, request.PageSize);
                }

                request.Filters = new List<IFilterDescriptor>();
                var dataSourceResult = mods.ToDataSourceResult(request);
                dataSourceResult.Total = 5000;
                dataSourceResult.Data = mods;
                return dataSourceResult;
            }
        }

        public override bool InstallMod(Service service, GenericMod gameMod)
        {
            throw new NotImplementedException();
        }

        public override int InstallModWithTask(Service service, GenericMod gameMod)
        {
            var game = new TCAdmin.GameHosting.SDK.Objects.Game(service.GameId);
            var providers = CustomModBase.GetCustomModBases();
            var provider = providers.SingleOrDefault(p => p.Id == 3);
            var config = provider.GetConfigurationForGame(game).ToObject<Configurations.MinecraftModpacksConfiguration>();
            var modpack = (MinecraftModpacksBrowser)gameMod;
            var installed = this.GetInstalledPlugins(service);
            if (installed.Count > 0)
            {
                var mpid = installed[0].Replace("MCMP", string.Empty);
                mpid = mpid.Substring(0, mpid.IndexOf(":"));
                var other = MinecraftModpacksBrowser.GetPack(int.Parse(mpid));
                if(other.Id != modpack.Id)
                {
                    if (other.Status == "error")
                    {
                        other = MinecraftModpacksBrowser.GetCurseforgePack(int.Parse(mpid));
                    }
                    throw new Exception(string.Format("Only one modpack can be installed at a time. Please uninstall {0}.", other.Name));
                }
            }
            
            var task = new TCAdmin.TaskScheduler.ModuleApi.TaskInfo();
            task.DisplayName = string.Format("Install {0} on {1}", modpack.Name, service.ConnectionInfo);
            task.CreatedBy = TCAdmin.SDK.Session.GetCurrentUser().UserId;
            task.UserId = service.UserId;
            task.Source = service.GetType().ToString();
            task.SourceId = service.ServiceId.ToString();
            task.RunNow = true;

            var arguments = new ModpackInfo()
            {
                ServiceId = service.ServiceId,
                ModpackId = int.Parse(modpack.Id),
                VersionId = int.Parse(System.Web.HttpContext.Current.Request.Form["version"]),
                Type = System.Web.HttpContext.Current.Request.Form["type"] ?? "ftb",
                ModLoader = System.Web.HttpContext.Current.Request.Form["modLoader"] ?? "auto",
                ModLoaderType = System.Web.HttpContext.Current.Request.Form["modLoaderType"] ?? "",
                GameVersion = System.Web.HttpContext.Current.Request.Form["gameVersion"] ?? "",
                RedirectUrl = System.Web.HttpContext.Current.Request.Form["redirect"],
                JarVariable = config.JarVariableName ?? "customjar",
                CurseApiKey = Models.Curse.CurseBrowser.CURSE_API_KEY
            };
            var taskstep = new TCAdmin.TaskScheduler.ModuleApi.StepInfo();
            taskstep.ModuleId = "b48cfbc9-7acc-4980-89c4-2b6a1f784aa0";
            taskstep.ProcessId = 1;
            taskstep.ServerId = service.ServerId;
            taskstep.DisplayName = string.Empty;
            taskstep.Arguments = TCAdmin.SDK.Misc.ObjectXml.ObjectToXml(arguments);
            task.AddStep(taskstep);

            return task.CreateTask(service.ServerId).TaskId;
        }

        public override bool UnInstallMod(Service service, GenericMod gameMod)
        {
            throw new NotImplementedException();
        }

        public override int UninstallModWithTask(Service service, GenericMod gameMod)
        {
            var provider = new TCAdminCustomMods.Providers.MinecraftModpacksProvider();
            var modpack = (MinecraftModpacksBrowser)gameMod;
            var task = new TCAdmin.TaskScheduler.ModuleApi.TaskInfo();
            task.DisplayName = string.Format("Uninstall {0} on {1}", modpack.Name, service.ConnectionInfo);
            task.CreatedBy = TCAdmin.SDK.Session.GetCurrentUser().UserId;
            task.UserId = service.UserId;
            task.Source = service.GetType().ToString();
            task.SourceId = service.ServiceId.ToString();
            task.RunNow = true;

            var arguments = new ModpackInfo()
            {
                ServiceId = service.ServiceId,
                ModpackId = int.Parse(modpack.Id),
                VersionId = int.Parse(provider.GetInstalledPlugins(service).SingleOrDefault(mp=>mp.StartsWith(string.Format("MCMP{0}:",modpack.Id))).Split(':')[1]),
                Type = System.Web.HttpContext.Current.Request.Form["type"] ?? "ftb",
                RedirectUrl = System.Web.HttpContext.Current.Request.Form["redirect"]
            };
            var taskstep = new TCAdmin.TaskScheduler.ModuleApi.StepInfo();
            taskstep.ModuleId = "b48cfbc9-7acc-4980-89c4-2b6a1f784aa0";
            taskstep.ProcessId = 2;
            taskstep.ServerId = service.ServerId;
            taskstep.DisplayName = string.Empty;
            taskstep.Arguments = TCAdmin.SDK.Misc.ObjectXml.ObjectToXml(arguments);
            task.AddStep(taskstep);

            return task.CreateTask(service.ServerId).TaskId;
        }

        public void AddInstalledPlugin(Service service, int modpackId, int version)
        {
            base.AddInstalledPlugin(service, string.Format("MCMP{0}:{1}", modpackId, version));
        }

        public new void AddInstalledPlugin(Service service, string slug)
        {
            throw new NotSupportedException();
        }

        public void RemoveInstalledPlugin(Service service, int modpackId, int version)
        {
            base.RemoveInstalledPlugin(service, string.Format("MCMP{0}:{1}", modpackId, version));
        }

        public new void RemoveInstalledPlugin(Service service, string slug)
        {
            throw new NotSupportedException();
        }

        public override bool InstallMod(Service service, GenericMod gameMod, string versionId)
        {
            throw new NotImplementedException();
        }
    }
}
