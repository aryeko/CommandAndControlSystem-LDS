using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace ControlApplication.DesktopClient
{
    public class PollingManager : IDisposable
    {
        /// <summary>
        /// Timer interval for getting all detections
        /// </summary>
        private Timer Timer { get; set; }
        internal const double TimeIntervalMinutes = 0.2;

        public PollingManager()
        {
            Timer = new Timer(ToDoFunc, null, TimeSpan.FromMinutes(TimeIntervalMinutes), TimeSpan.FromMinutes(TimeIntervalMinutes));
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
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(()=> GetMainWindow().LoadData()));
        }

        public void Dispose()
        {
            Timer.Dispose();
        }
    }
}
