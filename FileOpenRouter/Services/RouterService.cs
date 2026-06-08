using FileOpenRouter.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FileOpenRouter.Services
{
    public static class RouterService
    {
        public static RouteResult RouteFile(string filePath, RouterConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            string normalizedFilePath;
            try
            {
                normalizedFilePath = Path.GetFullPath(filePath);
            }
            catch (Exception ex)
            {
                return Fail("文件路径无效：" + ex.Message);
            }

            if (!File.Exists(normalizedFilePath))
            {
                return Fail("文件不存在：" + normalizedFilePath);
            }

            if (config.Rules != null)
            {
                foreach (var rule in config.Rules
                    .Where(IsUsableRule)
                    .OrderByDescending(rule => GetFolderMatchLength(rule.Folder)))
                {
                    if (IsFileUnderFolder(normalizedFilePath, rule.Folder))
                    {
                        return StartProgram(rule.Program, normalizedFilePath, rule.Name);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(config.FallbackProgram))
            {
                return Fail("没有匹配的规则，并且 fallbackProgram 为空。");
            }

            if (!File.Exists(config.FallbackProgram))
            {
                return Fail("没有匹配的规则，并且 fallbackProgram 不存在：" + config.FallbackProgram);
            }

            return StartProgram(config.FallbackProgram, normalizedFilePath, null);
        }

        private static bool IsUsableRule(RouteRule rule)
        {
            if (rule == null || !rule.Enabled)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(rule.Folder) || string.IsNullOrWhiteSpace(rule.Program))
            {
                return false;
            }

            return Directory.Exists(rule.Folder) && File.Exists(rule.Program);
        }

        private static int GetFolderMatchLength(string folderPath)
        {
            try
            {
                return Path.GetFullPath(folderPath)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Length;
            }
            catch
            {
                return 0;
            }
        }

        private static bool IsFileUnderFolder(string normalizedFilePath, string folderPath)
        {
            try
            {
                var normalizedFolder = Path.GetFullPath(folderPath)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    + Path.DirectorySeparatorChar;

                return normalizedFilePath.StartsWith(normalizedFolder, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static RouteResult StartProgram(string programPath, string filePath, string matchedRuleName)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = programPath,
                    Arguments = QuoteArgument(filePath),
                    UseShellExecute = false
                };

                Process.Start(psi);

                return new RouteResult
                {
                    Success = true,
                    MatchedRuleName = matchedRuleName,
                    ProgramPath = programPath
                };
            }
            catch (Exception ex)
            {
                return Fail("启动外部程序失败：" + ex.Message);
            }
        }

        private static string QuoteArgument(string argument)
        {
            return "\"" + argument.Replace("\"", "\\\"") + "\"";
        }

        private static RouteResult Fail(string message)
        {
            return new RouteResult
            {
                Success = false,
                ErrorMessage = message
            };
        }
    }
}
