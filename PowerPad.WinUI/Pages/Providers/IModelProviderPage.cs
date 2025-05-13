using System;

namespace PowerPad.WinUI.Pages.Providers
{
    /// <summary>
    /// Defines the contract for a page that provides model-related functionality.
    /// </summary>
    public interface IModelProviderPage : IDisposable
    {
        /// <summary>
        /// Closes the model information viewer.
        /// </summary>
        void CloseModelInfoViewer();
    }
}