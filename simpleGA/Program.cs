using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace simpleGA
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }

    public class app
    {
        public static int value; 
        public static int value0;
    }
}
