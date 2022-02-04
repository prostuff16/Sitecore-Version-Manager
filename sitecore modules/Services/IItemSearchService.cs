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
    }
}
