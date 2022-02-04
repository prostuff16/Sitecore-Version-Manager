using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.VersionManager.sitecore_modules.IoC;
using Sitecore.VersionManager.sitecore_modules.Shell.VersionManager;

namespace Sitecore.VersionManager.sitecore_modules.Services
{

    [Service(typeof(IItemSearchService))]
    class ItemSearchService : IItemSearchService
    {
        public IEnumerable<Guid> GetChildren(Item item)
        {
            try
            {
                var index = ContentSearchManager.GetIndex(VersionManagerConstants.Indexes.SitecoreMasterIndex);
                var query = PredicateBuilder.True<SearchResultItem>();
                query = query.And(i => i.Parent == item.ID);

                var itemList = new List<Guid>();

                using (var context = index.CreateSearchContext())
                {
                    var results = context.GetQueryable<SearchResultItem>().Where(query).GetResults();
                    if (results == null || !results.Any())
                    {
                        return itemList;
                    }

                    foreach (var resultItem in results)
                    {
                        itemList.Add(resultItem.Document.ItemId.ToGuid());
                    }
                }

                return itemList;
            }
            catch (Exception ex)
            {
                Log.Error($"", ex, this);
                return null;
            }
        }
    }
}