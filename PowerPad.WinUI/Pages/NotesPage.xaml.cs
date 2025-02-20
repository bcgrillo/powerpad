using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Markup;
using System;
using PowerPad.Core.Services;
using PowerPad.Core.Models;
using System.IO;

namespace PowerPad.WinUI.Pages
{
    internal sealed partial class NotesPage : Page
    {
        public ObservableCollection<CategoryBase> Categories { get; set; }
        public ObservableCollection<FileItem> FilesAndFolders { get; set; }
        public FileItem? CurrentFile { get; set; }

        public NotesPage()
        {
            this.InitializeComponent();

            var fileManager = new FileManager("D:\\OneDrive\\Escritorio\\Universidad\\PruebasTFG");
            FilesAndFolders = new ObservableCollection<FileItem>(fileManager.GetFilesAndFoldersHierarchy());
        }

        private void nvSample_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (CurrentFile != null) //&& CurrentFile.Status == FileStatus.Dirty
            {
                CurrentFile.Content = EditorView.GetContent();
                CurrentFile.AutoSave();
            }

            if (sender.SelectedItem is FileItem fileItem && !fileItem.IsFolder)
            {
                CurrentFile = fileItem;
                EditorView.SetContent(CurrentFile.Content);
            }
        }
    }

    public class CategoryBase { }

    public class Category : CategoryBase
    {
        public string Name { get; set; }
        public string Tooltip { get; set; }
        public Symbol Glyph { get; set; }
    }

    public class Separator : CategoryBase { }

    public class Header : CategoryBase
    {
        public string Name { get; set; }
    }

    [ContentProperty(Name = "ItemTemplate")]
    class MenuItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemTemplate { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            return item is Separator ? SeparatorTemplate : item is Header ? HeaderTemplate : ItemTemplate;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return item is Separator ? SeparatorTemplate : item is Header ? HeaderTemplate : ItemTemplate;
        }

        internal DataTemplate HeaderTemplate = (DataTemplate)XamlReader.Load(
            @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                   <NavigationViewItemHeader Content='{Binding Name}' />
                  </DataTemplate>");

        internal DataTemplate SeparatorTemplate = (DataTemplate)XamlReader.Load(
            @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                    <NavigationViewItemSeparator />
                  </DataTemplate>");
    }
}