using System.Windows;
using FaultIndicator_MainIPConfig.ViewModels;
using FaultIndicator_MainIPConfig.Interfaces;

namespace FaultIndicator_MainIPConfig
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMessageBoxes
    {
        public MainWindow()
        {
            InitializeComponent();
            //заисимость для работы скроллинга
            ((MainViewModel)DataContext).Messages.MessageBoxes = this;
        }

        public void ScrollToBottom()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                consoleBox.ScrollToEnd();
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //необходимо чтобы приложение не висело "в воздухе"
            //т.к. получение сообщений реализуется отдельным потоком
            ((MainViewModel)DataContext).SerialPort.CloseAll();
            Application.Current.Shutdown();
        }
    }
}
