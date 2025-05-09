using CommunityToolkit.Mvvm.ComponentModel;

namespace PowerPad.WinUI.ViewModels.FileSystem
{
    public partial class DraftDocumentViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string? Content { get; set; }

        [ObservableProperty]
        public partial string? PreviousContent { get; set; }

        [ObservableProperty]
        public partial string? NextContent { get; set; }
    }
}
