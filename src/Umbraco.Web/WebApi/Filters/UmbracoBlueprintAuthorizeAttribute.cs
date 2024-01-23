﻿using System;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Composing;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Identical to UmbracoTreeAuthorizeAttribute, except it will default it's behavior to allow everything by
    /// users with access to the content tree, if the setting DowngradeBlueprintSecurity is true
    /// 
    /// Ensures that the current user has access to the application for which the specified tree(s) belongs
    /// </summary>
    /// <remarks>
    /// This would allow a tree to be moved between sections
    /// Should only be used on PostSaveBlueprint and DeleteBlueprint
    /// </remarks>
    public sealed class UmbracoBlueprintAuthorizeAttribute : OverridableAuthorizationAttribute
    {
        /// <summary>
        /// Can be used by unit tests to enable/disable this filter
        /// </summary>
        internal static bool Enable = true;

        private readonly string[] _treeAliases;

        private IGlobalSettings _globalSettings;
        private IGlobalSettings GlobalSettings
        {
            get
            {
                return _globalSettings ?? (_globalSettings = Current.Factory.GetInstance<IGlobalSettings>());
            }
        }

        /// <summary>
        /// Constructor to set authorization to be based on a tree alias for which application security will be applied
        /// Overrides authorization to Constant.Tree.Content if the setting DowngradeBlueprintSecurity is true
        /// </summary>
        /// <param name="treeAliases">
        /// If the user has access to the application that the treeAlias is specified in, they will be authorized.
        /// Multiple trees may be specified.
        /// </param>
        public UmbracoBlueprintAuthorizeAttribute(params string[] treeAliases)
        {
            _treeAliases = treeAliases;

            if(GlobalSettings.DowngradeBlueprintSecurity)
            {
                _treeAliases = [Constants.Trees.Content];
            }
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (Enable == false)
            {
                return true;
            }

            var apps = _treeAliases.Select(x => Current.TreeService
                .GetByAlias(x))
                .WhereNotNull()
                .Select(x => x.SectionAlias)
                .Distinct()
                .ToArray();

            return Current.UmbracoContext.Security.CurrentUser != null
                   && apps.Any(app => Current.UmbracoContext.Security.UserHasSectionAccess(
                       app, Current.UmbracoContext.Security.CurrentUser));
        }
    }
}
