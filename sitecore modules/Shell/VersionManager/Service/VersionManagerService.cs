﻿using Sitecore.Diagnostics;

namespace Sitecore.VersionManager.sitecore_modules.Shell.VersionManager.Service
{
    public class VersionManagerService
    {
        public VersionManagerService()
        {
        }

        public void Run()
        {
            Log.Info($"Starting versions cleanup","SitecoreVersionManager");
            var items = Sitecore.VersionManager.VersionManager.ItemVersions;

            foreach (var item in items)
            {
                Sitecore.VersionManager.VersionManager.DeleteItemVersions(item);
            }
            Log.Info($"Finished versions cleanup for {items.Count}", "SitecoreVersionManager");
        }
    }
}
