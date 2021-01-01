using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Windows.Forms;

namespace NukePackTool
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmMain());
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
