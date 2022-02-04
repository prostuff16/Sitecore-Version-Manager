//-------------------------------------------------------------------------------------------------
// <copyright file="VersionAddedHandler.cs" company="Sitecore A/S">
// Copyright (C) 2010 by Sitecore A/S
// </copyright>
//-------------------------------------------------------------------------------------------------

using System.Linq;
using Sitecore.VersionManager.sitecore_modules.Shell.VersionManager;

namespace Sitecore.VersionManager.Handlers
{
    using System;
    using Sitecore.Data.Items;
    using Sitecore.Events;
    using Sitecore.VersionManager;

    /// <summary>
    /// Represents a version added event. Used for deleting old versions.
    /// </summary>
    public class VersionAddedHandler
    {
        // Methods

        /// <summary>
        /// Called when the version has added
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="args">The arguments</param>
        protected void OnVersionAdded(object sender, EventArgs args)
        {
            if (args != null)
            {
                if (VersionManagerConstants.AutomaticCleanup)
                {
                    var item = Event.ExtractParameter(args, 0) as Item;
                    bool containsTemplate = item != null && (VersionManagerConstants.ConfigVersionTemplates.Any() ? VersionManagerConstants.ConfigVersionTemplates.Contains(item.TemplateID.ToString()) : true);
                    if (item != null && VersionManager.IsItemUnderRoots(item) && containsTemplate)
                    {
                        VersionManager.DeleteItemVersions(item);
                    }
                }
            }
        }
    }
}