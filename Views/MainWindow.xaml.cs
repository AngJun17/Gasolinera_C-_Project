using System.Windows;
using System.Windows.Input;
using GasoStation.ViewModels;

namespace GasoStation.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            else
                DragMove();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();
    }
}
