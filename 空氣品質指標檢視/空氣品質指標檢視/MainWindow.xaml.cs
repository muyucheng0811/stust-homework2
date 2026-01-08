using System.Windows;
using YourNamespace.ViewModels;

namespace YourNamespace
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _vm;

            Loaded += async (_, __) => await _vm.LoadAsync();
        }
    }
}
