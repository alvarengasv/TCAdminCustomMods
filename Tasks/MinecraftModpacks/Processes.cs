using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCAdminCustomMods.Models.MinecraftModPacks;

namespace TCAdminCustomMods.Tasks.MinecraftModpacks
{
    public struct ModpackInfo
    {
        public int ServiceId { get; set; }
        public int ModpackId { get; set; }
        public int VersionId { get; set; }
        public string Type { get; set; }
        public string ModLoader { get; set; }
        public string ModLoaderType { get; set; }
        public string GameVersion { get; set; }
        public string RedirectUrl { get; set; }
        public string JarVariable { get; set; }
        public string CurseApiKey { get; set; }
    }

    public class Processes : TCAdmin.TaskScheduler.ModuleApi.StepBase
    {
        public override void Start()
        {
            switch (this.Arguments.Command)
            {
                case "InstallModpack":
                    InstallModpack();
                    break;
                case "UninstallModpack":
                    UninstallModpack();
                    break;
                default:
                    throw new NotImplementedException(this.Arguments.Command);
            }
        }

        private void InstallModpack()
        {
            var modpackinfo = (ModpackInfo)TCAdmin.SDK.Misc.ObjectXml.XmlToObject(this.Arguments.Arguments, typeof(ModpackInfo));

            var service = new TCAdmin.GameHosting.SDK.Objects.Service(modpackinfo.ServiceId);
            var original_status = service.Status;
            try
            {
                if(original_status.ServiceStatus == TCAdmin.Interfaces.Server.ServiceStatus.Running)
                {
                    service.Stop("Modpack installation");
                }

                var install_data = System.IO.Path.Combine(service.RootDirectory, String.Format("Modpack-{0}.data", modpackinfo.ModpackId));
                if (System.IO.File.Exists(install_data))
                {
                    UninstallModpack();
                }

                var server = new TCAdmin.GameHosting.SDK.Objects.Server(service.ServerId);
                var provider = new TCAdminCustomMods.Providers.MinecraftModpacksProvider();
                var iscurse = modpackinfo.Type.Equals("Curseforge", StringComparison.InvariantCultureIgnoreCase);
                var genericMod = iscurse ? Models.Curse.CurseBrowser.GetMod(modpackinfo.ModpackId) : provider.GetMod(modpackinfo.ModpackId.ToString(), Providers.ModSearchType.Id);
                
                var filepath = iscurse? "Shared/bin-extensions/MinecraftModpack-Curseforge.py": "Shared/bin-extensions/MinecraftModpack-FTB.py";
                var script = string.Empty;
                var utility = service.GetScriptUtility();
                utility.ScriptEngineManager.AddVariable("Script.WorkingDirectory", service.RootDirectory);
                modpackinfo.CurseApiKey = Models.Curse.CurseBrowser.CURSE_API_KEY;
                utility.AddObject("ThisModpackInfo", modpackinfo);
                utility.AddObject("ThisTaskStep", this);
                utility.AddObject("ThisApiInfo", genericMod);
                utility.AddObject("CURSE_API_KEY", Models.Curse.CurseBrowser.CURSE_API_KEY);

                if (System.IO.File.Exists(filepath))
                {
                    script = System.IO.File.ReadAllText(filepath);
                }
                else
                {
                    if (iscurse)
                    {
                        script = PythonScripts.MinecraftModpack_Curseforge;
                    }
                    else
                    {
                        script = PythonScripts.MinecraftModpack_FTB;
                    }
                    
                }
                utility.ScriptEngineManager.SetScript("ipy", script, null);
                DeleteModpackCmdLines(service);

                var createdfiles = new List<string>();
                using (var filewatcher = new System.IO.FileSystemWatcher(service.RootDirectory, "*"))
                {
                    filewatcher.InternalBufferSize = 32768;
                    filewatcher.IncludeSubdirectories = true;
                    filewatcher.Created += (object sender, System.IO.FileSystemEventArgs e) =>
                    {
                        createdfiles.Add(e.FullPath);
                    };
                    filewatcher.Renamed += (object sender, System.IO.RenamedEventArgs e) =>
                    {
                        createdfiles.Add(e.FullPath);
                    };
                    filewatcher.EnableRaisingEvents = true;

                    utility.ScriptEngineManager.Execute();
                    provider.PostInstallMod(service, genericMod);
                }

                createdfiles.Reverse();
                using (var file = System.IO.File.OpenWrite(install_data))
                {
                    using (var writer = new System.IO.StreamWriter(file))
                    {
                        var i = 0;
                        foreach(var createdfile in createdfiles)
                        {
                            if(System.IO.File.Exists(System.IO.Path.Combine(service.RootDirectory, createdfile)) || System.IO.File.Exists(System.IO.Path.Combine(service.RootDirectory, createdfile)))
                            i += 1;
                            if (i > 1)
                            {
                                writer.Write("\n");
                            }
                            writer.Write(TCAdmin.SDK.Misc.Strings.ReplaceCaseInsensitive(createdfile, service.RootDirectory, string.Empty));
                        }
                    }
                }

                //remove current version if it exists
                foreach (var installed in provider.GetInstalledPlugins(service))
                {
                    var packid = int.Parse(installed.Split(':')[0].Replace("MCMP", string.Empty));
                    if (packid == modpackinfo.ModpackId)
                    {
                        var verid = int.Parse(installed.Split(':')[1]);
                        provider.RemoveInstalledPlugin(service, packid, verid);
                    }
                }

                provider.AddInstalledPlugin(service, modpackinfo.ModpackId, modpackinfo.VersionId);

                //Repair permissions
                this.WriteLog("Setting file permissions...");
                var usercfgfile = string.Format("Services/{0}/User.cfg", service.ServiceId);
                if (System.IO.File.Exists(usercfgfile))
                {
                    var usercfg = new TCAdmin.SDK.Database.XmlField();
                    usercfg.LoadFromFile(usercfgfile);

                    switch (server.OperatingSystem)
                    {
                        case TCAdmin.SDK.Objects.OperatingSystem.Linux:
                            TCAdmin.SDK.Misc.Linux.SetDirectoryOwner(service.RootDirectory, usercfg["Service.User"].ToString(), true);
                            break;
                        case TCAdmin.SDK.Objects.OperatingSystem.Windows:
                            server.GameHostingUtilitiesService.ConfigureGameAccountPermissions(usercfg["Service.User"].ToString(), service.RootDirectory);
                            break;
                        default:
                            throw new NotImplementedException(server.OperatingSystem.ToString());
                    }
                }
                service.Configure();

                var responsedata = new TCAdmin.SDK.Database.XmlField();
                responsedata.SetValue("Task.RedirectUrl", modpackinfo.RedirectUrl);
                responsedata.SetValue("Task.RedirectUrlMvc", modpackinfo.RedirectUrl);
                this.SetResponse(responsedata.ToString());
                this.UpdateProgress(100);
            }
            finally
            {
                if (original_status.ServiceStatus == TCAdmin.Interfaces.Server.ServiceStatus.Running)
                {
                    service.Start("Modpack installation");
                }
            }
        }

        private void UninstallModpack()
        {
            var modpackinfo = (ModpackInfo)TCAdmin.SDK.Misc.ObjectXml.XmlToObject(this.Arguments.Arguments, typeof(ModpackInfo));
            var service = new TCAdmin.GameHosting.SDK.Objects.Service(modpackinfo.ServiceId);
            var original_status = service.Status;

            try
            {
                if (original_status.ServiceStatus == TCAdmin.Interfaces.Server.ServiceStatus.Running)
                {
                    service.Stop("Modpack uninstall");
                }

                var responsedata = new TCAdmin.SDK.Database.XmlField();
                responsedata.SetValue("Task.RedirectUrl", modpackinfo.RedirectUrl);
                responsedata.SetValue("Task.RedirectUrlMvc", modpackinfo.RedirectUrl);
                var datafile = System.IO.Path.Combine(service.RootDirectory, string.Format("Modpack-{0}.data", modpackinfo.ModpackId));
                var provider = new TCAdminCustomMods.Providers.MinecraftModpacksProvider();
                var genericMod = provider.GetMod(modpackinfo.ModpackId.ToString(), Providers.ModSearchType.Id);

                DeleteModpackCmdLines(service);

                if (!System.IO.File.Exists(datafile))
                {
                    this.WriteLog("Modpack data file does not exist so files can't be deleted.");
                    provider.RemoveInstalledPlugin(service, modpackinfo.ModpackId, modpackinfo.VersionId);
                    provider.PostUninstallMod(service, genericMod);
                    System.Threading.Thread.Sleep(5000);
                    this.SetResponse(responsedata.ToString());
                    this.UpdateProgress(100);
                    return;
                }

                this.WriteLog("Reading install data...");
                var files = new List<string>();
                using (var reader = System.IO.File.OpenText(datafile))
                {
                    string line;
                    while (!string.IsNullOrEmpty((line = reader.ReadLine())))
                    {
                        if (line.IndexOf("..") == -1)
                        {
                            var path = System.IO.Path.Combine(service.RootDirectory, line);
                            if (System.IO.File.Exists(path) | System.IO.Directory.Exists(path))
                            {
                                files.Add(line);
                            }
                        }
                    }
                }

                this.WriteLog("Deleting files...");

                if (files.Count > 0)
                {
                    var progress_increase = ((float)100) / files.Count;
                    var current_progress = (float)0;
                    foreach (var file in files)
                    {
                        //this.WriteLog(string.Format("Deleting {0}...", file), short.Parse(Math.Round(current_progress, 0).ToString()));
                        this.UpdateProgress(short.Parse(Math.Round(current_progress, 0).ToString()));
                        var fullpath = System.IO.Path.Combine(service.RootDirectory, file);
                        if (System.IO.File.Exists(fullpath))
                        {
                            try
                            {
                                System.IO.File.Delete(fullpath);
                            }
                            catch (Exception ex){ this.WriteDebugLog(ex.ToString()); }
                        }
                        else if(System.IO.Directory.Exists(fullpath))
                        {
                            try
                            {
                                System.IO.Directory.Delete(fullpath, true);
                            }
                            catch (Exception ex) { this.WriteDebugLog(ex.ToString()); }
                        }
                        current_progress += progress_increase;
                    }
                }

                System.IO.File.Delete(datafile);

                provider.GetInstalledPlugins(service);

                provider.RemoveInstalledPlugin(service, modpackinfo.ModpackId, modpackinfo.VersionId);
                provider.PostUninstallMod(service, genericMod);

                this.SetResponse(responsedata.ToString());
                this.UpdateProgress(100);
            }
            finally
            {
                if (original_status.ServiceStatus == TCAdmin.Interfaces.Server.ServiceStatus.Running)
                {
                    service.Start("Modpack uninstall");
                }
            }
        }

        private void DeleteModpackCmdLines(TCAdmin.GameHosting.SDK.Objects.Service service)
        {
            var deleted = false;
            var cmdlines = TCAdmin.GameHosting.SDK.Objects.CustomCmdLine.GetCustomCommandLines(service.ServiceId);
            foreach (TCAdmin.GameHosting.SDK.Objects.CustomCmdLine cmdline in cmdlines)
            {
                if(cmdline.Variables.HasValue("CM:ModpackCmd") && (bool)cmdline.Variables["CM:ModpackCmd"])
                {
                    deleted = true;
                    cmdline.Delete();
                }
            }

            if (deleted && service.OverrideCommandLine)
            {
                var game = new TCAdmin.GameHosting.SDK.Objects.Game(service.GameId);
                service.CustomFields["d3b2aa93-7e2b-4e0d-8080-67d14b2fa8a9:CmdLineManager:CmdLineName"] = string.Empty;
                service.UnparsedCommandLine = service.Private && !string.IsNullOrEmpty(game.CommandLines.PrivateCmdLine) ? game.CommandLines.PrivateCmdLine : game.CommandLines.DefaultCmdLine;
                service.Save();
                service.Configure();
            }
        }
    }
}
