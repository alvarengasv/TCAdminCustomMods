using System;
using System.Collections.Generic;
using System.Text;
using Alexr03.Common.TCAdmin.Logging;
using Newtonsoft.Json;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdmin.SDK.Misc;
using Server = TCAdmin.GameHosting.SDK.Objects.Server;

namespace TCAdminCustomMods.Providers
{
    public static class InstalledModsRegistry
    {
        private static readonly LogManager Logger = LogManager.Create(typeof(InstalledModsRegistry));

        public static List<T> Load<T>(Service service, string fileName)
        {
            var server = new Server(service.ServerId);
            var path = FileSystem.CombinePath(server.OperatingSystem, service.RootDirectory, fileName);
            try
            {
                var fileSystem = server.FileSystemService;
                if (!fileSystem.FileExists(path)) return new List<T>();
                var bytes = fileSystem.ReadBinary(path);
                if (bytes == null || bytes.Length == 0) return new List<T>();
                var json = Encoding.UTF8.GetString(bytes);
                return JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to read installed mods manifest at " + path, ex);
                return new List<T>();
            }
        }

        public static void Save<T>(Service service, string fileName, List<T> entries)
        {
            var server = new Server(service.ServerId);
            var path = FileSystem.CombinePath(server.OperatingSystem, service.RootDirectory, fileName);
            try
            {
                var json = JsonConvert.SerializeObject(entries, Formatting.Indented);
                var bytes = Encoding.UTF8.GetBytes(json);
                server.FileSystemService.CreateBinary(path, bytes, false);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to write installed mods manifest at " + path, ex);
            }
        }
    }
}
