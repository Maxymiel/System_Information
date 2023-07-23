using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ConverterHTML
{
    public static class Program
    {
        public static string path = "";

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length != 0) { path = args[0].ToString(); }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Converter());
        }
    }
}
