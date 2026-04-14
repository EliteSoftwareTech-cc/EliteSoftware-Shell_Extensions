using System;
using System.IO;

namespace WimMergeEngine
{
    public class IsoManager
    {
        private readonly ILogger _logger;

        public IsoManager(ILogger logger)
        {
            _logger = logger;
        }

        public void ExtractIso(string isoPath, string outputDir, bool useUltraIso, string ultraIsoPath)
        {
            if (useUltraIso && !string.IsNullOrEmpty(ultraIsoPath))
            {
                _logger.Log($"Extracting {isoPath} using UltraISO...");
                string args = $"-imax -in \"{isoPath}\" -d \"{outputDir}\"";
                ProcessHelper.RunCommand(ultraIsoPath, args, _logger);
            }
            else
            {
                string sevenZipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "7z.exe");
                if (!File.Exists(sevenZipPath))
                {
                    throw new FileNotFoundException($"7z.exe not found at {sevenZipPath}");
                }

                _logger.Log($"Extracting {isoPath} using 7z...");
                string args = $"x \"{isoPath}\" -o\"{outputDir}\" -y";
                ProcessHelper.RunCommand(sevenZipPath, args, _logger);
            }
        }

        public void BuildIso(string sourceDir, string outputIsoPath, string label, bool useUltraIso, string ultraIsoPath)
        {
            if (useUltraIso && !string.IsNullOrEmpty(ultraIsoPath))
            {
                _logger.Log($"Building ISO {outputIsoPath} using UltraISO...");
                string args = $"-imax -d \"{sourceDir}\" -out \"{outputIsoPath}\"";
                ProcessHelper.RunCommand(ultraIsoPath, args, _logger);
            }
            else
            {
                string oscdimgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "oscdimg.exe");
                if (!File.Exists(oscdimgPath))
                {
                    throw new FileNotFoundException($"oscdimg.exe not found at {oscdimgPath}");
                }

                string etfsboot = Path.Combine(sourceDir, "boot", "etfsboot.com");
                string efisys = Path.Combine(sourceDir, "efi", "microsoft", "boot", "efisys.bin");

                string bootArgs = $"-b\"{etfsboot}\"";
                if (File.Exists(efisys))
                {
                    bootArgs = $"-bootdata:2#p0,e,b\"{etfsboot}\"#pEF,e,b\"{efisys}\"";
                }

                _logger.Log($"Building ISO {outputIsoPath} using oscdimg...");
                string args = $"-m -o -u2 -udfver102 {bootArgs} -l\"{label}\" \"{sourceDir}\" \"{outputIsoPath}\"";
                ProcessHelper.RunCommand(oscdimgPath, args, _logger);
            }
        }
    }
}
