using System;
using System.Runtime.InteropServices;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using medi1.Pages; 

namespace medi1
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Hook into the MAUI window handler to maximize on startup
            WindowHandler.Mapper.AppendToMapping(
                nameof(IWindow), (handler, view) =>
            {
#if WINDOWS
                var nativeWindow = handler.PlatformView;
                nativeWindow.Activate();

                // Grab the Win32 HWND
                IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);

                // 3 = SW_MAXIMIZE
                ShowWindow(hWnd, 3);
#endif
            });

            // Navigate to your login page
            MainPage = new NavigationPage(new LoginPage());
        }

#if WINDOWS
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
#endif
    }
}
