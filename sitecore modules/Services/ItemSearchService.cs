using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.VersionManager.sitecore_modules.IoC;
using Sitecore.VersionManager.sitecore_modules.Shell.VersionManager;

namespace Sitecore.VersionManager.sitecore_modules.Services
{
    [Service(typeof(IItemSearchService))]
    internal class ItemSearchService : IItemSearchService
    {
        public IEnumerable<Guid> GetChildren(Item item)
        {
            try
            {
                var index = ContentSearchManager.GetIndex(VersionManagerConstants.Indexes.SitecoreMasterIndex);
                var query = PredicateBuilder.True<SearchResultItem>();
                query = query.And(i => i.Parent == item.ID);

                if (VersionManagerConstants.IgnoredFolders.Any())
                {
                    foreach (var ignoredFolder in VersionManagerConstants.IgnoredFolders)
                    {
                        query = query.And(i => i.TemplateId != ignoredFolder);
                    }
                }

                Log.Debug($"Query : {query}", "SitecoreVersionManager");
                var itemList = new List<Guid>();

                using (var context = index.CreateSearchContext())
                {
                    var results = context.GetQueryable<SearchResultItem>().Where(query).GetResults();
                    if (results == null || !results.Any())
                    {
                        Log.Debug("No results", "SitecoreVersionManager");
                        return itemList;
                    }

                    foreach (var resultItem in results)
                    {
                        itemList.Add(resultItem.Document.ItemId.ToGuid());
                    }
                }

                Log.Debug($"Items: {itemList.Count}", "SitecoreVersionManager");
                return itemList;
            }
            catch (Exception ex)
            {
                Log.Error("", ex, this);
                return null;
            }
        }

        public IEnumerable<Guid> GetGuids(List<string> roots)
        {
            try
            {
                var index = ContentSearchManager.GetIndex(VersionManagerConstants.Indexes.SitecoreMasterIndex);
                var query = PredicateBuilder.True<SearchResultItem>();
                var templatesQuery = PredicateBuilder.True<SearchResultItem>();
                var rootsQuery = PredicateBuilder.True<SearchResultItem>();

                if (VersionManagerConstants.ConfigVersionTemplates.Any())
                {
                    foreach (var configVersionTemplate in VersionManagerConstants.ConfigVersionTemplates)
                    {
                        templatesQuery = templatesQuery.Or(i => i.TemplateId == configVersionTemplate);
                    }
                }

                if (VersionManagerConstants.UpdatedWithinMinutes != 0)
                {
                    var lookbackDate = DateTime.Now.AddMinutes(-VersionManagerConstants.UpdatedWithinMinutes);
                    query = query.And(i =>
                        i.Updated > lookbackDate);
                }

                if (roots.Any())
                {
                    foreach (var root in roots)
                    {
                        rootsQuery = rootsQuery.Or(i => i.Path.Contains(root));
                    }
                }

                query = query.And(templatesQuery).And(rootsQuery);

                Log.Debug($"Query : {query}", "SitecoreVersionManager");
                var itemList = new List<Guid>();

                using (var context = index.CreateSearchContext())
                {
                    var results = context.GetQueryable<SearchResultItem>().Where(query);
                    if (results == null || !results.Any())
                    {
                        Log.Debug("No results", "SitecoreVersionManager");
                        return itemList;
                    }

                    Log.Debug($"{results.Count()} total results", "SitecoreVersionManager");
                    foreach (var resultItem in results)
                    {
                        Log.Debug(
                            $"ResultItem: {resultItem.ItemId.ToGuid()} with template {resultItem.TemplateName}:{resultItem.TemplateId}",
                            "SitecoreVersionManager");
                        itemList.Add(resultItem.ItemId.ToGuid());
                    }
                }

                Log.Debug($"Items: {itemList.Distinct().Count()}", "SitecoreVersionManager");
                return itemList.Distinct();
            }
            catch (Exception ex)
            {
                Log.Error("", ex, this);
                return null;
            }
        }
    }
}