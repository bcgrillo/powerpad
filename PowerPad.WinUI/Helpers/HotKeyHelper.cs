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
        private const uint VK_C = 0x43;
        private const uint WM_HOTKEY = 0x0312;
        private static PopupWindow? _popupWindow;

        public static void Register(Window window)
        {
            var hwnd = new HWND(WindowNative.GetWindowHandle(window).ToInt32());
            var success = PInvoke.RegisterHotKey
            (
                hwnd,
                0,
                HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_SHIFT,
                VK_C
            );

            if (success)
            {
                var _monitor = new WindowMessageMonitor(hwnd);
                _monitor.WindowMessageReceived += OnWindowMessageReceived;
            }
            else
            {
                throw new COMException("Failed to register hotkey.", Marshal.GetLastWin32Error());
            }
        }

        private static async void OnWindowMessageReceived(object? _, WindowMessageEventArgs eventArgs)
        {
            if (eventArgs.Message.MessageId == WM_HOTKEY)
            {
                //await SimulateCtrlC();

                _popupWindow ??= new PopupWindow();

                _popupWindow.Invoke();               

                DataPackageView dataPackageView = Clipboard.GetContent();
                _popupWindow.SetContent(await dataPackageView.GetTextAsync());
            }
        }

        //private static async Task SimulateCtrlC()
        //{
        //    const byte VK_CONTROL = 0x11;
        //    const byte VK_C = 0x43;
        //    const uint KEYEVENTF_KEYDOWN = 0x0000;

        //    PInvoke.keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYDOWN, 0);
        //    PInvoke.keybd_event(VK_C, 0, KEYEVENTF_KEYDOWN, 0);
        //    PInvoke.keybd_event(VK_C, 0, KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP, 0);
        //    PInvoke.keybd_event(VK_CONTROL, 0, KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP, 0);
        //}
    }
}
