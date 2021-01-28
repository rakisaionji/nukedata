using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NukeDataTool
{
    static class Program
    {
        // [DllImport("kernel32.dll")] static extern bool AttachConsole(int dwProcessId);
        // private const int ATTACH_PARENT_PROCESS = -1;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private delegate bool SetConsoleCtrlEventHandler(CtrlType sig);
        private static SetConsoleCtrlEventHandler HandlerRoutineCallback;

        [DllImport("kernel32.dll")] static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll", SetLastError = true)] static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
        [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("Kernel32")] private static extern bool SetConsoleCtrlHandler(SetConsoleCtrlEventHandler handler, bool add);

        const int SW_HIDE = 0;
        // const int SW_SHOW = 5;

        internal static bool IsAuto { get; private set; }
        internal static bool IsNoGui { get; private set; }
        internal static bool IsSilent { get; private set; }
        internal static bool IsSplash { get; private set; }

        // public static string PathKey { get; private set; }
        // public static string PathSrc { get; private set; }
        // public static string PathDst { get; private set; }

        private static FrmMain frmMain;

        private static void ParseArgs(string[] args)
        {
            var argsLen = args.Length;
            for (int i = 0; i < argsLen; i++)
            {
                var f = args[i].Replace('\\', '-').Replace('/', '-').ToLower();
                switch (f)
                {
                    case "-k": // Key
                    case "-key":
                        if ((i + 1) < argsLen)
                        {
                            var file = args[i + 1];
                            if (File.Exists(file)) frmMain.txtKey.Text = file;
                        }
                        break;
                    case "-i": // Input
                    case "-in":
                        if ((i + 1) < argsLen)
                        {
                            var file = args[i + 1];
                            if (File.Exists(file)) frmMain.txtSrc.Text = file;
                        }
                        break;
                    case "-o": // Output
                    case "-out":
                        if ((i + 1) < argsLen) frmMain.txtDst.Text = args[i + 1];
                        break;
                    case "-v": // Splash
                    case "-ver":
                        IsSplash = true; break;
                    case "-c": // No Gui
                    case "-cli":
                        IsNoGui = true; break;
                    case "-s": // Silent
                    case "-silent":
                        IsSilent = true; break;
                    case "-a": // Auto
                    case "-auto":
                        IsAuto = true; break;
                    default:
                        break;
                }
            }
        }

        private static bool HandlerRoutine(CtrlType signal)
        {
            switch (signal)
            {
                case CtrlType.CTRL_BREAK_EVENT:
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                    frmMain.FscryptDecrypt_Cancel();
                    Console.WriteLine("\nProcess is terminated.");
                    Environment.Exit(0);
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            frmMain = new FrmMain();
            ParseArgs(args);
            if (IsNoGui)
            {
                // Redirect console output to parent process
                // Must be before any calls to Console.WriteLine()
                // AttachConsole(ATTACH_PARENT_PROCESS);
                // Establish an event handler to process key press events.
                HandlerRoutineCallback = new SetConsoleCtrlEventHandler(HandlerRoutine);
                SetConsoleCtrlHandler(HandlerRoutineCallback, true);
                // Run Console Application code
                frmMain.btnDecrypt_Click(null, null);
            }
            else
            {
                // Check if Console window is created, then hide it
                var hWnd = GetConsoleWindow();
                if (hWnd != IntPtr.Zero)
                {
                    var currentProcess = Process.GetCurrentProcess().Id;
                    GetWindowThreadProcessId(hWnd, out uint consoleProcess);
                    if (consoleProcess == currentProcess) ShowWindow(hWnd, SW_HIDE);
                }
                // Launch the WinForms application like normal
                Application.Run(frmMain);
            }
        }
    }

    static class InvokeExtension
    {
        public static void InvokeIfRequired<T>(this T obj, Action<T> action)
            where T : ISynchronizeInvoke
        {
            if (obj.InvokeRequired)
            {
                obj.Invoke(action, new object[] { obj });
            }
            else
            {
                action(obj);
            }
        }

        public static TOut InvokeIfRequired<TIn, TOut>(this TIn obj, Func<TIn, TOut> func)
            where TIn : ISynchronizeInvoke
        {
            return obj.InvokeRequired
                ? (TOut)obj.Invoke(func, new object[] { obj })
                : func(obj);
        }
    }

    static class FormatFactory
    {
        /// <summary>
        ///     <para>Convert a number in byte unit to another unit.</para>
        ///     <para>See more at: http://loliraki.tk/2011/10/16/php-convert-bytes-1 </para>
        /// </summary>
        /// <param name="bytes">File size in byte unit.</param>
        /// <param name="offset">The target unit you want to convert <paramref name="bytes" /> to.</param>
        /// <returns><paramref name="bytes" /> converted to a possible or defined unit if success.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        internal static string FormatSize(long bytes, int offset = -1)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            // int c = 0;
            if (offset < 0)
            {
                object[] r = { bytes, units[0] };
                for (int k = 0; k < units.Length; k++)
                {
                    if ((bytes / Math.Pow(1024, k)) >= 1)
                    {
                        r[0] = bytes / Math.Pow(1024, k);
                        r[1] = units[k];
                        // c++;
                    }
                }
                return String.Format("{0:N} {1}", r[0], r[1]);
            }
            if (offset < 9)
            {
                return (bytes / Math.Pow(1024, offset)).ToString("N") + " " + units[offset];
            }
            throw new ArgumentOutOfRangeException("offset");
        }
    }
}
