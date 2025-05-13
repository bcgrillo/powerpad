using PowerPad.WinUI.Pages;
using System;
using System.Collections.Generic;

namespace PowerPad.WinUI.Helpers
{
    /// <summary>
    /// Provides helper methods to manage the lifecycle of pages, ensuring proper disposal of resources.
    /// </summary>
    public static class PageLifeCycleHelper
    {
        private static readonly Dictionary<Type, DisposablePage> _openPages = [];

        /// <summary>
        /// Registers a page and ensures any previously registered page of the same type is disposed.
        /// </summary>
        /// <param name="page">The page to register. Must not be null.</param>
        public static void RegisterPage(DisposablePage page)
        {
            ArgumentNullException.ThrowIfNull(page);

            var pageType = page.GetType();

            if (_openPages.TryGetValue(pageType, out DisposablePage? value))
            {
                value.Dispose();
                _openPages[pageType] = page;
            }
            else
            {
                _openPages.Add(pageType, page);
            }
        }
    }
}
