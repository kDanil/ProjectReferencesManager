using Microsoft.Win32;
using ProjectReferencesManager.Model;
using ProjectReferencesManager.Tools;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace ProjectReferencesManager
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private Solution selectedSolution;

        private Project selectedProject;

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

        public Project SelectedProject
        {
            get
            {
                return this.selectedProject;
            }

            set
            {
                if (this.selectedProject != value)
                {
                    this.selectedProject = value;
                    this.PropertyChanged.Raise(() => this.SelectedProject);
                }
            }
        }

        private void LoadReferencedProjects(Project project)
        {
            var projectInfos = new ProjectFileReader().Read(this.SelectedSolution.FolderPath + Path.DirectorySeparatorChar + project.Path);

            var guids = projectInfos.Select(p => p.GUID).ToArray();

            project.ReferencedProjects = this.SelectedSolution.Projects.Where(p => guids.Contains(p.GUID))
                                                                                    .OrderBy(p => p.Name)
                                                                                    .ToArray();
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
                    FullPath = dialog.FileName,
                    Projects = this.LoadProjects(dialog.FileName)
                };

                this.LoadProjectReferences();
                this.FindDependentProjects();
            }
        }

        private void FindDependentProjects()
        {
            foreach (var project in this.SelectedSolution.Projects)
            {
                project.DependentProjects = this.SelectedSolution.Projects.Except(new[] { project })
                                              .Where(p => p.ReferencedProjects.Contains(project))
                                              .ToArray();
            }
        }

        private void LoadProjectReferences()
        {
            foreach (var project in this.SelectedSolution.Projects)
            {
                this.LoadReferencedProjects(project);
            }
        }

        private IEnumerable<Project> LoadProjects(string filePath)
        {
            return new SolutionFileReader().Read(filePath)
                                           .Select(p => new Project()
                                           {
                                               Name = p.Name,
                                               Path = p.Path,
                                               GUID = p.GUID
                                           }).OrderBy(p => p.Name)
                                           .ToArray();
        }
    }
}