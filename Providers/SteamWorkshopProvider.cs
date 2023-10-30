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
using TCAdminCustomMods.Models.SteamWorkshop;

namespace TCAdminCustomMods.Providers
{
    public class SteamWorkshopProvider : CustomModProvider
    {
        public override GenericMod GetMod(string s, ModSearchType modSearchType)
        {
            var parts = s.Split(':');
            var workshopmod = new SteamWorkshopFile
            {
                Id = parts[1],
                Name = parts[0] == "collections"? "Workshop Collection" : "Workshop File",
                IsCollection = parts[0]=="collections"
            };

            var fileid = ulong.Parse(parts[1]);
            if (fileid > 0)
            {
                workshopmod.FileData = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(TCAdmin.Helper.Steam.WorkshopBrowser.GetFileDetails(fileid));

                if (workshopmod.FileData.response == null || workshopmod.FileData.response.publishedfiledetails == null)
                {
                    throw new Exception("publishedfiledetails not found");
                }
                else
                {
                    workshopmod.Name = workshopmod.FileData.response.publishedfiledetails[0].title;
                }
            }
            else
            {
                workshopmod.Name = "batch";
            }

            return workshopmod;
        }

        public override DataSourceResult GetMods([DataSourceRequest] DataSourceRequest request)
        {
            var total = 5000;
            var service = TCAdmin.GameHosting.SDK.Objects.Service.GetSelectedService();
            var game = TCAdmin.GameHosting.SDK.Objects.Game.GetSelectedGame();
            var wb = new TCAdmin.Helper.Steam.WorkshopBrowser(game.Steam.SteamStoreGameId);
            if (game.Steam.SteamStoreGameId == 730 && game.Steam.SteamGameType.IndexOf("730") != -1)
            {
                wb.RequiredTags = "CS2";
            }
            wb.LoadValuesFromQueryString();
            var collections = System.Web.HttpContext.Current.Request.Form["section"] != null && System.Web.HttpContext.Current.Request.Form["section"] == "collections" && game.Steam.WorkshopCollectionsEnabled;
            List<TCAdmin.Helper.Steam.WorkshopItem> files;
            var installedwsfiles = TCAdmin.GameHosting.SDK.Objects.ServiceWorkshopFile.GetServiceFileIds(service.ServiceId).Cast<TCAdmin.GameHosting.SDK.Objects.ServiceWorkshopFile>().Where(ws => (ws.IsCollection & collections)|| (!ws.IsCollection & !collections));

            switch (System.Web.HttpContext.Current.Request.Form["content"])
            {
                case "all":
                    files = wb.Parse(game.Steam.AllowUnlistedSearch, game.Steam.WorkshopCollectionsEnabled && collections, true);
                    foreach (var file in files)
                    {
                        TCAdmin.GameHosting.SDK.Objects.ServiceWorkshopFile wsfile = (ServiceWorkshopFile)installedwsfiles.SingleOrDefault(ws => ws.ServiceId == service.ServiceId && ws.FileId == file.FileId);
                        if (wsfile != null)
                        {
                            file.Installed = true;
                            file.UpdateAvailable = wsfile.UpdateAvailable;
                        }
                    }
                    break;
                case "installed":
                case "updatable":
                    total = installedwsfiles.Count();
                    var pagesize = int.Parse(System.Web.HttpContext.Current.Request.Form["pageSize"]);
                    var skip = (int.Parse(System.Web.HttpContext.Current.Request.Form["page"]) - 1) * pagesize;
                    var onlyupdatable = System.Web.HttpContext.Current.Request.Form["content"] == "updatable";
                    files = new List<TCAdmin.Helper.Steam.WorkshopItem>();
                    foreach (TCAdmin.GameHosting.SDK.Objects.ServiceWorkshopFile installedwsfile in installedwsfiles)
                    {
                        if (onlyupdatable && !installedwsfile.UpdateAvailable)
                            continue;

                        if (collections && !installedwsfile.IsCollection)
                            continue;

                        if (!collections && installedwsfile.IsCollection)
                            continue;

                        dynamic jsondata = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(installedwsfile.FileDetails);
                        var ws = new TCAdmin.Helper.Steam.WorkshopItem();
                        ws.FileId = installedwsfile.FileId;
                        ws.Installed = true;
                        ws.UpdateAvailable = installedwsfile.UpdateAvailable;
                        ws.PreviewImage = jsondata.preview_url == null ? "/Aspx/Interface/GameHosting/Images/NoPreviewUnavailable.png" : jsondata.preview_url;
                        ws.Title = jsondata.title == null ? "No Title" : jsondata.title;
                        ws.Author = string.Empty;
                        ws.AuthorUrl = string.Empty;
                        ws.RatingImage = string.Empty;
                        ws.ExtendedInfo.Description = jsondata.description == null ? string.Empty : jsondata.description;
                        ws.Url = string.Format("https://steamcommunity.com/sharedfiles/filedetails/?id={0}&searchtext=", ws.FileId);
                        files.Add(ws);
                    }

                    files = new List<TCAdmin.Helper.Steam.WorkshopItem>(files.Skip(skip).Take(pagesize));

                    break;
            default:
                    throw new NotImplementedException(System.Web.HttpContext.Current.Request.Form["content"]);
            }

            request.Filters = new List<IFilterDescriptor>();
            var dataSourceResult = files.ToDataSourceResult(request);
            dataSourceResult.Total = total;
            dataSourceResult.Data = files;
            return dataSourceResult;
        }

        public override bool InstallMod(Service service, GenericMod gameMod)
        {
            throw new NotImplementedException();
        }

        public override int InstallModWithTask(Service service, GenericMod gameMod)
        {
            var modinfo = (SteamWorkshopFile)gameMod;
            if (modinfo.IsCollection)
            {
                if (TCAdmin.GameHosting.SDK.Objects.Game.GetSelectedGame().Steam.WorkshopCollectionsEnabled)
                {
                    return InstallCollection(service, modinfo);
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                return InstallFile(service, modinfo);
            }
        }

        private int InstallFile(Service service, SteamWorkshopFile gameMod)
        {
            var task = new TCAdmin.TaskScheduler.ModuleApi.TaskInfo();
            var installdeps = false;
            var isbatch = System.Web.HttpContext.Current.Request.Form["batchAction"] != null;
            var isupdate = (System.Web.HttpContext.Current.Request.Form["update"] != null && System.Web.HttpContext.Current.Request.Form["update"] == "true") || (isbatch && System.Web.HttpContext.Current.Request.Form["batchAction"] == "update");
            var isbatchreinstall = isbatch && System.Web.HttpContext.Current.Request.Form["batchAction"] == "reinstall";
            var currentcontent = System.Web.HttpContext.Current.Request.Form["content"]; //values are all, installed, updatable
            var arguments = new TCAdmin.SDK.Database.XmlField();
            arguments.SetValue("WorkshopInstall.ServiceId", service.ServiceId);
            arguments.SetValue("WorkshopInstall.Redirect", string.Format("~/Service/Workshop/{0}", service.ServiceId));
            arguments.SetValue("WorkshopInstall.Update", isupdate);

            if (isupdate)
            {
                if (isbatch)
                {
                    task.DisplayName = String.Format("Batch update Workshop files on {0}", service.ConnectionInfo);
                }
                else
                {
                    task.DisplayName = String.Format("Update Workshop file {0} on {1}", gameMod.Id, service.ConnectionInfo);
                }
            }
            else
            {
                task.DisplayName = String.Format("Install Workshop file {0} on {1}", gameMod.Id, service.ConnectionInfo);
                installdeps = true;
            }

            task.CreatedBy = TCAdmin.SDK.Session.GetCurrentUser().UserId;
            task.UserId = service.UserId;
            task.Source = service.GetType().ToString();
            task.SourceId = service.ServiceId.ToString();
            task.RunNow = true;

            if (installdeps)
            {
                var addeddeps = new List<ulong>();
                TCAdmin.SDK.LogManager.Write(string.Format("Getting requirements for file id {0}...", gameMod.Id), TCAdmin.Interfaces.Logging.LogType.Information);
                foreach (var depfile in TCAdmin.Helper.Steam.WorkshopBrowser.GetFileRequirements(ulong.Parse(gameMod.Id)))
                {
                    //Make sure it's not installed already
                    var wsfile = new TCAdmin.GameHosting.SDK.Objects.ServiceWorkshopFile();
                    wsfile.ServiceId = service.ServiceId;
                    wsfile.FileId = (uint)depfile;
                    if (!wsfile.Find())
                    {
                        //Make sure step has not been added for this file
                        if (!addeddeps.Contains(depfile))
                        {
                            var deptaskstep = new TCAdmin.TaskScheduler.ModuleApi.StepInfo();
                            deptaskstep.ModuleId = TCAdmin.GameHosting.ModuleApi.ModuleDef.ModuleId;
                            deptaskstep.ProcessId = 21;
                            deptaskstep.ServerId = service.ServerId;
                            deptaskstep.DisplayName = string.Format("Install Workshop file requirement {0}", depfile);
                            arguments.SetValue("WorkshopInstall.FileId", depfile.ToString());
                            deptaskstep.Arguments = arguments.ToString();
                            task.AddStep(deptaskstep);
                            addeddeps.Add(depfile);
                        }
                    }
                }
                TCAdmin.SDK.LogManager.Write(string.Format("Finished getting requirements for file id {0}.", gameMod.Id), TCAdmin.Interfaces.Logging.LogType.Information);
            }

            if (isbatch)
            {
                var installedfiles = ServiceWorkshopFile.GetServiceFileIds(service.ServiceId).Cast<ServiceWorkshopFile>().ToList().Where(ws => !ws.IsCollection && (currentcontent == "installed" | (ws.UpdateAvailable & currentcontent == "updatable")));
                if (isupdate)
                {
                    foreach (ServiceWorkshopFile workshopfile in installedfiles)
                    {

                        if (workshopfile.UpdateAvailable)
                        {
                            arguments.SetValue("WorkshopInstall.FileId", workshopfile.FileId.ToString());
                            arguments.SetValue("WorkshopInstall.Update", true);
                            var taskstep = new TCAdmin.TaskScheduler.ModuleApi.StepInfo();
                            taskstep.ModuleId = TCAdmin.GameHosting.ModuleApi.ModuleDef.ModuleId;
                            taskstep.ProcessId = 21;
                            taskstep.ServerId = service.ServerId;
                            taskstep.DisplayName = String.Format("Update Workshop file {0} on {1}", workshopfile.FileId, service.ConnectionInfo);
                            taskstep.Arguments = arguments.ToString();
                            task.AddStep(taskstep);
                        }
                    }

                    if (task.Steps == null || task.Steps.Count() == 0)
                    {
                        throw new Exception("All workshop files are up to date.");
                    }
                }
                else if (isbatchreinstall)
                {
                    //uninstall task
                    foreach (TCAdmin.GameHosting.SDK.Objects.ServiceWorkshopFile workshopfile in installedfiles)
                    {
                        arguments.SetValue("WorkshopInstall.FileId", workshopfile.FileId.ToString());
                        var taskstep = new TCAdmin.TaskScheduler.ModuleApi.StepInfo();
                        taskstep.ModuleId = TCAdmin.GameHosting.ModuleApi.ModuleDef.ModuleId;
                        taskstep.ProcessId = 22;
                        taskstep.ServerId = service.ServerId;
                        taskstep.DisplayName = String.Format("Uninstall Workshop file {0} on {1}", workshopfile.FileId, service.ConnectionInfo);
                        taskstep.Arguments = arguments.ToString();
                        task.AddStep(taskstep);
                    }

                    //install task
                    foreach (TCAdmin.GameHosting.SDK.Objects.ServiceWorkshopFile workshopfile in installedfiles)
                    {
                        arguments.SetValue("WorkshopInstall.FileId", workshopfile.FileId.ToString());
                        arguments.SetValue("WorkshopInstall.Update", false);
                        var taskstep = new TCAdmin.TaskScheduler.ModuleApi.StepInfo();
                        taskstep.ModuleId = TCAdmin.GameHosting.ModuleApi.ModuleDef.ModuleId;
                        taskstep.ProcessId = 21;
                        taskstep.ServerId = service.ServerId;
                        taskstep.DisplayName = String.Format("Install Workshop file {0} on {1}", workshopfile.FileId, service.ConnectionInfo);
                        taskstep.Arguments = arguments.ToString();
                        task.AddStep(taskstep);
                    }
                }
            }
            else
            {
                arguments.SetValue("WorkshopInstall.FileId", gameMod.Id);
                var taskstep = new TCAdmin.TaskScheduler.ModuleApi.StepInfo();
                taskstep.ModuleId = TCAdmin.GameHosting.ModuleApi.ModuleDef.ModuleId;
                taskstep.ProcessId = 21;
                taskstep.ServerId = service.ServerId;
                taskstep.DisplayName = String.Empty;
                taskstep.Arguments = arguments.ToString();
                task.AddStep(taskstep);
            }

            if (task.Steps == null || task.Steps.Count() == 0)
            {
                return -1;
            }

            //Create task
            task.Source = service.GetType().ToString();
            task.SourceId = service.ServiceId.ToString();
            return task.CreateTask().TaskId;
        }

        private int InstallCollection(Service service, SteamWorkshopFile gameMod)
        {
            var isupdate = System.Web.HttpContext.Current.Request.Form["update"] != null && System.Web.HttpContext.Current.Request.Form["update"] == "true";
            var childfiles = new List<string>();

            // Get child files from api
            dynamic jsondata = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(TCAdmin.Helper.Steam.WorkshopBrowser.GetCollectionDetails(ulong.Parse(gameMod.Id)));

            if (jsondata.response != null)
            {
                if (jsondata.response.collectiondetails != null)
                {
                    //Newtonsoft.Json.Linq.JArray collectiondetails = jsondata.response.collectiondetails;
                    if (jsondata.response.collectiondetails[0].children != null)
                    {
                        foreach (var child in jsondata.response.collectiondetails[0].children)
                        {
                            // Make sure file is not already installed (in case of install).
                            TCAdmin.GameHosting.SDK.Objects.ServiceWorkshopFile ws = new TCAdmin.GameHosting.SDK.Objects.ServiceWorkshopFile();
                            ws.ServiceId = service.ServiceId;
                            ws.FileId = System.Convert.ToUInt64(child.publishedfileid);
                            if (ws.Find() == false)
                                childfiles.Add(child.publishedfileid.ToString());
                        }
                    }
                    else
                        throw new Exception("GetCollectionDetails failed. No \"children\".");
                }
                else
                    throw new Exception("GetCollectionDetails failed. No \"collectiondetails\".");
            }
            else
                throw new Exception("GetCollectionDetails failed. No \"response\".");

            // Create task
            if (childfiles.Count > 0 | isupdate)
            {
                TCAdmin.TaskScheduler.ModuleApi.TaskInfo task = new TCAdmin.TaskScheduler.ModuleApi.TaskInfo();

                if (isupdate)
                {
                    task.DisplayName = string.Format("Update Workshop collection {0} on {1}", gameMod.Id, service.ConnectionInfo);
                    task.Steps = this.GetCollectionUpdateSteps(service, ulong.Parse(gameMod.Id)).ToArray();
                }
                else
                {
                    task.DisplayName = string.Format("Install Workshop collection {0} on {1}", gameMod.Id, service.ConnectionInfo);
                    task.Steps = this.GetCollectionUpdateSteps(service, ulong.Parse(gameMod.Id)).ToArray();
                }

                task.CreatedBy = TCAdmin.SDK.Session.GetCurrentUser().UserId;
                task.UserId = service.UserId;
                task.Source = service.GetType().ToString();
                task.SourceId = service.ServiceId.ToString();
                task.RunNow = true;


                if (task.Steps == null || task.Steps.Count() == 0)
                {
                    //No updates to files. Remove update flag.
                    ServiceWorkshopFile ws = new TCAdmin.GameHosting.SDK.Objects.ServiceWorkshopFile();
                    ws.ServiceId = service.ServiceId;
                    ws.FileId = System.Convert.ToUInt64(ulong.Parse(gameMod.Id));
                    if (ws.Find())
                    {
                        var jsonobj = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(TCAdmin.Helper.Steam.WorkshopBrowser.GetFileDetails(ws.FileId));
                        ws.FileDetails = Newtonsoft.Json.JsonConvert.SerializeObject(jsonobj["response"]["publishedfiledetails"]).Trim('[', ']');
                        ws.UpdateAvailable = false;
                        ws.Save();
                        return -2;
                    }


                    return -1;
                }

                // Create task
                task.Source = service.GetType().ToString();
                task.SourceId = System.Convert.ToString(service.ServiceId);
                return task.CreateTask().TaskId;

                
            }

            return -1;
        }

        private List<TCAdmin.TaskScheduler.ModuleApi.StepInfo> GetCollectionUpdateSteps(Service service, ulong collectionId)
        {
            List<TCAdmin.TaskScheduler.ModuleApi.StepInfo> steps = new List<TCAdmin.TaskScheduler.ModuleApi.StepInfo>();

            // Add/remove workshop files as needed
            List<string> childfiles = new List<string>();
            // Get child files from api
            dynamic jsondata = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(TCAdmin.Helper.Steam.WorkshopBrowser.GetCollectionDetails(collectionId));

            if (jsondata.response != null)
            {
                if (jsondata.response.collectiondetails != null)
                {
                    if (jsondata.response.collectiondetails[0].children !=null)
                    {
                        foreach (var child in jsondata.response.collectiondetails[0].children)
                        {
                            childfiles.Add(child.publishedfileid.ToString());
                        }
                    }
                }
            }

            TCAdmin.SDK.Objects.ObjectList currently_installed_from_collection = TCAdmin.GameHosting.SDK.Objects.ServiceWorkshopFile.GetCollectionFileIds(service.ServiceId, collectionId);
            TCAdmin.SDK.Objects.ObjectList all_installed = TCAdmin.GameHosting.SDK.Objects.ServiceWorkshopFile.GetServiceFileIds(service.ServiceId);

            // Uninstall installed files that have been removed from collection
            foreach (TCAdmin.GameHosting.SDK.Objects.ServiceWorkshopFile installedfile in currently_installed_from_collection)
            {
                if (!childfiles.Contains(installedfile.FileId.ToString()))
                {
                    TCAdmin.TaskScheduler.ModuleApi.StepInfo taskstep = new TCAdmin.TaskScheduler.ModuleApi.StepInfo();
                    TCAdmin.SDK.Database.XmlField arguments = new TCAdmin.SDK.Database.XmlField();
                    arguments.SetValue("WorkshopInstall.ServiceId", service.ServiceId);
                    arguments.SetValue("WorkshopInstall.FileId", installedfile.FileId);
                    arguments.SetValue("WorkshopInstall.Redirect", string.Format("~/Service/Workshop/{0}", service.ServiceId));
                    arguments.SetValue("WorkshopInstall.CollectionId", collectionId);

                    taskstep.ModuleId = TCAdmin.GameHosting.ModuleApi.ModuleDef.ModuleId;
                    taskstep.ProcessId = 22;
                    taskstep.ServerId = service.ServerId;
                    taskstep.DisplayName = string.Format("Uninstall file {0}", installedfile.FileId);
                    taskstep.Arguments = arguments.ToString();
                    steps.Add(taskstep);
                }
            }

            List<ulong> addeddeps = new List<ulong>();

            // Add files that have been added and are not already installed
            foreach (string childid in childfiles)
            {
                if (currently_installed_from_collection.FindByKey(service.ServiceId, UInt64.Parse(childid)) == null && all_installed.FindByKey(service.ServiceId, UInt64.Parse(childid)) == null)
                {
                    TCAdmin.SDK.Database.XmlField arguments = new TCAdmin.SDK.Database.XmlField();
                    arguments.SetValue("WorkshopInstall.ServiceId", service.ServiceId);
                    arguments.SetValue("WorkshopInstall.CollectionId", collectionId);
                    arguments.SetValue("WorkshopInstall.Redirect", string.Format("~/Service/Workshop/{0}", service.ServiceId));
                    arguments.SetValue("WorkshopInstall.Update", false);

                    // Install requirements
                    TCAdmin.SDK.LogManager.Write(string.Format("Getting requirements for file id {0}...", childid), TCAdmin.Interfaces.Logging.LogType.Information);
                    foreach (ulong depfile in TCAdmin.Helper.Steam.WorkshopBrowser.GetFileRequirements(System.Convert.ToUInt64(childid)))
                    {
                        if (!addeddeps.Contains(depfile))
                        {
                            // Make sure it's not installed already
                            TCAdmin.GameHosting.SDK.Objects.ServiceWorkshopFile wsfile = new TCAdmin.GameHosting.SDK.Objects.ServiceWorkshopFile();
                            wsfile.ServiceId = service.ServiceId;
                            wsfile.FileId = depfile;
                            if (!wsfile.Find())
                            {
                                TCAdmin.TaskScheduler.ModuleApi.StepInfo deptaskstep = new TCAdmin.TaskScheduler.ModuleApi.StepInfo();
                                deptaskstep.ModuleId = TCAdmin.GameHosting.ModuleApi.ModuleDef.ModuleId;
                                deptaskstep.ProcessId = 21;
                                deptaskstep.ServerId = service.ServerId;
                                deptaskstep.DisplayName = string.Format("Install Workshop file requirement {0}", depfile);
                                arguments.SetValue("WorkshopInstall.FileId", depfile.ToString());
                                deptaskstep.Arguments = arguments.ToString();
                                steps.Add(deptaskstep);

                                // Remember it so it doesn't get added to the current task gain.
                                addeddeps.Add(depfile);
                            }
                        }
                    }
                    TCAdmin.SDK.LogManager.Write(string.Format("Finished getting requirements for file id {0}.", childid), TCAdmin.Interfaces.Logging.LogType.Information);

                    arguments.SetValue("WorkshopInstall.FileId", UInt64.Parse(childid));


                    TCAdmin.TaskScheduler.ModuleApi.StepInfo taskstep = new TCAdmin.TaskScheduler.ModuleApi.StepInfo();
                    taskstep.ModuleId = TCAdmin.GameHosting.ModuleApi.ModuleDef.ModuleId;
                    taskstep.ProcessId = 21;
                    taskstep.ServerId = service.ServerId;
                    taskstep.DisplayName = string.Format("Install file {0}", childid);
                    taskstep.Arguments = arguments.ToString();
                    steps.Add(taskstep);
                }
            }

            // Tell the last step to update the collection
            if (steps.Count > 0)
            {
                TCAdmin.TaskScheduler.ModuleApi.StepInfo laststep = steps[steps.Count - 1];
                TCAdmin.SDK.Database.XmlField arguments = new TCAdmin.SDK.Database.XmlField(laststep.Arguments);

                arguments.SetValue("WorkshopInstall.AddCollection", true);

                laststep.Arguments = arguments.ToString();
                steps[steps.Count - 1] = laststep;
            }

            return steps;
        }

        public override bool UnInstallMod(Service service, GenericMod gameMod)
        {
            throw new NotImplementedException();
        }

        private int UninstallFile(Service service, SteamWorkshopFile gameMod)
        {
            var isbatch = System.Web.HttpContext.Current.Request.Form["batchAction"] != null;
            var currentcontent = System.Web.HttpContext.Current.Request.Form["content"]; //values are all, installed, updatable

            var task = new TCAdmin.TaskScheduler.ModuleApi.TaskInfo();
            var arguments = new TCAdmin.SDK.Database.XmlField();
            arguments.SetValue("WorkshopInstall.ServiceId", service.ServiceId);
            arguments.SetValue("WorkshopInstall.Redirect", string.Format("~/Service/Workshop/{0}", service.ServiceId));

            if (isbatch)
            {
                task.DisplayName = String.Format("Uninstall Workshop files on {0}", service.ConnectionInfo);
            }
            else
            {
                task.DisplayName = String.Format("Uninstall Workshop file {0} on {1}", gameMod.Id, service.ConnectionInfo);
            }

            task.CreatedBy = TCAdmin.SDK.Session.GetCurrentUser().UserId;
            task.UserId = service.UserId;
            task.Source = service.GetType().ToString();
            task.SourceId = service.ServiceId.ToString();
            task.RunNow = true;

            if (isbatch)
            {
                var installedfiles = ServiceWorkshopFile.GetServiceFileIds(service.ServiceId).Cast<ServiceWorkshopFile>().ToList().Where(ws => !ws.IsCollection && (currentcontent == "installed" | (ws.UpdateAvailable & currentcontent == "updatable")));
                foreach (ServiceWorkshopFile workshopfile in installedfiles)
                {
                    arguments.SetValue("WorkshopInstall.FileId", workshopfile.FileId);
                    var taskstep = new TCAdmin.TaskScheduler.ModuleApi.StepInfo();
                    taskstep.ModuleId = TCAdmin.GameHosting.ModuleApi.ModuleDef.ModuleId;
                    taskstep.ProcessId = 22;
                    taskstep.ServerId = service.ServerId;
                    taskstep.DisplayName = String.Format("Uninstall Workshop file {0} on {1}", workshopfile.FileId, service.ConnectionInfo); ;
                    taskstep.Arguments = arguments.ToString();
                    task.AddStep(taskstep);
                }
            }
            else
            {
                arguments.SetValue("WorkshopInstall.FileId", gameMod.Id);
                var taskstep = new TCAdmin.TaskScheduler.ModuleApi.StepInfo();
                taskstep.ModuleId = TCAdmin.GameHosting.ModuleApi.ModuleDef.ModuleId;
                taskstep.ProcessId = 22;
                taskstep.ServerId = service.ServerId;
                taskstep.DisplayName = String.Empty;
                taskstep.Arguments = arguments.ToString();
                task.AddStep(taskstep);
            }

            if (task.Steps == null || task.Steps.Count() == 0)
            {
                return -1;
            }

            //Create task
            task.Source = service.GetType().ToString();
            task.SourceId = service.ServiceId.ToString();
            return task.CreateTask().TaskId;
        }

        private int UninstallCollection(Service service, SteamWorkshopFile gameMod)
        {
            var task = new TCAdmin.TaskScheduler.ModuleApi.TaskInfo();
            task.DisplayName = String.Format("Uninstall Workshop collection {0} on {1}", gameMod.Id, service.ConnectionInfo);
            task.Steps = GetCollectionUninstallSteps(ulong.Parse(gameMod.Id)).ToArray();
            task.CreatedBy = TCAdmin.SDK.Session.GetCurrentUser().UserId;
            task.UserId = service.UserId;
            task.Source = service.GetType().ToString();
            task.SourceId = service.ServiceId.ToString();
            task.RunNow = true;

            if(task.Steps ==null || task.Steps.Length == 0)
            {
                //If collection doesn't have any files delete it
                if (ServiceWorkshopFile.GetCollectionFileIds(service.ServiceId, ulong.Parse(gameMod.Id)).Count == 0)
                {
                    var wsfile = new ServiceWorkshopFile(service.ServiceId, uint.Parse(gameMod.Id));
                    wsfile.Delete();
                    return 0;
                }
                return -1;
            }

            return task.CreateTask().TaskId;

        }

        private List<TCAdmin.TaskScheduler.ModuleApi.StepInfo> GetCollectionUninstallSteps(ulong collectionId)
        {
            TCAdmin.GameHosting.SDK.Objects.Service service = TCAdmin.GameHosting.SDK.Objects.Service.GetSelectedService();
            List<TCAdmin.TaskScheduler.ModuleApi.StepInfo> steps = new List<TCAdmin.TaskScheduler.ModuleApi.StepInfo>();

            List<string> childfiles = new List<string>();
            // Get child files from api
            dynamic jsondata = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(TCAdmin.Helper.Steam.WorkshopBrowser.GetCollectionDetails(collectionId));
            if (jsondata.response != null)
            {
                if (jsondata.response.collectiondetails != null)
                {
                    if (jsondata.response.collectiondetails[0].children != null)
                    {
                        foreach (dynamic child in jsondata.response.collectiondetails[0].children)
                        {
                            childfiles.Add(child.publishedfileid.ToString());
                        }
                    }
                }
            }

            TCAdmin.SDK.Objects.ObjectList currently_installed_from_collection = TCAdmin.GameHosting.SDK.Objects.ServiceWorkshopFile.GetCollectionFileIds(service.ServiceId, collectionId);
            TCAdmin.SDK.Objects.ObjectList all_installed = TCAdmin.GameHosting.SDK.Objects.ServiceWorkshopFile.GetServiceFileIds(service.ServiceId);

            // Remove all workshop mods for this collection
            foreach (TCAdmin.GameHosting.SDK.Objects.ServiceWorkshopFile installedfile in currently_installed_from_collection)
            {
                TCAdmin.TaskScheduler.ModuleApi.StepInfo taskstep = new TCAdmin.TaskScheduler.ModuleApi.StepInfo();
                TCAdmin.SDK.Database.XmlField arguments = new TCAdmin.SDK.Database.XmlField();
                arguments.SetValue("WorkshopInstall.ServiceId", service.ServiceId);
                arguments.SetValue("WorkshopInstall.FileId", installedfile.FileId);
                arguments.SetValue("WorkshopInstall.Redirect", string.Format("~/Service/Workshop/{0}", service.ServiceId));
                arguments.SetValue("WorkshopInstall.CollectionId", collectionId);

                taskstep.ModuleId = TCAdmin.GameHosting.ModuleApi.ModuleDef.ModuleId;
                taskstep.ProcessId = 22;
                taskstep.ServerId = service.ServerId;
                taskstep.DisplayName = string.Format("Uninstall file {0}", installedfile.FileId);
                taskstep.Arguments = arguments.ToString();
                steps.Add(taskstep);
            }

            // Tell the last step to update the collection
            if (steps.Count > 0)
            {
                TCAdmin.TaskScheduler.ModuleApi.StepInfo laststep = steps[steps.Count - 1];
                TCAdmin.SDK.Database.XmlField arguments = new TCAdmin.SDK.Database.XmlField(laststep.Arguments);

                arguments.SetValue("WorkshopInstall.DeleteCollection", true);

                laststep.Arguments = arguments.ToString();
                steps[steps.Count - 1] = laststep;
            }

            return steps;
        }


        public override int UninstallModWithTask(Service service, GenericMod gameMod)
        {
            var modinfo = (SteamWorkshopFile)gameMod;
            if (modinfo.IsCollection)
            {
                return UninstallCollection(service, modinfo);
            }
            else
            {
                return UninstallFile(service, modinfo);
            }
        }

        public override bool InstallMod(Service service, GenericMod gameMod, string versionId)
        {
            throw new NotImplementedException();
        }
    }
}
