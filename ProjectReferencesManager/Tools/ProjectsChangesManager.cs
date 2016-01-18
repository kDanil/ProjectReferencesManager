using ProjectReferencesManager.Model;
using System.Collections.Generic;
using System.Linq;

namespace ProjectReferencesManager.Tools
{
    public interface IProjectsChangesManager
    {
        void AssignSolution(Solution solution);

        void ApplyProjectChanges();

        void RestoreProjects(Project targetProject, IEnumerable<RemovedProject> removedProjects, ProjectListType projectType);

        void PasteProjects(Project targetProject, ProjectListType type);

        void RemoveProjects(ProjectListType listType, Project targetProject, IEnumerable<IProject> projectsToRemove);
    }

    public class ProjectsChangesManager : IProjectsChangesManager
    {
        private readonly IReferencesModifier modifier;
        private readonly ICopyingManager copyingManager;
        private readonly IUserInteraction interaction;
        private Solution solution;

        public ProjectsChangesManager(
            IReferencesModifier modifier,
            ICopyingManager copyingManager,
            IUserInteraction interaction)
        {
            this.modifier = modifier;
            this.copyingManager = copyingManager;
            this.interaction = interaction;
        }

        public void AssignSolution(Solution solution)
        {
            this.solution = solution;
        }

        public void ApplyProjectChanges()
        {
            foreach (var project in this.solution.Projects.Where(p => p.DependentProjects.Any(pr => pr.IsChangedProject()) ||
                                                                      p.ReferencedProjects.Any(pr => pr.IsChangedProject())))
            {
                this.ApplyDependentProjectChanges(project);

                this.ApplyReferencedProjectChanges(project);
            }
        }

        public void RestoreProjects(Project targetProject, IEnumerable<RemovedProject> removedProjects, ProjectListType projectType)
        {
            var removedProjectGUIDs = removedProjects.Select(p => p.GUID).ToArray();
            var projectsToAdd = this.solution.Projects.Where(p => removedProjectGUIDs.Contains(p.GUID));

            switch (projectType)
            {
                case ProjectListType.Referenced:
                    targetProject.ReferencedProjects = targetProject.ReferencedProjects.Except(removedProjects)
                                                                                       .Concat(projectsToAdd)
                                                                                       .ToArray();
                    break;

                case ProjectListType.Dependent:
                    targetProject.DependentProjects = targetProject.DependentProjects.Except(removedProjects)
                                                                                     .Concat(projectsToAdd)
                                                                                     .ToArray();
                    break;
            }
        }

        public void PasteProjects(Project targetProject, ProjectListType type)
        {
            var newProjects = this.copyingManager.Paste();

            bool isCircular = false;

            switch (type)
            {
                case ProjectListType.Referenced:

                    if (!targetProject.DependentProjects.Any(p => this.IsProjectExists(p, newProjects)))
                    {
                        this.AddToReferenced(targetProject, newProjects);
                    }
                    else
                    {
                        isCircular = true;
                    }

                    break;

                case ProjectListType.Dependent:

                    if (!targetProject.ReferencedProjects.Any(p => this.IsProjectExists(p, newProjects)))
                    {
                        this.AddToDependent(targetProject, newProjects);
                    }
                    else
                    {
                        isCircular = true;
                    }

                    break;
            }

            if (isCircular)
            {
                this.interaction.ShowError("Circular reference detected");

                this.copyingManager.Copy(newProjects);
            }
        }

        public void RemoveProjects(ProjectListType listType, Project targetProject, IEnumerable<IProject> projectsToRemove)
        {
            switch (listType)
            {
                case ProjectListType.Referenced:
                    this.RemoveFromReferenced(targetProject, projectsToRemove);
                    break;

                case ProjectListType.Dependent:
                    this.RemoveFromDependent(targetProject, projectsToRemove);
                    break;
            }
        }

        private bool IsProjectExists(IProject project, IEnumerable<IProject> newProjects)
        {
            return !project.IsChangedProject() && newProjects.Any(np => np.GUID == project.GUID);
        }

        private void AddToDependent(Project targetProject, IEnumerable<IProject> newProjects)
        {
            var projectsToAdd = newProjects.Except(targetProject.DependentProjects)
                                                      .Select(p => new AddedProject(p));

            var projectsToRemove = targetProject.DependentProjects.Where(p => this.IsProjectToRemove(p, projectsToAdd));

            targetProject.DependentProjects = targetProject.DependentProjects.Except(projectsToRemove)
                                                                             .Concat(projectsToAdd)
                                                                             .ToArray();
        }

        private void AddToReferenced(Project targetProject, IEnumerable<IProject> newProjects)
        {
            var projectsToAdd = newProjects.Except(targetProject.ReferencedProjects)
                                           .Select(p => new AddedProject(p));

            var projectsToRemove = targetProject.ReferencedProjects.Where(p => this.IsProjectToRemove(p, projectsToAdd));

            targetProject.ReferencedProjects = targetProject.ReferencedProjects.Except(projectsToRemove)
                                                                               .Concat(projectsToAdd)
                                                                               .ToArray();
        }

        private bool IsProjectToRemove(IProject project, IEnumerable<IProject> projectsToAdd)
        {
            return project is RemovedProject && projectsToAdd.Any(pp => pp.GUID == project.GUID);
        }

        private void ApplyReferencedProjectChanges(Project project)
        {
            var addedProjects = project.ReferencedProjects.GetFilteredProjects<AddedProject>();
            var removedProjects = project.ReferencedProjects.GetFilteredProjects<RemovedProject>();

            this.modifier.AddReference(project.GetAbsolutePath(this.solution), project, addedProjects);
            this.modifier.RemoveReference(project.GetAbsolutePath(this.solution), removedProjects);

            project.ReferencedProjects = project.ReferencedProjects.Except(removedProjects)
                                                                   .Except(addedProjects)
                                                                   .Concat(addedProjects.FindOriginalProjects(this.solution.Projects))
                                                                   .ToArray();
        }

        private void ApplyDependentProjectChanges(Project project)
        {
            var addedProjects = project.DependentProjects.GetFilteredProjects<AddedProject>();
            var removedProjects = project.DependentProjects.GetFilteredProjects<RemovedProject>();

            foreach (var addedProject in addedProjects)
            {
                this.modifier.AddReference(addedProject.GetAbsolutePath(this.solution), addedProject, new[] { project });
            }

            project.DependentProjects = project.DependentProjects.Except(removedProjects)
                                                                 .Except(addedProjects)
                                                                 .Concat(addedProjects.FindOriginalProjects(this.solution.Projects))
                                                                 .ToArray();
        }

        private void RemoveFromDependent(Project targetProject, IEnumerable<IProject> projectsToRemove)
        {
            var newProjects = targetProject.DependentProjects.Except(projectsToRemove).ToArray();
            var projectsToAdd = projectsToRemove.Where(p => p is Project || p is RemovedProject)
                                                .Select(p => new RemovedProject(p));

            targetProject.DependentProjects = newProjects.Concat(projectsToAdd).ToArray();
        }

        private void RemoveFromReferenced(Project targetProject, IEnumerable<IProject> projectsToRemove)
        {
            var newProjects = targetProject.ReferencedProjects.Except(projectsToRemove).ToArray();
            var projectsToAdd = projectsToRemove.Where(p => p is Project || p is RemovedProject)
                                                .Select(p => new RemovedProject(p));

            targetProject.ReferencedProjects = newProjects.Concat(projectsToAdd).ToArray();
        }
    }
}