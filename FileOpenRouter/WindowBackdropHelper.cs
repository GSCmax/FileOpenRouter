using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace FileOpenRouter
{
    public static class WindowBackdropHelper
    {
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
        private const int DWMWA_MICA_EFFECT = 1029;

        public static void Apply(Window window)
        {
            if (window == null)
            {
                return;
            }

            window.SourceInitialized -= OnSourceInitialized;
            window.SourceInitialized += OnSourceInitialized;
        }

        private static void OnSourceInitialized(object sender, EventArgs e)
        {
            var window = sender as Window;
            if (window == null)
            {
                return;
            }

            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero)
            {
                return;
            }

            var source = HwndSource.FromHwnd(hwnd);
            if (source != null && source.CompositionTarget != null)
            {
                source.CompositionTarget.BackgroundColor = Colors.Transparent;
            }

            window.Background = Brushes.Transparent;

            if (GetWindowsBuildNumber() >= 22000)
            {
                EnableWin11Backdrop(hwnd, DwmSystemBackdropType.MainWindow);
            }
            else
            {
                EnableWin10Blur(hwnd);
            }
        }

        private static void EnableWin11Backdrop(IntPtr hwnd, DwmSystemBackdropType dwmSystemBackdrop)
        {
            ExtendFrameIntoClientArea(hwnd);

            var darkMode = 0;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));

            var backdrop = (int)dwmSystemBackdrop;
            var result = DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int));
            if (result != 0)
            {
                var enabled = 1;
                DwmSetWindowAttribute(hwnd, DWMWA_MICA_EFFECT, ref enabled, sizeof(int));
            }
        }

        private static void ExtendFrameIntoClientArea(IntPtr hwnd)
        {
            var margins = new Margins
            {
                Left = -1,
                Right = -1,
                Top = -1,
                Bottom = -1
            };

            DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }

        private static void EnableWin10Blur(IntPtr hwnd)
        {
            var accent = new AccentPolicy
            {
                AccentState = AccentState.EnableAcrylicBlurBehind,
                GradientColor = unchecked((int)0x99F7F8FA)
            };

            var size = Marshal.SizeOf(typeof(AccentPolicy));
            var pointer = Marshal.AllocHGlobal(size);

            try
            {
                Marshal.StructureToPtr(accent, pointer, false);

                var data = new WindowCompositionAttributeData
                {
                    Attribute = WindowCompositionAttribute.AccentPolicy,
                    Data = pointer,
                    SizeOfData = size
                };

                SetWindowCompositionAttribute(hwnd, ref data);
            }
            finally
            {
                Marshal.FreeHGlobal(pointer);
            }
        }

        private static int GetWindowsBuildNumber()
        {
            var version = new OsVersionInfoEx();
            version.OSVersionInfoSize = Marshal.SizeOf(typeof(OsVersionInfoEx));

            return RtlGetVersion(ref version) == 0
                ? version.BuildNumber
                : Environment.OSVersion.Version.Build;
        }

        private enum DwmSystemBackdropType
        {
            Auto = 0,
            None = 1,
            MainWindow = 2,
            TransientWindow = 3,
            TabbedWindow = 4
        }

        private enum AccentState
        {
            Disabled = 0,
            EnableGradient = 1,
            EnableTransparentGradient = 2,
            EnableBlurBehind = 3,
            EnableAcrylicBlurBehind = 4
        }

        private enum WindowCompositionAttribute
        {
            AccentPolicy = 19
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Margins
        {
            public int Left;
            public int Right;
            public int Top;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct OsVersionInfoEx
        {
            public int OSVersionInfoSize;
            public int MajorVersion;
            public int MinorVersion;
            public int BuildNumber;
            public int PlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string CsdVersion;
            public ushort ServicePackMajor;
            public ushort ServicePackMinor;
            public ushort SuiteMask;
            public byte ProductType;
            public byte Reserved;
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            int attribute,
            ref int attributeValue,
            int attributeSize);

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins margins);

        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(
            IntPtr hwnd,
            ref WindowCompositionAttributeData data);

        [DllImport("ntdll.dll")]
        private static extern int RtlGetVersion(ref OsVersionInfoEx versionInfo);
    }
}
