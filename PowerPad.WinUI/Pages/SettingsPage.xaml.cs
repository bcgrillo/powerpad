using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Markup;
using System;
using PowerPad.Core.Services;
using PowerPad.Core.Models;
using System.IO;
using PowerPad.WinUI.Components;
using PowerPad.WinUI.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace PowerPad.WinUI.Pages
{
    internal sealed partial class SettingsPage : Page
    {
        private readonly SettingsViewModel _services;

        public SettingsPage()
        {
            this.InitializeComponent();

            _services = Ioc.Default.GetRequiredService<SettingsViewModel>();
        }
    }
}