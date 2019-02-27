using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace GameTrace
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        App()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        int r = 0;

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Interlocked.Increment(ref r);
            Console.WriteLine("handled. {0}", r);
            Console.WriteLine("Terminating " + e.IsTerminating.ToString());

            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "Dead thread";

            while (true)
                Thread.Sleep(TimeSpan.FromHours(1));
            //Process.GetCurrentProcess().Kill();
        }
    }



    
}
