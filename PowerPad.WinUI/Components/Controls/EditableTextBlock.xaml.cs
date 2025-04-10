using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;

namespace PowerPad.WinUI.Components.Controls
{
    public partial class EditableTextBlock : UserControl
    {
        private readonly EditableTextBlockState _state;
        private bool _focus;
        private bool _pointerOver;

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

        public string? PlaceholderText
        {
            get => (string)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.Register(nameof(PlaceholderText), typeof(string), typeof(EditableTextBlockState), new(null));

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
            if (_state.IsEditing && ConfirmOnLostFocus) Confirm();
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
            var maskedValue = new string('•', Math.Min(value.Length, 50));
            return maskedValue;
        }

        private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            _pointerOver = true;
            UpdateEditButtonOpacity();
        }

        private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            _pointerOver = false;
            UpdateEditButtonOpacity();
        }

        private void EditButton_GotFocus(object sender, RoutedEventArgs e)
        {
            _focus = true;
            UpdateEditButtonOpacity();
        }

        private void EditButton_LostFocus(object sender, RoutedEventArgs e)
        {
            _focus = false;
            UpdateEditButtonOpacity();
        }

        private void UpdateEditButtonOpacity() => EditButton.Opacity = _focus || _pointerOver ? 1 : 0;
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
