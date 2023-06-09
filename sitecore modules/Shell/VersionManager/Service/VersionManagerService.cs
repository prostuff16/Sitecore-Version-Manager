using Sitecore.Diagnostics;
using Sitecore.VersionManager.sitecore_modules.Shell.VersionManager;

//Don't change this namespace or using this via a Sitecron Job will become impossible
namespace Sitecore.VersionManager.Service
{
    public class VersionManagerService
    {
        public void Run()
        {
            Log.Warn("Starting versions cleanup", null, "SitecoreVersionManager");
            var items = VersionManager.ItemVersions;

            foreach (var item in items)
            {
                if (VersionManagerConstants.ServiceRunInDebugMode)
                {
                    Log.Warn(
                        $"Running in Debug mode! Disable debug mode in the config to delete versions from {item.ID} : {item.Paths.FullPath}",
                        null, "SitecoreVersionManager");
                }
                else
                {
                    VersionManager.DeleteItemVersions(item);
                }
            }

            Log.Warn($"Finished version cleanup for {items.Count} items", null, "SitecoreVersionManager");
        }
    }
}