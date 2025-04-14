using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.Helpers;
using System;

namespace PowerPad.WinUI.Pages
{
    public abstract class DisposablePage : Page, IDisposable
    {
        protected bool _disposed;

        protected DisposablePage()
        {
            PageLifeCycleHelper.RegisterPage(this);
        }

        public abstract void Dispose();
    }
}
