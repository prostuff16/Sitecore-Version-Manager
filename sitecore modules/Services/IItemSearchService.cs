using System;
using System.Collections.Generic;
using Sitecore.Data.Items;

namespace Sitecore.VersionManager.sitecore_modules.Services
{
    public interface IItemSearchService
    {
        /// <summary>
        ///     Returns a list of Guids that are children of the specified item
        /// </summary>
        /// <param name="item">Item to look for descendents under</param>
        /// <returns>List of Guids if any, empty list if none, and null if an error occurred.</returns>
        IEnumerable<Guid> GetChildren(Item item);

        /// <summary>
        ///     Returns a list of Guids from the sitecore_master_index that meet the criteria in the config file
        ///     using the availbale configs that can be specified
        /// </summary>
        /// <param name="roots"></param>
        /// <returns>List of Guids</returns>
        IEnumerable<Guid> GetGuids(List<string> roots);
    }
}