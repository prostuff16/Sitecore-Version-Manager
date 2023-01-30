//-------------------------------------------------------------------------------------------------
// <copyright file="VersionManager.cs" company="Sitecore A/S">
// Copyright (C) 2010 by Sitecore A/S
// </copyright>
//-------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Sitecore.Common;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Data.Serialization;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.VersionManager.sitecore_modules.Services;
using Sitecore.VersionManager.sitecore_modules.Shell.VersionManager;

namespace Sitecore.VersionManager
{
    /// <summary>
    ///     Class that will manage deleting, serialization, finding items
    /// </summary>
    public class VersionManager
    {
        /// <summary>
        ///     Contains overflowed items. As key used Item.ID+"^"+Item.Language
        /// </summary>
        private static readonly Dictionary<string, Item> SourceList = new Dictionary<string, Item>();

        private static bool _isDisabled;

        #region Properties

        /// <summary>
        ///     Gets a list of overflowed items
        /// </summary>
        public static List<Item> ItemVersions
        {
            get
            {
                if (SourceList == null || SourceList.Count == 0)
                {
                    GetItemVersions();
                }

                return new List<Item>(SourceList.Values);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether Version Manager is disabled.
        /// </summary>
        public static bool IsDisabled
        {
            get
            {
                _isDisabled = VersionManagerConstants.Roots.Count == 0 || VersionManagerConstants.MaxVersions < 1;
                return _isDisabled;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        ///     Gets an item with specified language and guid
        /// </summary>
        /// <param name="str">Contains item guid and item language</param>
        /// <returns>item from guid and language values</returns>
        public static Item GetItemFromStr(string str)
        {
            var itemParams = str.Split('^');
            var language = Language.Parse(itemParams[1]);
            var master = Factory.GetDatabase("master");
            var item = master.GetItem(itemParams[0], language);
            return item;
        }

        /// <summary>
        ///     Converts list of Item to the list of GridItem
        /// </summary>
        /// <returns>a list of a GridItem</returns>
        public static List<GridItem> GetGridItem()
        {
            var tempList = new List<GridItem>();
            foreach (var item in ItemVersions)
            {
                tempList.Add(new GridItem(item));
            }

            return tempList;
        }

        /// <summary>
        ///     Clear sourceList
        /// </summary>
        public static void Refresh()
        {
            SourceList?.Clear();
        }

        /// <summary>
        ///     Delete versions of item if item has more than maximum allowed number of versions.
        ///     Deleted versions are serialized.
        /// </summary>
        /// <param name="item">Current item</param>
        public static void DeleteItemVersions(Item item)
        {
            if (IsDisabled)
            {
                return;
            }

            var current = item.Versions.Count;
            if (current - VersionManagerConstants.MaxVersions > 0)
            {
                var versions = item.Versions.GetVersions(false);
                if (VersionManagerConstants.ArchiveDeletedVersions)
                {
                    _SerializeItemVersions(item, versions[0].Version.Number,
                        versions[versions.Length - 1].Version.Number);
                }

                for (var i = 0; i < current - VersionManagerConstants.MaxVersions; i++)
                {
                    versions[i].Versions.RemoveVersion();
                    Log.Info(
                        $"Version Manager: Removed version {i}; Item: {item.Paths.Path}; Language: {item.Language}",
                        "SitecoreVersionManager");
                }

                SourceList.Remove($"{item.ID}^{item.Language}");
            }
        }

        /// <summary>
        ///     Gets/removes all descendants overflowed versions of specified item
        /// </summary>
        public static void GetItemVersions()
        {
            var master = Factory.GetDatabase("master");

            if (VersionManagerConstants.ConfigVersionTemplates.Any())
            {
                Log.Info("Starting to get Items based on VersionManager.ValidTemplates", "SitecoreVersionManager");
                var itemSearchService = new ItemSearchService();

                var items = itemSearchService.GetGuids(_GetAllRoots());
                Log.Info($"{items.Count()} total Items to process", "SitecoreVersionManager");
                _CheckVersionFromSearchResults(items);
                Log.Info("Done getting Items", "SitecoreVersionManager");
            }
            else
            {
                foreach (var str in _GetAllRoots())
                {
                    Log.Info("Starting to get Items using VersionManager.Roots", "SitecoreVersionManager");
                    var startItem = master.GetItem(str);
                    _CheckVersion(startItem);
                    Log.Info("Done getting Items", "SitecoreVersionManager");
                }
            }
        }

        private static void _CheckVersionFromSearchResults(IEnumerable<Guid> guids)
        {
            try
            {
                var master = Factory.GetDatabase("master");
                foreach (var guid in guids)
                {
                    try
                    {
                        Log.Debug($"Getting item with guid {guid}", "SitecoreVersionManager");
                        var item = master.GetItem(guid.ToID());

                        foreach (var language in item.Languages)
                        {
                            Log.Debug($"Processing languages for {item.Paths.Path} : {item.ID}",
                                "SitecoreVersionManager");
                            var langItem = master.GetItem(item.ID, language);
                            //var containsTemplate = !VersionManagerConstants.ConfigVersionTemplates.Any() ||
                            //                       VersionManagerConstants.ConfigVersionTemplates
                            //                           .Contains(item.TemplateID);
                            if (langItem.Versions.Count > VersionManagerConstants.MaxVersions) // && containsTemplate)
                            {
                                SourceList.Add($"{langItem.ID}^{langItem.Language}", langItem);
                                Log.Debug($"Added item {langItem.ID}", "SitecoreVersionManager");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Debug($"Error Processing item with guid : {guid}", "SitecoreVersionManager");
                        Log.Error($"Exception for item with guid : {guid} : {e.StackTrace}", e,
                            "SitecoreVersionManager");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception in CheckVersionFromSearchResults", e, "SitecoreVersionManager");
            }
        }

        /// <summary>
        ///     Checks if the item is under the any root that are defined in the config file
        /// </summary>
        /// <param name="item"> The item for analyze </param>
        /// <returns> True if item is under some root, false in other way </returns>
        public static bool IsItemUnderRoots(Item item)
        {
            foreach (var root in _GetAllRoots())
            {
                if (item.Paths.FullPath.ToUpper().Contains(root.ToUpper()))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Private methods

        /// <summary>
        ///     delete roots that is childs of some other roots
        /// </summary>
        /// <param name="roots">List of roots</param>
        private static void _CheckRoots(List<string> roots)
        {
            var master = Factory.GetDatabase("master");
            for (var i = 0; i < roots.Count; i++)
            {
                for (var j = 0; j < roots.Count; j++)
                {
                    if (i != j)
                    {
                        if (roots[i].ToUpper().Contains(roots[j].ToUpper()))
                        {
                            roots.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }

            for (var i = 0; i < roots.Count; i++)
            {
                var home = master.GetItem(roots[i]);
                if (home == null)
                {
                    roots.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        ///     Adds overflowed versions of an item tree to the GridItem list
        /// </summary>
        /// <param name="item">processed item</param>
        private static void _CheckVersion(Item item)
        {
            try
            {
                var master = Factory.GetDatabase("master");
                foreach (var language in item.Languages)
                {
                    Log.Debug($"Processing languages for {item.Paths.Path} : {item.ID}", "SitecoreVersionManager");
                    var langItem = master.GetItem(item.ID, language);
                    var containsTemplate = !VersionManagerConstants.ConfigVersionTemplates.Any() ||
                                           VersionManagerConstants.ConfigVersionTemplates.Contains(item.TemplateID);
                    if (langItem.Versions.Count > VersionManagerConstants.MaxVersions && containsTemplate)
                    {
                        SourceList.Add($"{langItem.ID}^{langItem.Language}", langItem);
                        Log.Debug($"Added item {langItem.ID}", "SitecoreVersionManager");
                    }
                }

                var itemSearchService = new ItemSearchService();
                var itemGuids = itemSearchService.GetChildren(item);

                if (itemGuids.Any())
                {
                    var items = itemGuids.Select(i => master.GetItem(i.ToID()));
                    foreach (var item1 in items)
                    {
                        Log.Debug($"Processing item {item.Paths.Path} : {item1.ID}", "SitecoreVersionManager");
                        _CheckVersion(item1);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Debug($"Error Processing item {item.Paths.Path} : {item.ID}", "SitecoreVersionManager");
                Log.Debug($"Exception for item {item.ID} : {e.StackTrace}", "SitecoreVersionManager");
            }
        }

        /// <summary>
        ///     Get all valid roots from config
        /// </summary>
        /// <returns>roots from config</returns>
        private static List<string> _GetAllRoots()
        {
            var result = new List<string>();
            if (VersionManagerConstants.Roots.Count == 0)
            {
                _isDisabled = true;
                return result;
            }

            foreach (XmlNode node in VersionManagerConstants.Roots)
            {
                if (node.Attributes != null)
                {
                    result.Add(node.Attributes["value"].Value);
                }
            }

            _CheckRoots(result);
            return result;
        }

        /// <summary>
        ///     Serializes the version
        /// </summary>
        /// <param name="item">Version for serializing</param>
        /// <param name="first">First version number  </param>
        /// <param name="last">Last version number</param>
        private static void _SerializeItemVersions(Item item, int first, int last)
        {
            Assert.ArgumentNotNull(item, "item");
            var reference = new ItemReference(item.Database.Name, GetPath(item));
            Log.Info($"Serializing {reference}", "SitecoreVersionManager");
            var path = new StringBuilder("VersionManager/");
            path.Append(DateTime.Now.Year + "/");
            path.Append(DateTime.Now.Month + "/");
            path.Append(DateTime.Now.Day + "/");
            path.Append(item.Name + item.ID + "/");
            if (first != last)
            {
                path.Append("Versions_" + first + "-" + last);
            }
            else
            {
                path.Append("Versions_" + first);
            }

            Manager.DumpItem(PathUtils.GetFilePath(path.ToString()), item);
        }

        #endregion
    }
}