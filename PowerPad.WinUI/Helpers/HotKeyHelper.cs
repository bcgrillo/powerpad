using Microsoft.UI.Xaml;
using WinRT.Interop;
using WinUIEx.Messaging;
using Windows.Win32.Foundation;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using H.NotifyIcon;
using System.Runtime.InteropServices;
using System;
using Windows.ApplicationModel.DataTransfer;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Helpers
{
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

        public static void SimulateCtrlC()
        {
            PInvoke.keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYDOWN, 0);
            PInvoke.keybd_event(VK_C, 0, KEYEVENTF_KEYDOWN, 0);
            PInvoke.keybd_event(VK_C, 0, KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP, 0);
            PInvoke.keybd_event(VK_CONTROL, 0, KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP, 0);
        }

        public static void SimulateCtrlV()
        {
            PInvoke.keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYDOWN, 0);
            PInvoke.keybd_event(VK_V, 0, KEYEVENTF_KEYDOWN, 0);
            PInvoke.keybd_event(VK_V, 0, KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP, 0);
            PInvoke.keybd_event(VK_CONTROL, 0, KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP, 0);
        }
    }
}