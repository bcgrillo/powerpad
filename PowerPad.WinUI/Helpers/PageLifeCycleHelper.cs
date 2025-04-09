using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.Pages;
using System;
using System.Collections.Generic;

namespace PowerPad.WinUI.Helpers
{
    public static class PageLifeCycleHelper
    {
        private readonly static Dictionary<Type, DisposablePage> _openPages = [];

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
