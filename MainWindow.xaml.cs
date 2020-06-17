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
            InitializeComponent();

            var viewModel = new ViewModel.ViewModelClass();
            DataContext = viewModel;
        }
    }
}
