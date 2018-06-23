using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace TaskBarAppIdAdjuster
{
    public class NativeWindowHelpers
    {
        public delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn,
            IntPtr lParam);

        public delegate bool Win32Callback(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.Dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr parentHandle, Win32Callback callback, IntPtr lParam);

        [DllImport("user32")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32")]
        public static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        public const int MF_BYCOMMAND = 0;
        public const int MF_DISABLED = 2;
        public const int SC_CLOSE = 0xF060;

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public const int SW_HIDE = 0;
        public const int SW_SHOW = 5;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        public static String GetWindowText(IntPtr handle)
        {        
            int capacity = GetWindowTextLength(handle) * 2;
            StringBuilder stringBuilder = new StringBuilder(capacity);
            GetWindowText(handle, stringBuilder, stringBuilder.Capacity);

            return stringBuilder.ToString();
        }

        public static IEnumerable<IntPtr> EnumerateProcessThreadWindowHandles(int processId)
        {
            var handles = new List<IntPtr>();

            foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
                EnumThreadWindows(thread.Id,
                    (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);

            return handles;
        }

        public static IEnumerable<IntPtr> EnumerateProcessWindowHandles(IntPtr parentHandle)
        {
            var handles = new List<IntPtr>();

            EnumChildWindows(parentHandle, (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);
            
            return handles;
        }
    }
}
