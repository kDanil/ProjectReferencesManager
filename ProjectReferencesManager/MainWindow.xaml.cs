using ProjectReferencesManager.Tools;
using System.Windows;

namespace ProjectReferencesManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var viewModel = new ToolsContainer().MainWindowViewModel;

            this.Closing += (s, e) => e.Cancel = !viewModel.CanClose();

            this.DataContext = viewModel;
        }
    }
}