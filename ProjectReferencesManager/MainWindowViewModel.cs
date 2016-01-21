using ProjectReferencesManager.Model;
using ProjectReferencesManager.Tools;
using ProjectReferencesManager.Tools.Core;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace ProjectReferencesManager
{
    public interface IMainWindowViewModel
    {
        Solution SelectedSolution { get; set; }

        Project SelectedProject { get; set; }

        bool IsChanges { get; }

        int ChangesCount { get; }

        void RunWithRefresh(Action action);
    }

    public class MainWindowViewModel : IMainWindowViewModel, INotifyPropertyChanged
    {
        private readonly ICopyingManager copyingManager;
        private readonly IProjectsChangesManager changesManager;
        private readonly ISolutionLoader loader;
        private readonly ISolutionFileOpener fileOpener;
        private Project selectedProject;
        private Solution selectedSolution;

        public MainWindowViewModel(
            ISolutionFileOpener fileOpener,
            ISolutionLoader loader,
            ICopyingManager copyingManager,
            IProjectsChangesManager changesManager)
        {
            this.fileOpener = fileOpener;
            this.loader = loader;
            this.copyingManager = copyingManager;
            this.changesManager = changesManager;

            this.Commands = new MainWindowCommands();

            this.Commands.OpenSolutionCommand = new RelayCommand(this.OpenSolution);
            this.Commands.CopyProjectsCommand = new RelayCommandWithParameter(this.CopyProjects, this.CanCopyProjects);
            this.Commands.PasteProjectsCommand = new RelayCommandWithParameter(p => this.RunWithRefresh(() => this.PasteProjects(p)), this.CanPasteProjects);
            this.Commands.RemoveProjectsCommand = new RelayCommandWithParameter(p => this.RunWithRefresh(() => this.RemoveProjects(p)), this.CanRemoveProjects);
            this.Commands.RestoreProjectsCommand = new RelayCommandWithParameter(p => this.RunWithRefresh(() => this.RestoreProjects(p)), this.CanRestoreProjects);
            this.Commands.RestoreProjectChangesCommand = new RelayCommand(() => this.RunWithRefresh(() => this.RestoreProjectChanges()), this.CanRestoreProjectChanges);
            this.Commands.ApplyProjectChangesCommand = new RelayCommand(() => this.RunWithRefresh(() => this.ApplyProjectChanges()), this.CanApplyProjectChanges);
        }

        private void RestoreProjectChanges()
        {
            this.changesManager.RestoreProjectChanges(this.SelectedSolution);
        }

        private bool CanRestoreProjectChanges()
        {
            return this.SelectedSolution != null && this.SelectedSolution.GetChangedProjects().Any();
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

                return this.SelectedSolution.GetChangedProjects().Count();
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

        public void RunWithRefresh(Action action)
        {
            action();
            this.RefreshChangesInformation();
        }

        private void PasteProjects(object type)
        {
            this.changesManager.PasteProjects(this.SelectedProject, (ProjectListType)type);
        }

        private void RestoreProjects(object projectsListBox)
        {
            var listBox = projectsListBox as ListBox;
            var listBoxType = (ProjectListType)listBox.Tag;
            var removedProjects = listBox.SelectedItems.OfType<RemovedProject>();

            this.changesManager.RestoreProjects(listBoxType, this.SelectedProject, removedProjects);
        }

        private bool CanRestoreProjects(object projectsListBox)
        {
            if (projectsListBox == null)
            {
                return false;
            }

            return (projectsListBox as ListBox).SelectedItems.OfType<RemovedProject>().Any();
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

        private ProjectListType GetProjectListType(object projectsListBox)
        {
            return (ProjectListType)(projectsListBox as ListBox).Tag;
        }

        private void OpenSolution()
        {
            var fileName = this.fileOpener.Open();

            if (!string.IsNullOrEmpty(fileName))
            {
                this.SelectedSolution = this.loader.Load(fileName);
            }
        }

        private void RefreshChangesInformation()
        {
            this.PropertyChanged.Raise(() => this.ChangesCount);
            this.PropertyChanged.Raise(() => this.IsChanges);
        }

        private void RemoveProjects(object projectsListBox)
        {
            var projectsToRemove = (projectsListBox as ListBox).SelectedItems.OfType<IProject>();
            var listType = this.GetProjectListType(projectsListBox);

            this.changesManager.RemoveProjects(listType, this.SelectedProject, projectsToRemove);
        }
    }
}