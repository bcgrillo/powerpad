using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.Helpers;
using System;

namespace PowerPad.WinUI.Pages
{
    public abstract class DisposablePage : Page, IDisposable
    {
        protected DisposablePage()
        {
            PageLifeCycleHelper.RegisterPage(this);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected abstract void Dispose(bool disposing);
    }
}
