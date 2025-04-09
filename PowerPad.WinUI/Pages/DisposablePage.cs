using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
