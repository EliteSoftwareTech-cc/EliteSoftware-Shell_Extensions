using System;
using System.IO;

namespace WimMergeEngine
{
    public class CustomizationEngine
    {
        private readonly ILogger _logger;

        public CustomizationEngine(ILogger logger)
        {
            _logger = logger;
        }

        public void ApplyBootMenuBranding(string bcdPath, string title)
        {
            _logger.Log($"Applying boot menu title '{title}' to {bcdPath}...");
            ProcessHelper.RunCommand("bcdedit.exe", $"/store \"{bcdPath}\" /set {{default}} description \"{title}\"", _logger);
        }

        public void EnableTestModeInWinPE(string bcdPath)
        {
            _logger.Log($"Enabling Test Mode (Disabling Signature Enforcement) in WinPE BCD...");
            ProcessHelper.RunCommand("bcdedit.exe", $"/store \"{bcdPath}\" /set {{default}} testsigning on", _logger);
            ProcessHelper.RunCommand("bcdedit.exe", $"/store \"{bcdPath}\" /set {{default}} nointegritychecks on", _logger);
        }

        public void EnableTestModeInInstalledOS(string setupCompletePath)
        {
            _logger.Log($"Generating SetupComplete.cmd for installed OS at {setupCompletePath}...");
            string content = "@echo off\r\n" +
                             "bcdedit /set {default} testsigning on\r\n" +
                             "bcdedit /set {default} nointegritychecks on\r\n";
            File.WriteAllText(setupCompletePath, content);
        }

        public void ReplaceEula(string sourceDir, string eulaRtfPath)
        {
            if (string.IsNullOrEmpty(eulaRtfPath) || !File.Exists(eulaRtfPath)) return;

            string targetPath = Path.Combine(sourceDir, "sources", "license.rtf");
            if (File.Exists(targetPath))
            {
                _logger.Log($"Replacing EULA at {targetPath}...");
                File.Copy(eulaRtfPath, targetPath, true);
            }
        }

        public void InjectWallpaper(string mountDir, string wallpaperBmpPath)
        {
            if (string.IsNullOrEmpty(wallpaperBmpPath) || !File.Exists(wallpaperBmpPath)) return;

            string targetPath = Path.Combine(mountDir, "sources", "setup.bmp");
            string targetPathWin8 = Path.Combine(mountDir, "sources", "background.bmp");
            
            _logger.Log($"Injecting wallpaper {wallpaperBmpPath} into WinPE...");
            File.Copy(wallpaperBmpPath, targetPath, true);
            File.Copy(wallpaperBmpPath, targetPathWin8, true);
        }

        public void CreateAutorunIcon(string rootDir, string iconPath)
        {
            if (string.IsNullOrEmpty(iconPath) || !File.Exists(iconPath)) return;

            _logger.Log($"Applying custom drive icon from {iconPath}...");
            string targetIcon = Path.Combine(rootDir, "setup_icon.ico");
            File.Copy(iconPath, targetIcon, true);

            string autorunPath = Path.Combine(rootDir, "autorun.inf");
            string autorunContent = "[AutoRun]\r\n" +
                                    "icon=setup_icon.ico\r\n" +
                                    "action=Run Windows Setup\r\n" +
                                    "open=setup.exe\r\n";
            File.WriteAllText(autorunPath, autorunContent);
        }
    }
}
