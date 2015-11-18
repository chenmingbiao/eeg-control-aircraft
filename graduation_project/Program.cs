using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace graduation_project
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
            //login loginFrom = new login();
            //Application.Run(loginFrom);
            main mainForm = new main("liaochen" , "3243tsad");
            Application.Run(mainForm);
        }
    }
}
