using Sitecore.Diagnostics;
using Sitecore.VersionManager.sitecore_modules.Shell.VersionManager;

namespace Sitecore.VersionManager.Service
{
    public class VersionManagerService
    {
        public void Run()
        {
            Log.Info("Starting versions cleanup", "SitecoreVersionManager");
            var items = VersionManager.ItemVersions;

            foreach (var item in items)
                if (VersionManagerConstants.ServiceRunInDebugMode)
                    Log.Info(
                        $"Running in Debug mode! Disable debug mode in the config to delete versions from {item.ID} : {item.Paths.FullPath}",
                        "SitecoreVersionManager");
                else
                    VersionManager.DeleteItemVersions(item);
            Log.Info($"Finished versions cleanup for {items.Count}", "SitecoreVersionManager");
        }
    }
}