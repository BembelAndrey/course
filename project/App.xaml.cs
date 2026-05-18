using System;
using System.IO;
using System.Windows;

namespace project
{
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            File.WriteAllText(@"C:\Users\Slabu\Desktop\OOP-course_project\project\project\crash.log", e.Exception.ToString());
            e.Handled = true;
            Environment.Exit(1);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            File.WriteAllText(@"C:\Users\Slabu\Desktop\OOP-course_project\project\project\crash.log", e.ExceptionObject.ToString());
            Environment.Exit(1);
        }
    }
}
