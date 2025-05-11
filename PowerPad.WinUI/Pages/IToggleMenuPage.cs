namespace PowerPad.WinUI.Pages
{
    /// <summary>
    /// Defines the contract for a page that supports toggling the visibility of a navigation menu.
    /// </summary>
    public interface IToggleMenuPage
    {
        /// <summary>
        /// Gets the width of the navigation menu.
        /// </summary>
        double NavigationWidth { get; }

        /// <summary>
        /// Toggles the visibility of the navigation menu.
        /// </summary>
        void ToggleNavigationVisibility();
    }
}
