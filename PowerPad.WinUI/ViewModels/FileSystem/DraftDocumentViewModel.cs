using CommunityToolkit.Mvvm.ComponentModel;

namespace PowerPad.WinUI.ViewModels.FileSystem
{
    /// <summary>
    /// ViewModel representing a draft document with content and navigation properties.
    /// </summary>
    public partial class DraftDocumentViewModel : ObservableObject
    {
        /// <summary>
        /// Gets or sets the current content of the draft document.
        /// </summary>
        [ObservableProperty]
        public partial string? Content { get; set; }

        /// <summary>
        /// Gets or sets the previous content of the draft document.
        /// </summary>
        [ObservableProperty]
        public partial string? PreviousContent { get; set; }

        /// <summary>
        /// Gets or sets the next content of the draft document.
        /// </summary>
        [ObservableProperty]
        public partial string? NextContent { get; set; }
    }
}
