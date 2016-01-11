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
            var copyingManager = new CopyingManager();
            this.DataContext = new MainWindowViewModel(new SolutionLoader(reader), copyingManager, new ProjectsChangesManager(new ReferencesModifier(reader), copyingManager, new UserInteraction()));
        }
    }
}