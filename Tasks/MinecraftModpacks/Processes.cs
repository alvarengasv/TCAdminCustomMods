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
        public string RedirectUrl { get; set; }
        public string JarVariable { get; set; }
    }

    public class Processes : TCAdmin.TaskScheduler.ModuleApi.StepBase
    {
        private const string DEFAULT_INSTALL_SCRIPT = @"import urllib2, json, time, clr
clr.AddReference('TCAdmin.GameHosting.SDK')
clr.AddReference('TCAdmin.SDK')
from System.Text.RegularExpressions import Regex, RegexOptions, Match
from TCAdmin.GameHosting.SDK.Objects import CustomCmdLine, GameVariableConfig
from TCAdmin.SDK.Scripting import ScriptUtility
from xml.etree.ElementTree import parse
from System.Net import WebClient
from System.IO import Path, Directory, File
from System import String, Exception, Version
from System.Diagnostics import Process
from glob import glob

#Define function that we need to execute the modloader installer
def install_modloader(modloader_type, modloader_executable):
  ThisTaskStep.WriteLog(String.Format('Running {0} installer... This might take some time. ', modloader_type.capitalize()))
  arg = String.Format(' -jar {0} --installServer {1}', modloader_executable, ThisService.RootDirectory) if modloader_type == 'forge' else String.Format(' -jar {0} server -downloadMinecraft -mcversion {1} -dir {2} -loader {3}', modloader_executable, minecraft_version, ThisService.RootDirectory, modloader_version)
  p = Process()
  p.StartInfo.WorkingDirectory = ThisService.WorkingDirectory
  p.StartInfo.FileName = ThisService.Executable
  p.StartInfo.Arguments = arg
  p.Start()
  fakeprogress_increment = 5
  fakeprogress = 0
  while not p.HasExited :
    ThisTaskStep.UpdateProgress(fakeprogress)
    fakeprogress += fakeprogress_increment
    if fakeprogress > 100 :
      fakeprogress = 0
    time.sleep(5)
  if File.Exists(modloader_executable):
    File.Delete(modloader_executable)
  if modloader_type == 'forge':
    possible_jars = glob(Path.Combine(ThisService.RootDirectory, modloader_executable.replace('-installer', '*')))
    run_jar = possible_jars[0] if len(possible_jars) > 0 else None
  elif modloader_type == 'fabric':
    run_jar = 'fabric-server-launch.jar'
  return run_jar

#Get information about the modpack
ThisTaskStep.WriteLog('Getting information about the modpack...')
if ThisModpackInfo.Type == 'curseforge':
  response = urllib2.urlopen(String.Format('https://api.modpacks.ch/public/curseforge/{0}/{1}', ThisModpackInfo.ModpackId, ThisModpackInfo.VersionId))
else:
  response = urllib2.urlopen(String.Format('https://api.modpacks.ch/public/modpack/{0}/{1}', ThisModpackInfo.ModpackId, ThisModpackInfo.VersionId))
data = json.load(response)

modloader_version, minecraft_version = None, None
#Get modloader type.
for target in data['targets']:
  if target['type'] == 'modloader':
    modloader_version = target['version']
    modloader_type = target['name']
  if target['type'] == 'game':
    minecraft_version = target['version']
#Fix for forge versions up until 12.18.0.2007. These versions has Minecraft version suffixed.
if modloader_type == 'forge' and Version(modloader_version).CompareTo(Version('12.18.0.2008')) < 0:
  modloader_version = modloader_version+'-'+minecraft_version

wc = WebClient()
supported_modloaders = ['forge', 'fabric']
if modloader_type in supported_modloaders:
  if modloader_version and minecraft_version:
    if modloader_type == 'fabric':
      #Find the latest version of the fabric installer and download it.
      fabric_metadata = urllib2.urlopen('https://maven.fabricmc.net/net/fabricmc/fabric-installer/maven-metadata.xml')
      xmldoc = parse(fabric_metadata)
      for item in xmldoc.iterfind('versioning'):
        fabric_release = item.findtext('release')
        modloader_url = String.Format('https://maven.fabricmc.net/net/fabricmc/fabric-installer/{0}/fabric-installer-{0}.jar', fabric_release)
        modloader_executable = Path.Combine(ThisService.RootDirectory, 'fabric-installer-'+fabric_release+'.jar')
    elif modloader_type == 'forge':
      combined_version = minecraft_version+'-'+modloader_version
      modloader_url = 'https://maven.minecraftforge.net/net/minecraftforge/forge/'+combined_version+'/forge-'+combined_version+'-installer.jar'
      modloader_executable = Path.Combine(ThisService.RootDirectory, 'forge-'+combined_version+'-installer.jar')
    ThisTaskStep.WriteLog(String.Format('Downloading {0} version {1} from {2}', modloader_type, modloader_version, modloader_url))
    wc.DownloadFile(modloader_url, modloader_executable)
else:
  raise Exception(String.Format('The modpack\'s modloader ({0}) is currently not supported. Aborting install.', modloader_type.capitalize()))

#Download modfiles
progress_increment = float(100) / len(data['files'])
current_progress = 0
#install_data = Path.Combine(ThisService.RootDirectory, String.Format('Modpack-{0}.data', ThisModpackInfo.ModpackId))
#if File.Exists(install_data):
#  File.Delete(install_data)

for i in data['files']:
  ThisTaskStep.WriteLog(String.Format('Downloading {0}...', i['name']), int(current_progress))
  install_path = Path.Combine(ThisService.RootDirectory, i['path'].replace('./', ''))
  if not Directory.Exists(install_path):
    Directory.CreateDirectory(install_path)
  wc.DownloadFile(i['url'], Path.Combine(install_path, i['name']))
  #file_object = open(install_data, 'a')
  #file_object.write(Path.Combine(install_path, i['name']).Replace(ThisService.RootDirectory, '') + '\n')
  #file_object.close()
  current_progress +=progress_increment

#Delete libraries folder
if Directory.Exists(Path.Combine(ThisService.RootDirectory, 'libraries')):
  Directory.Delete(Path.Combine(ThisService.RootDirectory, 'libraries'), True)
#Install modloader (only fabric and forge supported)
if ThisModpackInfo.ModLoader == 'default':
  if modloader_type == 'forge' or modloader_type == 'fabric':

    run_jar = install_modloader(modloader_type, modloader_executable)
    
    #Try recommended 
    if modloader_type == 'forge' and not File.Exists(run_jar):
      ThisTaskStep.WriteLog(String.Format('Failed installing {0} using the default modpack version. Trying recommended version.', modloader_type.capitalize()))
      forge_version_page = urllib2.urlopen(String.Format('https://files.minecraftforge.net/net/minecraftforge/forge/index_{0}.html', minecraft_version)).read()
      regex_pattern = '<a href=\""https:\/\/adfoc.us\/serve\/sitelinks\/\?id=[0-9]+&url=(?<DownloadUrl>.*)\"" title=\""Installer\"">'
      matches = Regex.Matches(forge_version_page, regex_pattern, RegexOptions.IgnoreCase)
      if matches.Count > 0:
        forge_latest_url = matches[0].Groups['DownloadUrl'].Value
        forge_recommended_url = matches[1].Groups['DownloadUrl'].Value
        regex_pattern = '^https:\/\/maven\.minecraftforge\.net\/net\/minecraftforge\/forge\/(?<forge_version>.*)\/forge-.*-installer\.jar$'
        match = Regex.Match(forge_recommended_url, regex_pattern, RegexOptions.IgnoreCase)
        if match.Success:
          combined_version = match.Groups['forge_version'].Value
          modloader_executable = Path.Combine(ThisService.RootDirectory, 'forge-'+combined_version+'-installer.jar')
          wc.DownloadFile(forge_recommended_url, modloader_executable)
          run_jar = install_modloader(modloader_type, modloader_executable)
          #Try latest modloader version
          if modloader_type == 'forge' and not File.Exists(run_jar):
            ThisTaskStep.WriteLog(String.Format('Failed installing {0} using the recommended modpack version. Trying latest version.', modloader_type.capitalize()))
            match = Regex.Match(forge_latest_url, regex_pattern, RegexOptions.IgnoreCase)
            if match.Success:
              combined_version = match.Groups['forge_version'].Value
              modloader_executable = Path.Combine(ThisService.RootDirectory, 'forge-'+combined_version+'-installer.jar')
              wc.DownloadFile(forge_recommended_url, modloader_executable)
              run_jar = install_modloader(modloader_type, modloader_executable)
              if modloader_type == 'forge' and not File.Exists(run_jar):
                raise Exception('Modloader could not be installed. Modpack has been installed. You will need to install the modloader manually')
elif ThisModpackInfo.ModLoader == 'recommended':
  ThisTaskStep.WriteLog('Installing recommended Forge modloader for this Minecraft version')
  forge_version_page = urllib2.urlopen(String.Format('https://files.minecraftforge.net/net/minecraftforge/forge/index_{0}.html', minecraft_version)).read()
  regex_pattern = '<a href=\""https:\/\/adfoc.us\/serve\/sitelinks\/\?id=[0-9]+&url=(?<DownloadUrl>.*)\"" title=\""Installer\"">'
  matches = Regex.Matches(forge_version_page, regex_pattern, RegexOptions.IgnoreCase)
  if matches.Count > 0:
    forge_recommended_url = matches[1].Groups['DownloadUrl'].Value
    regex_pattern = '^https:\/\/maven\.minecraftforge\.net\/net\/minecraftforge\/forge\/(?<forge_version>.*)\/forge-.*-installer\.jar$'
    match = Regex.Match(forge_recommended_url, regex_pattern, RegexOptions.IgnoreCase)
    if match.Success:
      combined_version = match.Groups['forge_version'].Value
      modloader_executable = Path.Combine(ThisService.RootDirectory, 'forge-'+combined_version+'-installer.jar')
      wc.DownloadFile(forge_recommended_url, modloader_executable)
      run_jar = install_modloader(modloader_type, modloader_executable)
      if not File.Exists(run_jar):
        raise Exception('Modloader could not be installed. Modpack has been installed. You will need to install the modloader manually')
elif ThisModpackInfo.ModLoader == 'latest':
  ThisTaskStep.WriteLog('Installing latest Forge modloader for this Minecraft version')
  forge_version_page = urllib2.urlopen(String.Format('https://files.minecraftforge.net/net/minecraftforge/forge/index_{0}.html', minecraft_version)).read()
  regex_pattern = '<a href=\""https:\/\/adfoc.us\/serve\/sitelinks\/\?id=[0-9]+&url=(?<DownloadUrl>.*)\"" title=\""Installer\"">'
  matches = Regex.Matches(forge_version_page, regex_pattern, RegexOptions.IgnoreCase)
  if matches.Count > 0:
    forge_latest_url = matches[0].Groups['DownloadUrl'].Value
    regex_pattern = '^https:\/\/maven\.minecraftforge\.net\/net\/minecraftforge\/forge\/(?<forge_version>.*)\/forge-.*-installer\.jar$'
    match = Regex.Match(forge_latest_url, regex_pattern, RegexOptions.IgnoreCase)
    if match.Success:
      combined_version = match.Groups['forge_version'].Value
      modloader_executable = Path.Combine(ThisService.RootDirectory, 'forge-'+combined_version+'-installer.jar')
      wc.DownloadFile(forge_latest_url, modloader_executable)
      run_jar = install_modloader(modloader_type, modloader_executable)
      if not File.Exists(run_jar):
        raise Exception('Modloader could not be installed. Modpack has been installed. You will need to install the modloader manually')

#Set commandline
ThisTaskStep.WriteLog('Setting commandline')
cmdline = CustomCmdLine()
util = ScriptUtility
util = ThisService.GetScriptUtility();
util.ExecuteScripts = False;
cmdline.ServiceId = ThisService.ServiceId
Script.WriteToConsole(ThisApiInfo.Name)
if ThisApiInfo.Name.Length > 25 :
  cmdline.Description = ThisApiInfo.Name.Substring(0, 25)
else :
  cmdline.Description = ThisApiInfo.Name
cmdline.Variables.LoadXml(ThisService.Variables.ToString())
cmdline.Variables['CM:ModpackCmd'] = True
cmdline.Variables[ThisModpackInfo.JarVariable] = run_jar.replace(ThisService.RootDirectory, '')
cmdline_template = ThisGame.CommandLines.DefaultCustomCmdLine
if ThisService.Private and ThisGame.CommandLines.PrivateCustomCmdLine != '' :
  cmdline_template = ThisGame.CommandLines.PrivateCustomCmdLine
elif cmdline_template == '' :
  cmdline_template = ThisGame.CommandLines.DefaultCmdLine
cmdline_template = GameVariableConfig.ReplaceVariablesWithCustomTemplates(ThisGame.CustomVariables, cmdline_template)
cmdline.CommandLine = util.ProcessDynamicValues(ThisGame.CustomVariables, cmdline.Variables, cmdline_template, True)
cmdline.GenerateKey()
cmdline.Save()
ThisService.OverrideCommandLine = True
ThisService.UnparsedCommandLine = cmdline.CommandLine
ThisService.CustomFields['d3b2aa93-7e2b-4e0d-8080-67d14b2fa8a9:CmdLineManager:CmdLineName'] = String.Format('CUS:{0}', cmdline.CmdLineId)
ThisService.Save()
ThisService.Configure()";

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
                MinecraftModpacksBrowser genericMod = (MinecraftModpacksBrowser)provider.GetMod(modpackinfo.ModpackId.ToString(), Providers.ModSearchType.Id);
                var filepath = "Shared/bin-extensions/MinecraftModpack-Install-Script.txt";
                var script = string.Empty;
                var utility = service.GetScriptUtility();
                utility.ScriptEngineManager.AddVariable("Script.WorkingDirectory", service.RootDirectory);
                utility.AddObject("ThisModpackInfo", modpackinfo);
                utility.AddObject("ThisApiInfo", genericMod);
                utility.AddObject("ThisTaskStep", this);
                if (System.IO.File.Exists(filepath))
                {
                    script = System.IO.File.ReadAllText(filepath);
                }
                else
                {
                    script = DEFAULT_INSTALL_SCRIPT;
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

                if (files.Count > 0)
                {
                    var progress_increase = ((float)100) / files.Count;
                    var current_progress = (float)0;
                    foreach (var file in files)
                    {
                        this.WriteLog(string.Format("Deleting {0}...", file), short.Parse(Math.Round(current_progress, 0).ToString()));
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
                                System.IO.Directory.Delete(fullpath);
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
