using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;


namespace BuiKuVPN
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            //MainWindow MainWindow = new MainWindow();
            //MainWindow.Show();

            //SysMainWindow SysMainWindow = new SysMainWindow();
            //SysMainWindow.Show();
        }
    }


}
