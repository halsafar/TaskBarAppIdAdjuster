using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TaskBarAppIdAdjuster
{
    /// <summary>
    /// Handle setting up some of the native methods required to bind to an open console versus the debug-output window in VS.
    /// </summary>
    public class NativeConsole
    {
        [DllImport("kernel32.dll",
            EntryPoint = "GetStdHandle",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll",
            EntryPoint = "AllocConsole",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        private const int STD_OUTPUT_HANDLE = -11;
        private const int MY_CODE_PAGE = 437;



        /// <summary>
        /// Open a console window, bind stdout to it.
        /// Remove the window chrome 'X' button to prevent closing.
        /// </summary>
        public static void OpenConsole()
        {
            AllocConsole();

            IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, true);
            FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
            Encoding encoding = System.Text.Encoding.GetEncoding(MY_CODE_PAGE);
            StreamWriter standardOutput = new StreamWriter(fileStream, encoding);
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);

            // Prevent remove X in window chrome, closing the console this way closes the entire application
            IntPtr consoleHwnd = GetConsoleWindow();
            var sm = NativeWindowHelpers.GetSystemMenu(consoleHwnd, false);
            NativeWindowHelpers.EnableMenuItem(sm, NativeWindowHelpers.SC_CLOSE, NativeWindowHelpers.MF_BYCOMMAND | NativeWindowHelpers.MF_DISABLED);

            Console.WriteLine("Output is now redirected to the console.");
        }

        /// <summary>
        /// Close the console.
        /// </summary>
        public static void CloseConsole()
        {
            IntPtr consoleHwnd = GetConsoleWindow();
            NativeWindowHelpers.ShowWindow(consoleHwnd, NativeWindowHelpers.SW_HIDE);
        }
    }
}
