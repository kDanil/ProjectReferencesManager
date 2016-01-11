using Microsoft.Win32;
using ProjectReferencesManager.Model;
using ProjectReferencesManager.Tools;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectReferencesManager
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly CopyingManager copyingManager;
        private readonly ReferencesModifier modifier;
        private readonly ProjectFileReader projectReader;
        private Project selectedProject;
        private Solution selectedSolution;

        public MainWindowViewModel(ProjectFileReader projectReader, CopyingManager copingManager, ReferencesModifier modifier)
        {
            this.projectReader = projectReader;
            this.copyingManager = copingManager;
            this.modifier = modifier;

            this.Commands = new MainWindowCommands();

            this.Commands.OpenSolutionCommand = new RelayCommand(this.OpenSolution);
            this.Commands.CopyProjectsCommand = new RelayCommandWithParameter(this.CopyProjects, this.CanCopyProjects);
            this.Commands.PasteProjectsCommand = new RelayCommandWithParameter(p => { this.PasteProjects(p); this.RefreshChangesInformation(); }, this.CanPasteProjects);
            this.Commands.RemoveProjectsCommand = new RelayCommandWithParameter(p => { this.RemoveProjects(p); this.RefreshChangesInformation(); }, this.CanRemoveProjects);
            this.Commands.RestoreProjectsCommand = new RelayCommandWithParameter(p => { this.RestoreProjects(p); this.RefreshChangesInformation(); }, this.CanRestoreProjects);
            this.Commands.ApplyProjectChangesCommand = new RelayCommand(() => { this.ApplyProjectChanges(); this.RefreshChangesInformation(); }, this.CanApplyProjectChanges);
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
                    this.PropertyChanged.Raise(() => this.SelectedSolution);
                }
            }
        }

        private void RestoreProjects(object projectsListBox)
        {
            var listBox = projectsListBox as ListBox;
            var listBoxType = (ProjectListType)listBox.Tag;

            var removedProjects = listBox.SelectedItems.OfType<RemovedProject>();
            var removedProjectGUIDs = removedProjects.Select(p => p.GUID).ToArray();
            var projectsToAdd = this.SelectedSolution.Projects.Where(p => removedProjectGUIDs.Contains(p.GUID));

            switch (listBoxType)
            {
                case ProjectListType.Referenced:
                    this.SelectedProject.ReferencedProjects = this.SelectedProject.ReferencedProjects.Except(removedProjects)
                                                                                                     .Concat(projectsToAdd)
                                                                                                     .ToArray();
                    break;

                case ProjectListType.Dependent:
                    this.SelectedProject.DependentProjects = this.SelectedProject.DependentProjects.Except(removedProjects)
                                                                                                   .Concat(projectsToAdd)
                                                                                                   .ToArray();
                    break;
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
            foreach (var project in this.SelectedSolution.Projects.Where(p => p.DependentProjects.Any(pr => this.IsChangedProject(pr)) || p.ReferencedProjects.Any(pr => this.IsChangedProject(pr))))
            {
                this.ApplyDependentProjectChanges(project);

                this.ApplyReferencedProjectChanges(project);
            }
        }

        private void ApplyReferencedProjectChanges(Project project)
        {
            var addedProjects = this.GetFilteredProjects<AddedProject>(project.ReferencedProjects);
            var removedProjects = this.GetFilteredProjects<RemovedProject>(project.ReferencedProjects);

            this.modifier.AddReference(this.GetAbsoluteProjectPath(project), this.GetPathDepth(project.Path), addedProjects);
            this.modifier.RemoveReference(this.GetAbsoluteProjectPath(project), removedProjects);

            project.ReferencedProjects = project.ReferencedProjects.Except(removedProjects)
                                                                   .Except(addedProjects)
                                                                   .Concat(this.FindOriginalProjects(addedProjects))
                                                                   .ToArray();
        }

        private Project FindOriginalProject(IProject project)
        {
            return this.FindOriginalProjects(new[] { project }).Single();
        }

        private IEnumerable<Project> FindOriginalProjects(IEnumerable<IProject> projects)
        {
            return this.SelectedSolution.Projects.Where(p => projects.Any(ap => ap.GUID == p.GUID));
        }

        private void ApplyDependentProjectChanges(Project project)
        {
            var addedProjects = this.GetFilteredProjects<AddedProject>(project.DependentProjects);
            var removedProjects = this.GetFilteredProjects<RemovedProject>(project.DependentProjects);

            foreach (var addedProject in addedProjects)
            {
                this.modifier.AddReference(this.GetAbsoluteProjectPath(addedProject), this.GetPathDepth(addedProject.Path), new[] { project });
            }

            project.DependentProjects = project.DependentProjects.Except(removedProjects)
                                                                 .Except(addedProjects)
                                                                 .Concat(this.FindOriginalProjects(addedProjects))
                                                                 .ToArray();
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

        private void FindDependentProjects()
        {
            foreach (var project in this.SelectedSolution.Projects)
            {
                project.DependentProjects = this.SelectedSolution.Projects.Except(new[] { project })
                                              .Where(p => p.ReferencedProjects.Contains(project))
                                              .ToArray();
            }
        }

        private string GetAbsoluteProjectPath(IProject project)
        {
            return this.SelectedSolution.FolderPath + Path.DirectorySeparatorChar + project.Path;
        }

        private int GetPathDepth(string path)
        {
            return path.Split(Path.DirectorySeparatorChar).Length - 1;
        }

        private IEnumerable<IProject> GetChangedProjects()
        {
            var dependentProjects = this.SelectedSolution.Projects.SelectMany(p => p.DependentProjects.Where(pr => this.IsChangedProject(pr)));
            var referencedProjects = this.SelectedSolution.Projects.SelectMany(p => p.ReferencedProjects.Where(pr => this.IsChangedProject(pr)));

            return dependentProjects.Concat(referencedProjects);
        }

        private IEnumerable<T> GetFilteredProjects<T>(IEnumerable<IProject> projects) where T : IProject
        {
            return projects.Where(pr => this.IsChangedProject(pr)).OfType<T>();
        }

        private bool IsChangedProject(IProject project)
        {
            return project is AddedProject || project is RemovedProject;
        }

        private ProjectListType GetProjectListType(object projectsListBox)
        {
            return (ProjectListType)(projectsListBox as ListBox).Tag;
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

        private void LoadReferencedProjects(Project project)
        {
            var projectInfos = this.projectReader.Read(this.GetAbsoluteProjectPath(project));

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

        private void PasteProjects(object type)
        {
            var listType = (ProjectListType)type;

            var newProjects = this.copyingManager.Paste();

            bool isCircular = false;

            switch (listType)
            {
                case ProjectListType.Referenced:

                    if (!this.SelectedProject.DependentProjects.Any(p => !this.IsChangedProject(p) && newProjects.Any(np => np.GUID == p.GUID)))
                    {
                        this.AddToReferenced(newProjects);
                    }
                    else
                    {
                        isCircular = true;
                    }

                    break;

                case ProjectListType.Dependent:

                    if (!this.SelectedProject.ReferencedProjects.Any(p => !this.IsChangedProject(p) && newProjects.Any(np => np.GUID == p.GUID)))
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