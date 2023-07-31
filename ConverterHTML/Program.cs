using System;
using System.Windows.Forms;

namespace ConverterHTML
{
    public static class Program
    {
        public static string path = "";

        /// <summary>
        ///     Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length != 0) path = args[0];

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Converter());
        }
    }
}