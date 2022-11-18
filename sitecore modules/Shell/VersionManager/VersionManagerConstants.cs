using System;
using System.Linq;
using System.Xml;
using Sitecore.Common;
using Sitecore.Configuration;
using Sitecore.Data;

namespace Sitecore.VersionManager.sitecore_modules.Shell.VersionManager
{
    public static class VersionManagerConstants
    {
        public static readonly ID[] ConfigVersionTemplates = Settings.GetSetting("VersionManager.ValidTemplates", "")
            .Trim().Replace(" ", "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(i => Guid.Parse(i).ToID()).ToArray();

        public static readonly int MaxVersions = Settings.GetIntSetting("VersionManager.NumberOfVersionsToKeep", 5);

        public static readonly bool AutomaticCleanup =
            Settings.GetBoolSetting("VersionManager.AutomaticCleanupEnabled", false);

        public static readonly bool ArchiveDeletedVersions =
            Settings.GetBoolSetting("VersionManager.ArchiveDeletedVersions", true);

        public static readonly bool ShowContentEditorWarnings =
            Settings.GetBoolSetting("VersionManager.ShowContentEditorWarnings", true);

        public static readonly ID[] IgnoredFolders = Settings.GetSetting("VersionManager.IgnoredFolders", "").Trim()
            .Replace(" ", "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(i => Guid.Parse(i).ToID()).ToArray();

        public static readonly XmlNodeList Roots =
            Factory.GetConfigNodes("settings/setting[@name='VersionManager.Roots']/root");

        public static readonly bool ServiceRunInDebugMode =
            Settings.GetBoolSetting("VersionManager.ExecuteServiceRunInDebugMode", true);

        public static readonly int UpdatedWithinMinutes =
            Settings.GetIntSetting("VersionManager.UpdatedWithinMinutes", 0);

        public static class Indexes
        {
            public const string SitecoreMasterIndex = "sitecore_master_index";
            public const string SitecoreWebIndex = "sitecore_web_index";
        }
    }
}