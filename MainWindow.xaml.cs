using MahApps.Metro.Controls;

namespace Main
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();

            var viewModel = new ViewModel.ViewModel();
            DataContext = viewModel;
        }
    }
}
