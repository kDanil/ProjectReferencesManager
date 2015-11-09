using Microsoft.Win32;
using ProjectReferencesManager.Model;
using ProjectReferencesManager.Tools;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ProjectReferencesManager
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private Solution selectedSolution;

        public MainWindowViewModel()
        {
            this.OpenSolutionCommand = new RelayCommand(this.OpenSolution);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public RelayCommand OpenSolutionCommand { get; private set; }

        public Solution SelectedSolution
        {
            get
            {
                return this.selectedSolution;
            }

            set
            {
                if (this.selectedSolution != value)
                {
                    this.selectedSolution = value;
                    this.PropertyChanged.Raise(() => this.SelectedSolution);
                }
            }
        }

        private void OpenSolution()
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.FileName = "*.sln";

            if (dialog.ShowDialog() == true)
            {
                this.SelectedSolution = new Solution()
                {
                    FullPath = dialog.SafeFileName,
                    Projects = this.LoadProjects(dialog.FileName)
                };
            }
        }

        private IEnumerable<Project> LoadProjects(string filePath)
        {
            return new SolutionFileReader().Read(filePath)
                                           .Select(p => new Project()
                                           {
                                               Name = p.Name
                                           }).OrderBy(p => p.Name)
                                           .ToArray();
        }
    }
}