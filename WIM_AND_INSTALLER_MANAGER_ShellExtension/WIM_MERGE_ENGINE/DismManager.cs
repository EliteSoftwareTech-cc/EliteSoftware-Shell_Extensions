using System;
using System.IO;

namespace WimMergeEngine
{
    public class DismManager
    {
        private readonly ILogger _logger;

        public DismManager(ILogger logger)
        {
            _logger = logger;
        }

        public void MountWim(string wimPath, int index, string mountDir)
        {
            _logger.Log($"Mounting WIM: {wimPath} (Index: {index}) to {mountDir}...");
            ProcessHelper.RunCommand("dism.exe", $"/Mount-Wim /WimFile:\"{wimPath}\" /Index:{index} /MountDir:\"{mountDir}\"", _logger);
        }

        public void UnmountWim(string mountDir, bool commit)
        {
            string commitArg = commit ? "/Commit" : "/Discard";
            _logger.Log($"Unmounting WIM: {mountDir} (Commit: {commit})...");
            ProcessHelper.RunCommand("dism.exe", $"/Unmount-Wim /MountDir:\"{mountDir}\" {commitArg}", _logger);
        }

        public void AddDrivers(string mountDir, string driversPath)
        {
            _logger.Log($"Injecting drivers from {driversPath} into {mountDir}...");
            ProcessHelper.RunCommand("dism.exe", $"/Image:\"{mountDir}\" /Add-Driver /Driver:\"{driversPath}\" /Recurse", _logger);
        }

        public void ExportWim(string sourceWim, int index, string destWim, string newName)
        {
            _logger.Log($"Exporting WIM index {index} from {sourceWim} to {destWim} (Name: {newName})...");
            string args = $"/Export-Image /SourceImageFile:\"{sourceWim}\" /SourceIndex:{index} /DestinationImageFile:\"{destWim}\" /DestinationName:\"{newName}\"";
            ProcessHelper.RunCommand("dism.exe", args, _logger);
        }
    }
}
