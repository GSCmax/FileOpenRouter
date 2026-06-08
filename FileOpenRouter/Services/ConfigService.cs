using FileOpenRouter.Models;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;

namespace FileOpenRouter.Services
{
    public static class ConfigService
    {
        private const string AppFolderName = "FileOpenRouter";

        public static string GetConfigPath()
        {
            return Path.Combine(GetConfigDirectory(), GetConfigFileName());
        }

        public static string GetConfigDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppFolderName);
        }

        public static RouterConfig Load()
        {
            EnsureConfigDirectory();

            var configPath = GetConfigPath();
            if (!File.Exists(configPath))
            {
                var defaultConfig = new RouterConfig();
                Save(defaultConfig);
                return defaultConfig;
            }

            try
            {
                using (var stream = File.OpenRead(configPath))
                {
                    var serializer = new DataContractJsonSerializer(typeof(RouterConfig));
                    var config = serializer.ReadObject(stream) as RouterConfig;
                    if (config == null)
                    {
                        throw new SerializationException("配置内容为空或格式不正确。");
                    }

                    return ConfigValidationService.Normalize(config);
                }
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is UnauthorizedAccessException ||
                                       ex is SerializationException ||
                                       ex is XmlException ||
                                       ex is ArgumentException)
            {
                throw new InvalidOperationException(
                    "读取配置文件失败，可能是 JSON 已损坏或文件不可访问。" +
                    Environment.NewLine +
                    "配置文件：" + configPath +
                    Environment.NewLine +
                    "错误：" + ex.Message,
                    ex);
            }
        }

        public static void Save(RouterConfig config)
        {
            EnsureConfigDirectory();

            config = ConfigValidationService.Normalize(config);

            var serializer = new DataContractJsonSerializer(typeof(RouterConfig));
            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = new UTF8Encoding(false)
            };

            using (var stream = File.Create(GetConfigPath()))
            using (var writer = JsonReaderWriterFactory.CreateJsonWriter(
                stream,
                settings.Encoding,
                false,
                true,
                "  "))
            {
                serializer.WriteObject(writer, config);
            }
        }

        private static void EnsureConfigDirectory()
        {
            var configDirectory = GetConfigDirectory();
            if (!Directory.Exists(configDirectory))
            {
                Directory.CreateDirectory(configDirectory);
            }
        }

        private static string GetConfigFileName()
        {
            var assembly = Assembly.GetEntryAssembly();
            var exeName = assembly == null
                ? AppFolderName
                : Path.GetFileNameWithoutExtension(assembly.Location);

            if (string.IsNullOrWhiteSpace(exeName))
            {
                exeName = AppFolderName;
            }

            return exeName + ".json";
        }
    }
}
