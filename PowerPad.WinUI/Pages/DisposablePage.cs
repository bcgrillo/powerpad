using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.Helpers;
using System;

namespace PowerPad.WinUI.Pages
{
    /// <summary>
    /// Represents an abstract base class for pages that require disposal of resources.
    /// </summary>
    public abstract class DisposablePage : Page, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisposablePage"/> class.
        /// Registers the page with the <see cref="PageLifeCycleHelper"/>.
        /// </summary>
        protected DisposablePage()
        {
            PageLifeCycleHelper.RegisterPage(this);
        }

        /// <summary>
        /// Disposes the resources used by the page.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the page and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">A boolean value indicating whether to release managed resources.</param>
        protected abstract void Dispose(bool disposing);
    }
}
