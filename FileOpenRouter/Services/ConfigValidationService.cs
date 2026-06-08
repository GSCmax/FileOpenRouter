using FileOpenRouter.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FileOpenRouter.Services
{
    public static class ConfigValidationService
    {
        public static RouterConfig Normalize(RouterConfig config)
        {
            if (config == null)
            {
                config = new RouterConfig();
            }

            if (config.Rules == null)
            {
                config.Rules = new List<RouteRule>();
            }

            if (config.FallbackProgram == null)
            {
                config.FallbackProgram = string.Empty;
            }

            foreach (var rule in config.Rules.Where(item => item != null))
            {
                if (rule.Name == null)
                {
                    rule.Name = string.Empty;
                }

                if (rule.Folder == null)
                {
                    rule.Folder = string.Empty;
                }

                if (rule.Program == null)
                {
                    rule.Program = string.Empty;
                }
            }

            return config;
        }

        public static string ValidateForSave(RouterConfig config)
        {
            config = Normalize(config);

            if (string.IsNullOrWhiteSpace(config.FallbackProgram))
            {
                return "请先选择兜底程序。";
            }

            if (!File.Exists(config.FallbackProgram))
            {
                return "兜底程序不存在：" + Environment.NewLine + config.FallbackProgram;
            }

            if (!IsExeFile(config.FallbackProgram))
            {
                return "兜底程序必须是 .exe 文件。";
            }

            if (IsCurrentProgram(config.FallbackProgram))
            {
                return "兜底程序不能选择本程序自身。";
            }

            foreach (var rule in config.Rules.Where(item => item != null && item.Enabled))
            {
                var ruleName = string.IsNullOrWhiteSpace(rule.Name) ? "未命名规则" : rule.Name;

                if (string.IsNullOrWhiteSpace(rule.Folder))
                {
                    return "启用规则的文件夹路径不能为空：" + ruleName;
                }

                if (!Directory.Exists(rule.Folder))
                {
                    return "启用规则的文件夹不存在：" + Environment.NewLine + ruleName + Environment.NewLine + rule.Folder;
                }

                if (string.IsNullOrWhiteSpace(rule.Program))
                {
                    return "启用规则的打开程序不能为空：" + ruleName;
                }

                if (!File.Exists(rule.Program))
                {
                    return "启用规则的打开程序不存在：" + Environment.NewLine + ruleName + Environment.NewLine + rule.Program;
                }

                if (!IsExeFile(rule.Program))
                {
                    return "启用规则的打开程序必须是 .exe 文件：" + Environment.NewLine + ruleName + Environment.NewLine + rule.Program;
                }

                if (IsCurrentProgram(rule.Program))
                {
                    return "启用规则的打开程序不能选择本程序自身：" + Environment.NewLine + ruleName + Environment.NewLine + rule.Program;
                }
            }

            return null;
        }

        private static bool IsExeFile(string path)
        {
            return string.Equals(Path.GetExtension(path), ".exe", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCurrentProgram(string programPath)
        {
            try
            {
                var currentProgramPath = Process.GetCurrentProcess().MainModule.FileName;
                return string.Equals(
                    Path.GetFullPath(programPath),
                    Path.GetFullPath(currentProgramPath),
                    StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}
