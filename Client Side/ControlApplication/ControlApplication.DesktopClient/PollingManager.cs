using System;
using System.Threading;
using System.Windows;

namespace ControlApplication.DesktopClient
{
    public class PollingManager : IDisposable
    {
        /// <summary>
        /// Timer interval for getting all detections
        /// </summary>
        private Timer Timer { get; set; }
        internal const double TIME_INTERVAL_MINUTES = 0.2;

        public PollingManager()
        {
            Timer = new Timer(ToDoFunc, null, TimeSpan.FromMinutes(TIME_INTERVAL_MINUTES), TimeSpan.FromMinutes(TIME_INTERVAL_MINUTES));
        }

        /// <summary>
        /// Gets the MainWindow control
        /// </summary>
        /// <returns>The MainWindow or null on error</returns>
        private static MainWindow GetMainWindow()
        {
            return Application.Current.MainWindow as MainWindow;
        }

        private void ToDoFunc(object obj)
        {
            //Console.WriteLine("*****Getting all detections******");
            Application.Current.Dispatcher.Invoke(()=> GetMainWindow().LoadData());
            //Application.Current.Dispatcher.Invoke(() => GetMainWindow().LoadDataFromClients());
        }

        public void Dispose()
        {
            Timer.Dispose();
        }
    }
}
