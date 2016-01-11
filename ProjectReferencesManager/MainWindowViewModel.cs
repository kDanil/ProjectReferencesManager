using Microsoft.Win32;
using ProjectReferencesManager.Model;
using ProjectReferencesManager.Tools;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System;

namespace ProjectReferencesManager
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly CopyingManager copyingManager;
        private Project selectedProject;
        private Solution selectedSolution;
        private readonly ProjectsChangesManager changesManager;
        private readonly SolutionLoader solutionLoader;

        public MainWindowViewModel(
            SolutionLoader solutionLoader,
            CopyingManager copingManager,
            ProjectsChangesManager changesManager)
        {
            this.solutionLoader = solutionLoader;
            this.copyingManager = copingManager;
            this.changesManager = changesManager;

            this.Commands = new MainWindowCommands();

            this.Commands.OpenSolutionCommand = new RelayCommand(this.OpenSolution);
            this.Commands.CopyProjectsCommand = new RelayCommandWithParameter(this.CopyProjects, this.CanCopyProjects);
            this.Commands.PasteProjectsCommand = new RelayCommandWithParameter(p => { this.PasteProjects(p); this.RefreshChangesInformation(); }, this.CanPasteProjects);
            this.Commands.RemoveProjectsCommand = new RelayCommandWithParameter(p => { this.RemoveProjects(p); this.RefreshChangesInformation(); }, this.CanRemoveProjects);
            this.Commands.RestoreProjectsCommand = new RelayCommandWithParameter(p => { this.RestoreProjects(p); this.RefreshChangesInformation(); }, this.CanRestoreProjects);
            this.Commands.ApplyProjectChangesCommand = new RelayCommand(() => { this.ApplyProjectChanges(); this.RefreshChangesInformation(); }, this.CanApplyProjectChanges);
        }

        private void RestoreProjects(object projectsListBox)
        {
            var listBox = projectsListBox as ListBox;
            var listBoxType = (ProjectListType)listBox.Tag;
            var removedProjects = listBox.SelectedItems.OfType<RemovedProject>();

            this.changesManager.RestoreProjects(this.SelectedProject, removedProjects, listBoxType);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowCommands Commands { get; private set; }

        public int ChangesCount
        {
            get
            {
                if (this.SelectedSolution == null)
                {
                    return default(int);
                }

                return this.GetChangedProjects().Count();
            }
        }

        public bool IsChanges
        {
            get
            {
                return this.ChangesCount > 0;
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

                    this.changesManager.AssignSolution(this.selectedSolution);

                    this.PropertyChanged.Raise(() => this.SelectedSolution);
                }
            }
        }

        private bool CanRestoreProjects(object projectsListBox)
        {
            if (projectsListBox == null)
            {
                return false;
            }

            return (projectsListBox as ListBox).SelectedItems.OfType<RemovedProject>().Any();
        }

        private void AddToDependent(IEnumerable<IProject> newProjects)
        {
            var projectsToAdd = newProjects.Except(this.SelectedProject.DependentProjects)
                                                      .Select(p => new AddedProject(p));

            var projectsToRemove = this.SelectedProject.DependentProjects.Where(p => p is RemovedProject && projectsToAdd.Any(pp => pp.GUID == p.GUID));

            this.SelectedProject.DependentProjects = this.SelectedProject.DependentProjects.Except(projectsToRemove)
                                                                                           .Concat(projectsToAdd)
                                                                                           .ToArray();
        }

        private void AddToReferenced(IEnumerable<IProject> newProjects)
        {
            var projectsToAdd = newProjects.Except(this.SelectedProject.ReferencedProjects)
                                           .Select(p => new AddedProject(p));

            var projectsToRemove = this.SelectedProject.ReferencedProjects.Where(p => p is RemovedProject && projectsToAdd.Any(pp => pp.GUID == p.GUID));

            this.SelectedProject.ReferencedProjects = this.SelectedProject.ReferencedProjects.Except(projectsToRemove)
                                                                                             .Concat(projectsToAdd)
                                                                                             .ToArray();
        }

        private void ApplyProjectChanges()
        {
            this.changesManager.ApplyProjectChanges();
        }

        private bool CanApplyProjectChanges()
        {
            return this.IsChanges;
        }

        private bool CanCopyProjects(object projectsListBox)
        {
            if (projectsListBox == null)
            {
                return false;
            }

            return (projectsListBox as ListBox).SelectedItems.OfType<IProject>().Any(p => p is Project);
        }

        private bool CanPasteProjects(object type)
        {
            if (type == null)
            {
                return false;
            }

            var listType = (ProjectListType)type;

            return this.copyingManager.HasData() && listType != ProjectListType.Solution;
        }

        private bool CanRemoveProjects(object projectsListBox)
        {
            if (projectsListBox == null)
            {
                return false;
            }

            var listType = this.GetProjectListType(projectsListBox);

            var selectedProject = (projectsListBox as ListBox).SelectedItem;

            return listType != ProjectListType.Solution && selectedProject is Project || selectedProject is AddedProject;
        }

        private void CopyProjects(object projectsListBox)
        {
            this.copyingManager.Copy((projectsListBox as ListBox).SelectedItems.OfType<Project>());
        }



        private IEnumerable<IProject> GetChangedProjects()
        {
            var dependentProjects = this.SelectedSolution.Projects.SelectMany(p => p.DependentProjects.Where(pr => pr.IsChangedProject()));
            var referencedProjects = this.SelectedSolution.Projects.SelectMany(p => p.ReferencedProjects.Where(pr => pr.IsChangedProject()));

            return dependentProjects.Concat(referencedProjects);
        }

        private ProjectListType GetProjectListType(object projectsListBox)
        {
            return (ProjectListType)(projectsListBox as ListBox).Tag;
        }

        private void OpenSolution()
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.FileName = "*.sln";

            if (dialog.ShowDialog() == true)
            {
                this.SelectedSolution = this.solutionLoader.Load(dialog.FileName);
            }
        }

        private void PasteProjects(object type)
        {
            var listType = (ProjectListType)type;

            var newProjects = this.copyingManager.Paste();

            bool isCircular = false;

            switch (listType)
            {
                case ProjectListType.Referenced:

                    if (!this.SelectedProject.DependentProjects.Any(p => !p.IsChangedProject() && newProjects.Any(np => np.GUID == p.GUID)))
                    {
                        this.AddToReferenced(newProjects);
                    }
                    else
                    {
                        isCircular = true;
                    }

                    break;

                case ProjectListType.Dependent:

                    if (!this.SelectedProject.ReferencedProjects.Any(p => !p.IsChangedProject() && newProjects.Any(np => np.GUID == p.GUID)))
                    {
                        this.AddToDependent(newProjects);
                    }
                    else
                    {
                        isCircular = true;
                    }

                    break;
            }

            if (isCircular)
            {
                MessageBox.Show("Circular reference detected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                this.copyingManager.Copy(newProjects);
            }
        }

        private void RefreshChangesInformation()
        {
            this.PropertyChanged.Raise(() => this.ChangesCount);
            this.PropertyChanged.Raise(() => this.IsChanges);
        }

        private void RemoveFromDependent(IEnumerable<IProject> projectsToRemove)
        {
            var newProjects = this.SelectedProject.DependentProjects.Except(projectsToRemove).ToArray();
            var projectsToAdd = projectsToRemove.Where(p => p is Project || p is RemovedProject)
                                                .Select(p => new RemovedProject(p));

            this.SelectedProject.DependentProjects = newProjects.Concat(projectsToAdd).ToArray();
        }

        private void RemoveFromReferenced(IEnumerable<IProject> projectsToRemove)
        {
            var newProjects = this.SelectedProject.ReferencedProjects.Except(projectsToRemove).ToArray();
            var projectsToAdd = projectsToRemove.Where(p => p is Project || p is RemovedProject)
                                                .Select(p => new RemovedProject(p));

            this.SelectedProject.ReferencedProjects = newProjects.Concat(projectsToAdd).ToArray();
        }

        private void RemoveProjects(object projectsListBox)
        {
            var projectsToRemove = (projectsListBox as ListBox).SelectedItems.OfType<IProject>();
            var listType = this.GetProjectListType(projectsListBox);

            switch (listType)
            {
                case ProjectListType.Referenced:
                    this.RemoveFromReferenced(projectsToRemove);
                    break;

                case ProjectListType.Dependent:
                    this.RemoveFromDependent(projectsToRemove);
                    break;
            }
        }
    }
}