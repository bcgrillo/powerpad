using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;

namespace PowerPad.WinUI.Components.Controls
{
    /// <summary>
    /// Represents a custom control that allows editing of text with optional password masking and placeholder support.
    /// </summary>
    public partial class EditableTextBlock : UserControl
    {
        private readonly EditableTextBlockState _state;
        private bool _focus;
        private bool _pointerOver;

        /// <summary>
        /// Gets or sets the value of the text displayed in the control.
        /// </summary>
        public string? Value
        {
            get => (string?)GetValue(ValueProperty);
            set
            {
                SetValue(ValueProperty, value);
                IntegratedTextBox.Text = PasswordMode ? MaskedValue(value) : value;
            }
        }

        /// <summary>
        /// DependencyProperty for the Value property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(string), typeof(EditableTextBlock), new(null));

        /// <summary>
        /// Gets or sets a value indicating whether the text should be confirmed when the control loses focus.
        /// </summary>
        public bool ConfirmOnLostFocus
        {
            get => (bool)GetValue(ConfirmOnLostFocusProperty);
            set => SetValue(ConfirmOnLostFocusProperty, value);
        }

        /// <summary>
        /// DependencyProperty for the ConfirmOnLostFocus property.
        /// </summary>
        public static readonly DependencyProperty ConfirmOnLostFocusProperty =
            DependencyProperty.Register(nameof(ConfirmOnLostFocus), typeof(bool), typeof(EditableTextBlock), new(false));

        /// <summary>
        /// Gets or sets a value indicating whether the text should be displayed in password mode (masked).
        /// </summary>
        public bool PasswordMode
        {
            get => (bool)GetValue(PasswordModeProperty);
            set => SetValue(PasswordModeProperty, value);
        }

        /// <summary>
        /// DependencyProperty for the PasswordMode property.
        /// </summary>
        public static readonly DependencyProperty PasswordModeProperty =
            DependencyProperty.Register(nameof(PasswordMode), typeof(bool), typeof(EditableTextBlock), new(false));

        /// <summary>
        /// Gets or sets the placeholder text displayed when the control is empty.
        /// </summary>
        public string? PlaceholderText
        {
            get => (string)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        /// <summary>
        /// DependencyProperty for the PlaceholderText property.
        /// </summary>
        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.Register(nameof(PlaceholderText), typeof(string), typeof(EditableTextBlock), new(null));

        /// <summary>
        /// Gets or sets the foreground brush to be used when the control is forced to display a specific color.
        /// </summary>
        public Brush? ForcedForeground
        {
            get => (Brush?)GetValue(ForcedForegroundProperty);
            set => SetValue(ForcedForegroundProperty, value);
        }

        /// <summary>
        /// DependencyProperty for the ForcedForeground property.
        /// </summary>
        public static readonly DependencyProperty ForcedForegroundProperty =
            DependencyProperty.Register(nameof(ForcedForeground), typeof(Brush), typeof(EditableTextBlock), new(null));

        /// <summary>
        /// Occurs when the text is edited and confirmed.
        /// </summary>
        public event EventHandler? Edited;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditableTextBlock"/> class.
        /// </summary>
        public EditableTextBlock()
        {
            this.InitializeComponent();

            _state = new EditableTextBlockState();
        }

        /// <summary>
        /// Enters the edit mode, allowing the user to modify the text.
        /// </summary>
        public void EnterEditMode()
        {
            IntegratedTextBox.Text = Value;
            _state.EnterEditMode();
            IntegratedTextBox.Focus(FocusState.Programmatic);
            IntegratedTextBox.SelectAll();
        }

        /// <summary>
        /// Handles the KeyDown event for the integrated text box.
        /// </summary>
        private void IntegratedTextBox_KeyDown(object _, KeyRoutedEventArgs eventArgs)
        {
            if (_state.IsEditing)
            {
                if (eventArgs.Key == Windows.System.VirtualKey.Enter) Confirm();
                else if (eventArgs.Key == Windows.System.VirtualKey.Escape) Cancel();
            }
        }

        /// <summary>
        /// Handles the LostFocus event for the integrated text box.
        /// </summary>
        private void IntegratedTextBox_LostFocus(object _, RoutedEventArgs __)
        {
            if (_state.IsEditing && ConfirmOnLostFocus) Confirm();
        }

        /// <summary>
        /// Confirms the current text and exits edit mode.
        /// </summary>
        private void Confirm()
        {
            _state.ExitEditMode();
            Value = IntegratedTextBox.Text;
            if (PasswordMode) IntegratedTextBox.Text = MaskedValue(Value);

            Edited?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Cancels the current edit and reverts to the previous text.
        /// </summary>
        private void Cancel()
        {
            _state.ExitEditMode();
            IntegratedTextBox.Text = PasswordMode ? MaskedValue(Value) : Value;
        }

        /// <summary>
        /// Handles button click events for edit, confirm, and cancel actions.
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs __)
        {
            if ((Button)sender == EditButton) EnterEditMode();
            else if ((Button)sender == ConfirmButton) Confirm();
            else if ((Button)sender == CancelButton) Cancel();
        }

        /// <summary>
        /// Masks the given value with asterisks for password mode.
        /// </summary>
        private static string MaskedValue(string? value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            var maskedValue = new string('•', Math.Min(value.Length, 50));
            return maskedValue;
        }

        /// <summary>
        /// Handles the PointerEntered event to update the edit button opacity.
        /// </summary>
        private void UserControl_PointerEntered(object _, PointerRoutedEventArgs __)
        {
            _pointerOver = true;
            UpdateEditButtonOpacity();
        }

        /// <summary>
        /// Handles the PointerExited event to update the edit button opacity.
        /// </summary>
        private void UserControl_PointerExited(object _, PointerRoutedEventArgs __)
        {
            _pointerOver = false;
            UpdateEditButtonOpacity();
        }

        /// <summary>
        /// Handles the GotFocus event for the edit button.
        /// </summary>
        private void EditButton_GotFocus(object _, RoutedEventArgs __)
        {
            _focus = true;
            UpdateEditButtonOpacity();
        }

        /// <summary>
        /// Handles the LostFocus event for the edit button.
        /// </summary>
        private void EditButton_LostFocus(object _, RoutedEventArgs __)
        {
            _focus = false;
            UpdateEditButtonOpacity();
        }

        /// <summary>
        /// Updates the opacity of the edit button based on focus and pointer state.
        /// </summary>
        private void UpdateEditButtonOpacity() => EditButton.Opacity = _focus || _pointerOver ? 1 : 0;
    }

    /// <summary>
    /// Represents the state of the <see cref="EditableTextBlock"/> control.
    /// </summary>
    public partial class EditableTextBlockState : ObservableObject
    {
        private static readonly Brush TRANSPARENT = new SolidColorBrush(Colors.Transparent);

        /// <summary>
        /// Gets or sets a value indicating whether the control is in edit mode.
        /// </summary>
        [ObservableProperty]
        public partial bool IsEditing { get; set; }

        /// <summary>
        /// Gets or sets the border brush of the control.
        /// </summary>
        [ObservableProperty]
        public partial Brush BorderBrush { get; set; } = TRANSPARENT;

        /// <summary>
        /// Enters the edit mode by updating the state and border brush.
        /// </summary>
        public void EnterEditMode()
        {
            IsEditing = true;
            BorderBrush = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"];
        }

        /// <summary>
        /// Exits the edit mode by resetting the state and border brush.
        /// </summary>
        public void ExitEditMode()
        {
            IsEditing = false;
            BorderBrush = TRANSPARENT;
        }
    }
}