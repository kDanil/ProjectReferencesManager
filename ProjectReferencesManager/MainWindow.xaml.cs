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
            this.DataContext = new MainWindowViewModel(new SolutionLoader(reader), new CopyingManager(), new ProjectsChangesManager(new ReferencesModifier(reader)));
        }
    }
}