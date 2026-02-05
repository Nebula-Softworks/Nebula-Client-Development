using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Celestia_IDE.Core
{
    static class CaptureProtection
    {
        [DllImport("user32.dll")]
        public static extern bool SetWindowDisplayAffinity(
            IntPtr hWnd,
            uint dwAffinity
        );

        public const uint WDA_NONE = 0x0;
        public const uint WDA_EXCLUDEFROMCAPTURE = 0x11;
    }

    public static class WindowUtils
    {
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        public static IntPtr GetMainWindowHandle(int processId)
        {
            IntPtr found = IntPtr.Zero;

            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd))
                    return true;

                GetWindowThreadProcessId(hWnd, out uint pid);
                if (pid != processId)
                    return true;

                // optional: ensure it has a title
                var sb = new StringBuilder(256);
                GetWindowText(hWnd, sb, sb.Capacity);
                if (sb.Length == 0)
                    return true;

                found = hWnd;
                return false; // stop enumeration
            }, IntPtr.Zero);

            return found;
        }
    }

}