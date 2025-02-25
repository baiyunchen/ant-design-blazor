﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Routing;

namespace AntBlazor
{
    public class MenuLink : AntDomComponentBase
    {
        private const string DefaultActiveClass = "active";
        private bool _isActive;
        private string _hrefAbsolute;
        private string _class;
        internal string Href => _hrefAbsolute;

        /// <summary>
        /// Gets or sets the CSS class name applied to the NavLink when the
        /// current route matches the NavLink href.
        /// </summary>
        [Parameter]
        public string ActiveClass { get; set; }

        /// <summary>
        /// Gets or sets the computed CSS class based on whether or not the link is active.
        /// </summary>
        protected string CssClass { get; set; }

        /// <summary>
        /// Gets or sets the child content of the component.
        /// </summary>
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        /// <summary>
        /// Gets or sets a value representing the URL matching behavior.
        /// </summary>
        [Parameter]
        public NavLinkMatch Match { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> Attributes { get; set; }

        [CascadingParameter]
        public MenuItem MenuItem { get; set; }

        [CascadingParameter]
        public Menu Menu { get; set; }

        [CascadingParameter]
        public AntButton Button { get; set; }

        [Inject] private NavigationManager NavigationManger { get; set; }

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            // We'll consider re-rendering on each location change
            NavigationManger.LocationChanged += OnLocationChanged;
            //if (Button != null)
            //{
            //    Button.Link = this;
            //}
        }

        /// <inheritdoc />
        protected override void OnParametersSet()
        {
            // Update computed state
            string href = (string)null;
            if (Attributes != null && Attributes.TryGetValue("href", out object obj))
            {
                href = Convert.ToString(obj, CultureInfo.CurrentCulture);
            }
            _hrefAbsolute = href == null ? null : NavigationManger.ToAbsoluteUri(href).AbsoluteUri;
            _isActive = ShouldMatch(NavigationManger.Uri);
            _class = (string)null;
            if (Attributes != null && Attributes.TryGetValue("class", out obj))
            {
                _class = Convert.ToString(obj, CultureInfo.CurrentCulture);
            }
            UpdateCssClass();
            if (MenuItem != null && _isActive && !MenuItem.IsSelected)
            {
                Menu?.SelectItem(MenuItem);
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            // To avoid leaking memory, it's important to detach any event handlers in Dispose()
            NavigationManger.LocationChanged -= OnLocationChanged;
            base.Dispose(disposing);
        }

        private void UpdateCssClass()
        {
            CssClass = _isActive ? CombineWithSpace(_class, ActiveClass ?? DefaultActiveClass) : _class;
        }

        private void OnLocationChanged(object sender, LocationChangedEventArgs args)
        {
            // We could just re-render always, but for this component we know the
            // only relevant state change is to the _isActive property.
            bool shouldBeActiveNow = ShouldMatch(args.Location);
            if (shouldBeActiveNow != _isActive)
            {
                _isActive = shouldBeActiveNow;
                UpdateCssClass();
                if (MenuItem != null)
                {
                    if (_isActive)
                    {
                        MenuItem.Select();
                        Menu.SelectItem(MenuItem);
                    }
                    else
                    {
                        MenuItem.Deselect();
                    }
                }

                StateHasChanged();
            }
        }

        private bool ShouldMatch(string currentUriAbsolute)
        {
            if (EqualsHrefExactlyOrIfTrailingSlashAdded(currentUriAbsolute))
            {
                return true;
            }
            if (Match == NavLinkMatch.Prefix
            && IsStrictlyPrefixWithSeparator(currentUriAbsolute, _hrefAbsolute))
            {
                return true;
            }
            return false;
        }

        private bool EqualsHrefExactlyOrIfTrailingSlashAdded(string currentUriAbsolute)
        {
            if (string.Equals(currentUriAbsolute, _hrefAbsolute, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (currentUriAbsolute.Length == _hrefAbsolute.Length - 1)
            {
                // Special case: highlight links to http://host/path/ even if you're
                // at http://host/path (with no trailing slash)
                //
                // This is because the router accepts an absolute URI value of "same
                // as base URI but without trailing slash" as equivalent to "base URI",
                // which in turn is because it's common for servers to return the same page
                // for http://host/vdir as they do for host://host/vdir/ as it's no
                // good to display a blank page in that case.
                if (_hrefAbsolute[^1] == '/'
                && _hrefAbsolute.StartsWith(currentUriAbsolute, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (builder != null)
            {
                builder.OpenElement(0, "a");
                builder.AddMultipleAttributes(1, Attributes);
                builder.AddAttribute(2, "class", CssClass);
                builder.AddContent(3, ChildContent);
                builder.CloseElement();
            }
        }

        private static string CombineWithSpace(string str1, string str2)
        => str1 == null ? str2
        : (str2 == null ? str1 : $"{str1} {str2}");

        private static bool IsStrictlyPrefixWithSeparator(string value, string prefix)
        {
            int prefixLength = prefix.Length;
            if (value.Length > prefixLength)
            {
                return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                && (
                // Only match when there's a separator character either at the end of the
                // prefix or right after it.
                // Example: "/abc" is treated as a prefix of "/abc/def" but not "/abcdef"
                // Example: "/abc/" is treated as a prefix of "/abc/def" but not "/abcdef"
                prefixLength == 0
                || !char.IsLetterOrDigit(prefix[prefixLength - 1])
                || !char.IsLetterOrDigit(value[prefixLength])
                );
            }
            else
            {
                return false;
            }
        }
    }
}
