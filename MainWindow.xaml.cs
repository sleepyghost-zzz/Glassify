using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Glassify
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern int SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("dwmapi.dll")]
        private static extern int DwmEnableBlurBehindWindow(IntPtr hWnd, ref DWM_BLURBEHIND blurBehind);

        private const int LWA_COLORKEY = 0x1;
        private const int LWA_ALPHA = 0x2;
        private const int WS_EX_LAYERED = 0x80000;

        [StructLayout(LayoutKind.Sequential)]
        private struct DWM_BLURBEHIND
        {
            public DwmBlurBehindFlags dwFlags;
            public bool fEnable;
            public IntPtr hRgnBlur;
            public bool fTransitionOnMaximized;
        }

        [Flags]
        private enum DwmBlurBehindFlags
        {
            DWM_BB_ENABLE = 0x1,
            DWM_BB_BLURREGION = 0x2,
            DWM_BB_TRANSITIONONMAXIMIZED = 0x4
        }

        public MainWindow()
        {
            InitializeComponent();

            // Create UI Elements
            Slider transparencySlider = new Slider { Minimum = 0, Maximum = 255, Width = 200, Margin = new Thickness(10) };
            Slider blurSlider = new Slider { Minimum = 0, Maximum = 100, Width = 200, Margin = new Thickness(10) };
            StackPanel stackPanel = new StackPanel { Orientation = Orientation.Vertical };
            stackPanel.Children.Add(new Label { Content = "Transparency" });
            stackPanel.Children.Add(transparencySlider);
            stackPanel.Children.Add(new Label { Content = "Blur" });
            stackPanel.Children.Add(blurSlider);
            Content = stackPanel;

            // Event Handlers for Sliders
            transparencySlider.ValueChanged += (s, e) => ApplyTransparency((byte)transparencySlider.Value);
            blurSlider.ValueChanged += (s, e) => ApplyBlur((int)blurSlider.Value);
        }

        private void ApplyTransparency(byte transparency)
        {
            IntPtr hwnd = FindFileExplorerWindow();
            if (hwnd != IntPtr.Zero)
            {
                SetLayeredWindowAttributes(hwnd, 0, transparency, LWA_ALPHA);
            }
        }

        private void ApplyBlur(int blurStrength)
        {
            IntPtr hwnd = FindFileExplorerWindow();
            if (hwnd != IntPtr.Zero)
            {
                DWM_BLURBEHIND blurBehind = new DWM_BLURBEHIND
                {
                    dwFlags = DwmBlurBehindFlags.DWM_BB_ENABLE,
                    fEnable = blurStrength > 0,
                    hRgnBlur = IntPtr.Zero,
                    fTransitionOnMaximized = true
                };
                DwmEnableBlurBehindWindow(hwnd, ref blurBehind);
            }
        }

        private IntPtr FindFileExplorerWindow()
        {
            return FindWindow("CabinetWClass", null); // "CabinetWClass" is the class name for File Explorer
        }
    }
}
