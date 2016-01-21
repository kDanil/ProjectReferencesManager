using System.ComponentModel;
using System.Windows.Input;

namespace ProjectReferencesManager
{
    public class MainWindowCommands : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand OpenSolutionCommand { get; set; }

        public ICommand PasteProjectsCommand { get; set; }

        public ICommand RemoveProjectsCommand { get; set; }

        public ICommand RestoreProjectsCommand { get; set; }

        public ICommand ApplyProjectChangesCommand { get; set; }

        public ICommand RestoreProjectChangesCommand { get; set; }

        public ICommand CopyProjectsCommand { get; set; }
    }
}