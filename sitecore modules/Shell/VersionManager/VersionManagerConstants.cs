using System.Xml;
using Sitecore.Configuration;

namespace Sitecore.VersionManager.sitecore_modules.Shell.VersionManager
{
    public static class VersionManagerConstants
    {
        public static readonly string[] ConfigVersionTemplates = Settings.GetSetting("VersionManager.ValidTemplates", "").Trim().Replace(" ", "").Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
        public static readonly int MaxVersions = Settings.GetIntSetting("VersionManager.NumberOfVersionsToKeep", 5);
        public static readonly bool AutomaticCleanup = Settings.GetBoolSetting("VersionManager.AutomaticCleanupEnabled", false);
        public static readonly bool ArchiveDeletedVersions = Settings.GetBoolSetting("VersionManager.ArchiveDeletedVersions", true);
        public static readonly bool ShowContentEditorWarnings = Settings.GetBoolSetting("VersionManager.ShowContentEditorWarnings", true);
        public static readonly XmlNodeList Roots = Factory.GetConfigNodes("settings/setting[@name='VersionManager.Roots']/root");

        public static class Indexes
        {
            public const string SitecoreMasterIndex = "sitecore_master_index";
            public const string SitecoreWebIndex = "sitecore_web_index";
        }
    }

    
}
