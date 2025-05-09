namespace PowerPad.WinUI.Pages
{
    public interface IToggleMenuPage
    {
        double NavigationWidth { get; }

        void ToggleNavigationVisibility();
    }
}
