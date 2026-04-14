using System;
using System.Collections.Generic;
using System.IO;

namespace WimMergeEngine
{
    public class ProjectBuilder
    {
        private readonly ILogger _logger;
        private readonly IsoManager _isoManager;
        private readonly DismManager _dismManager;
        private readonly CustomizationEngine _customizationEngine;

        public ProjectBuilder(ILogger logger)
        {
            _logger = logger;
            _isoManager = new IsoManager(logger);
            _dismManager = new DismManager(logger);
            _customizationEngine = new CustomizationEngine(logger);
        }

        public void Build(
            List<string> isoFiles,
            string outputIsoPath,
            string driverFolder,
            string bootMenuTitle,
            string eulaFile,
            string wallpaperFile,
            string iconFile,
            bool disableSigEnforcement,
            bool useUltraIso = false,
            string ultraIsoPath = "")
        {
            if (isoFiles.Count < 1) throw new ArgumentException("At least one ISO must be provided.");

            string workDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Work");
            string mountDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mount");
            string baseIsoExtractDir = Path.Combine(workDir, "BaseIso");

            // Clean up previous runs
            if (Directory.Exists(workDir)) Directory.Delete(workDir, true);
            if (Directory.Exists(mountDir)) Directory.Delete(mountDir, true);
            
            Directory.CreateDirectory(baseIsoExtractDir);
            Directory.CreateDirectory(mountDir);

            try
            {
                // 1. Extract Base ISO
                string baseIso = isoFiles[0];
                _logger.Log($"Step 1: Extracting Base ISO: {baseIso}");
                _isoManager.ExtractIso(baseIso, baseIsoExtractDir, useUltraIso, ultraIsoPath);

                string baseInstallWim = Path.Combine(baseIsoExtractDir, "sources", "install.wim");
                string baseBootWim = Path.Combine(baseIsoExtractDir, "sources", "boot.wim");

                // 2. Extract Additional ISOs and Merge WIMs
                for (int i = 1; i < isoFiles.Count; i++)
                {
                    string extraIso = isoFiles[i];
                    string extraExtractDir = Path.Combine(workDir, $"ExtraIso_{i}");
                    Directory.CreateDirectory(extraExtractDir);

                    _logger.Log($"Step 2: Extracting Additional ISO {i}: {extraIso}");
                    _isoManager.ExtractIso(extraIso, extraExtractDir, useUltraIso, ultraIsoPath);

                    string extraInstallWim = Path.Combine(extraExtractDir, "sources", "install.wim");
                    if (File.Exists(extraInstallWim))
                    {
                        string osName = Path.GetFileNameWithoutExtension(extraIso);
                        _logger.Log($"Merging WIM from {osName} into base...");
                        // Typically, install.wim has index 1 as the main OS, but we can assume index 1 for simplicity
                        _dismManager.ExportWim(extraInstallWim, 1, baseInstallWim, osName);
                    }
                }

                // 3. Driver Slipstreaming
                if (Directory.Exists(driverFolder) && Directory.GetFiles(driverFolder, "*.inf", SearchOption.AllDirectories).Length > 0)
                {
                    _logger.Log("Step 3: Slipstreaming Drivers...");

                    // Slipstream into Boot.wim (Index 1 and 2 usually)
                    for (int i = 1; i <= 2; i++)
                    {
                        try
                        {
                            _dismManager.MountWim(baseBootWim, i, mountDir);
                            _dismManager.AddDrivers(mountDir, driverFolder);
                            _dismManager.UnmountWim(mountDir, true);
                        }
                        catch (Exception ex)
                        {
                            _logger.Log($"Warning: Failed to slipstream drivers into boot.wim index {i}. {ex.Message}");
                            try { _dismManager.UnmountWim(mountDir, false); } catch { }
                        }
                    }

                    // Slipstream into Install.wim (All indexes)
                    // Note: In a real app, we'd query the number of indexes. Here we just try index 1 to 5 as an example.
                    for (int i = 1; i <= 5; i++)
                    {
                        try
                        {
                            _dismManager.MountWim(baseInstallWim, i, mountDir);
                            _dismManager.AddDrivers(mountDir, driverFolder);
                            _dismManager.UnmountWim(mountDir, true);
                        }
                        catch (Exception ex)
                        {
                            // Expected to fail when index is out of bounds
                            _logger.Log($"Finished slipstreaming install.wim or index {i} does not exist. {ex.Message}");
                            try { _dismManager.UnmountWim(mountDir, false); } catch { }
                            break; 
                        }
                    }
                }

                // 4. Customizations
                _logger.Log("Step 4: Applying Customizations...");
                string bootBcdPath = Path.Combine(baseIsoExtractDir, "boot", "bcd");
                if (File.Exists(bootBcdPath))
                {
                    _customizationEngine.ApplyBootMenuBranding(bootBcdPath, bootMenuTitle);
                    if (disableSigEnforcement)
                    {
                        _customizationEngine.EnableTestModeInWinPE(bootBcdPath);
                    }
                }

                if (disableSigEnforcement)
                {
                    string setupScriptsDir = Path.Combine(baseIsoExtractDir, "sources", "$OEM$", "$$", "Setup", "Scripts");
                    Directory.CreateDirectory(setupScriptsDir);
                    _customizationEngine.EnableTestModeInInstalledOS(Path.Combine(setupScriptsDir, "SetupComplete.cmd"));
                }

                _customizationEngine.ReplaceEula(baseIsoExtractDir, eulaFile);
                _customizationEngine.CreateAutorunIcon(baseIsoExtractDir, iconFile);

                // Inject wallpaper into boot.wim index 2 (WinPE)
                if (!string.IsNullOrEmpty(wallpaperFile) && File.Exists(wallpaperFile))
                {
                    try
                    {
                        _dismManager.MountWim(baseBootWim, 2, mountDir);
                        _customizationEngine.InjectWallpaper(mountDir, wallpaperFile);
                        _dismManager.UnmountWim(mountDir, true);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"Warning: Failed to inject wallpaper. {ex.Message}");
                        try { _dismManager.UnmountWim(mountDir, false); } catch { }
                    }
                }

                // 5. Build Final ISO
                _logger.Log("Step 5: Building final ISO...");
                _isoManager.BuildIso(baseIsoExtractDir, outputIsoPath, bootMenuTitle, useUltraIso, ultraIsoPath);

                _logger.Log("Build Process Complete!");
            }
            finally
            {
                // Cleanup
                try { _dismManager.UnmountWim(mountDir, false); } catch { }
                try { if (Directory.Exists(workDir)) Directory.Delete(workDir, true); } catch { }
                try { if (Directory.Exists(mountDir)) Directory.Delete(mountDir, true); } catch { }
            }
        }
    }
}
