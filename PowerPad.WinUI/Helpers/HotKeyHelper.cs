using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using WinRT.Interop;
using WinUIEx.Messaging;

namespace PowerPad.WinUI.Helpers
{
    /// <summary>
    /// Provides helper methods for registering and handling global hotkeys.
    /// </summary>
    public static class HotKeyHelper
    {
        private const byte VK_C = 0x43;
        private const byte VK_V = 0x56;
        private const byte VK_SPACE = 0x20;
        private const uint WM_HOTKEY = 0x0312;
        private const byte VK_CONTROL = 0x11;
        private const uint KEYEVENTF_KEYDOWN = 0x0000;
        private const int HOTKEY_C = 1;
        private const int HOTKEY_SPACE = 2;

        private static PopupWindow? _popupWindow;
        private static bool _registered;

        /// <summary>
        /// Registers or unregisters global hotkeys for the specified window.
        /// </summary>
        /// <param name="window">The window to associate the hotkeys with.</param>
        /// <param name="register">True to register the hotkeys, false to unregister them.</param>
        /// <exception cref="COMException">Thrown if registering the hotkeys fails.</exception>
        public static void Register(Window window, bool register)
        {
            var hwnd = new HWND(WindowNative.GetWindowHandle(window).ToInt32());

            if (register && !_registered)
            {
                var success = PInvoke.RegisterHotKey(
                    hwnd,
                    HOTKEY_C,
                    HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_SHIFT,
                    VK_C
                );

                if (success)
                {
                    success = PInvoke.RegisterHotKey(
                        hwnd,
                        HOTKEY_SPACE,
                        HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_SHIFT,
                        VK_SPACE
                    );
                }

                if (success)
                {
                    var _monitor = new WindowMessageMonitor(hwnd);
                    _monitor.WindowMessageReceived += OnWindowMessageReceived;

                    _registered = true;
                }
                else
                {
                    throw new COMException("Failed to register hotkey.", Marshal.GetLastWin32Error());
                }
            }
            else if (!register && _registered)
            {
                PInvoke.UnregisterHotKey(hwnd, HOTKEY_C);
                PInvoke.UnregisterHotKey(hwnd, HOTKEY_SPACE);

                _registered = false;
            }
        }

        /// <summary>
        /// Handles window messages and processes hotkey events.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="eventArgs">The event arguments containing the window message.</param>
        private static async void OnWindowMessageReceived(object? _, WindowMessageEventArgs eventArgs)
        {
            if (eventArgs.Message.MessageId == WM_HOTKEY)
            {
                var copyMode = eventArgs.Message.WParam == HOTKEY_C;

                if (copyMode)
                {
                    SimulateCtrlC();
                    await Task.Delay(300);
                }

                _popupWindow ??= new PopupWindow();

                if (copyMode)
                {
                    DataPackageView dataPackageView = Clipboard.GetContent();
                    _popupWindow.SetContent((await dataPackageView.GetTextAsync()) ?? string.Empty);
                }
                else
                {
                    _popupWindow.SetContent(string.Empty);
                }

                _popupWindow.ShowPopup();
            }
        }

        /// <summary>
        /// Simulates the Ctrl+C keyboard shortcut.
        /// </summary>
        public static void SimulateCtrlC()
        {
            PInvoke.keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYDOWN, 0);
            PInvoke.keybd_event(VK_C, 0, KEYEVENTF_KEYDOWN, 0);
            PInvoke.keybd_event(VK_C, 0, KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP, 0);
            PInvoke.keybd_event(VK_CONTROL, 0, KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP, 0);
        }

        /// <summary>
        /// Simulates the Ctrl+V keyboard shortcut.
        /// </summary>
        public static void SimulateCtrlV()
        {
            PInvoke.keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYDOWN, 0);
            PInvoke.keybd_event(VK_V, 0, KEYEVENTF_KEYDOWN, 0);
            PInvoke.keybd_event(VK_V, 0, KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP, 0);
            PInvoke.keybd_event(VK_CONTROL, 0, KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP, 0);
        }
    }
}