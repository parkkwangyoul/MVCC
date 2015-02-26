using System.Windows;
using MVCC.ViewModel;

using System.Threading;

namespace MVCC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        /// 

        Globals globals = Globals.Instance;

        public MainWindow()
        {
            // 로고시간 길게
            Thread.Sleep(1000);

            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
        }

        private void OnDragMoveWindow(object sender, RoutedEventArgs e)
        {
            this.DragMove();
        }

        private void OnMinimizeWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void OnMaximizeWindow(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
        }
        private void OnCloseWindow(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < globals.SerialPortList.Count; i++)
            {
                globals.SerialPortList[i].Close();
            }

            this.Close();
        }
    }
}