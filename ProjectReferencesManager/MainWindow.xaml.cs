using ProjectReferencesManager.Tools;
using System.Windows;

namespace ProjectReferencesManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var reader = new ProjectFileReader();
            this.DataContext = new MainWindowViewModel(reader, new CopyingManager(), new ReferencesModifier(reader));
        }
    }
}