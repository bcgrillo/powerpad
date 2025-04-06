using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace PowerPad.WinUI.Components.Controls
{
    public sealed partial class EditableTextBlock : UserControl
    {
        private readonly EditableTextBlockState _state;

        public string? Value
        {
            get => (string?)GetValue(ValueProperty);
            set
            {
                SetValue(ValueProperty, value);
                IntegratedTextBox.Text = PasswordMode ? MaskedValue(value) : value;
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(string), typeof(EditableTextBlockState), new(null));

        public bool ConfirmOnLostFocus
        {
            get => (bool)GetValue(ConfirmOnLostFocusProperty);
            set => SetValue(ConfirmOnLostFocusProperty, value);
        }

        public static readonly DependencyProperty ConfirmOnLostFocusProperty =
            DependencyProperty.Register(nameof(ConfirmOnLostFocus), typeof(bool), typeof(EditableTextBlockState), new(false));

        public bool PasswordMode
        {
            get => (bool)GetValue(PasswordModeProperty);
            set => SetValue(PasswordModeProperty, value);
        }

        public static readonly DependencyProperty PasswordModeProperty =
            DependencyProperty.Register(nameof(PasswordMode), typeof(bool), typeof(EditableTextBlockState), new(false));

        public EditableTextBlock()
        {
            this.InitializeComponent();

            _state = new EditableTextBlockState();
        }

        private void IntegratedTextBox_KeyDown(object _, KeyRoutedEventArgs e)
        {
            if (_state.IsEditing)
            {
                if (e.Key == Windows.System.VirtualKey.Enter) Confirm();
                else if (e.Key == Windows.System.VirtualKey.Escape) Cancel();
            }
        }

        private void IntegratedTextBox_LostFocus(object _, RoutedEventArgs __)
        {
            if (ConfirmOnLostFocus) Confirm();
        }

        private void EnterEditMode()
        {
            IntegratedTextBox.Text = Value;
            _state.EnterEditMode();
            IntegratedTextBox.Focus(FocusState.Programmatic);
            IntegratedTextBox.SelectAll();
        }

        private void Confirm()
        {
            _state.ExitEditMode();
            Value = IntegratedTextBox.Text;
            if (PasswordMode) IntegratedTextBox.Text = MaskedValue(Value);
        }

        private void Cancel()
        {
            _state.ExitEditMode();
            IntegratedTextBox.Text = PasswordMode ? MaskedValue(Value) : Value;
        }

        private void Button_Click(object sender, RoutedEventArgs _)
        {
            if ((Button)sender == EditButton) EnterEditMode();
            else if ((Button)sender == ConfirmButton) Confirm();
            else if ((Button)sender == CancelButton) Cancel();
        }

        private static string MaskedValue(string? value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            var maskedValue = new string('•', value.Length);
            return maskedValue;
        }
    }

    public partial class EditableTextBlockState : ObservableObject
    {
        private static readonly Brush TRANSPARENT = new SolidColorBrush(Colors.Transparent);

        [ObservableProperty]
        private bool _isEditing;

        [ObservableProperty]
        private Brush _borderBrush = TRANSPARENT;

        public void EnterEditMode()
        {
            IsEditing = true;
            BorderBrush = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"];
        }

        public void ExitEditMode()
        {
            IsEditing = false;
            BorderBrush = TRANSPARENT;
        }
    }
}
