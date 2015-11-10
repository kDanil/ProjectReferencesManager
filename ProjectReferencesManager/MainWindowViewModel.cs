using Microsoft.Win32;
using ProjectReferencesManager.Model;
using ProjectReferencesManager.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectReferencesManager
{
    public enum ProjectListType
    {
        Solution,
        Referenced,
        Dependent
    }

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly CopyingManager copyingManager;
        private Solution selectedSolution;
        private Project selectedProject;
        private readonly ReferencesModifier modifier;
        private readonly ProjectFileReader projectReader;

        public MainWindowViewModel(ProjectFileReader projectReader, CopyingManager copingManager, ReferencesModifier modifier)
        {
            this.projectReader = projectReader;
            this.copyingManager = copingManager;
            this.modifier = modifier;

            this.OpenSolutionCommand = new RelayCommand(this.OpenSolution);
            this.CopyProjectsCommand = new RelayCommandWithParameter(this.CopyProjects);
            this.PasteProjectsCommand = new RelayCommandWithParameter(this.PasteProjects, this.CanPasteProjects);
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

        public ICommand CopyProjectsCommand { get; private set; }

        public ICommand PasteProjectsCommand { get; private set; }

        private bool CanPasteProjects(object projectsListBox)
        {
            if (projectsListBox == null)
            {
                return false;
            }

            var listType = (ProjectListType)(projectsListBox as ListBox).Tag;

            return this.copyingManager.HasData() && listType != ProjectListType.Solution;
        }

        private void PasteProjects(object projectsListBox)
        {
            var listType = (ProjectListType)(projectsListBox as ListBox).Tag;

            var newProjects = this.copyingManager.Paste();

            switch (listType)
            {
                case ProjectListType.Referenced:

                    var projectsToAdd = newProjects.Except(this.SelectedProject.ReferencedProjects)
                                                   .Select(p => new NewProject(p));

                    this.SelectedProject.ReferencedProjects = this.SelectedProject.ReferencedProjects.Concat(projectsToAdd).ToArray();

                    break;
                case ProjectListType.Dependent:
                    break;
                default:
                    break;
            }
        }

        private void CopyProjects(object projectsListBox)
        {
            this.copyingManager.Copy((projectsListBox as ListBox).SelectedItems.OfType<IProject>());
        }

        private void LoadReferencedProjects(Project project)
        {
            var projectInfos = this.projectReader.Read(this.GetAbsoluteProjectPath(project));

            var guids = projectInfos.Select(p => p.GUID).ToArray();

            project.ReferencedProjects = this.SelectedSolution.Projects.Where(p => guids.Contains(p.GUID))
                                                                                    .OrderBy(p => p.Name)
                                                                                    .ToArray();
        }

        private string GetAbsoluteProjectPath(IProject project)
        {
            return this.SelectedSolution.FolderPath + Path.DirectorySeparatorChar + project.Path;
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