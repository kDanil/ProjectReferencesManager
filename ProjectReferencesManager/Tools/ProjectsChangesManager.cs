﻿using ProjectReferencesManager.Model;
using System.Collections.Generic;
using System.Linq;

namespace ProjectReferencesManager.Tools
{
    public class ProjectsChangesManager
    {
        private readonly ReferencesModifier modifier;
        private Solution solution;

        public ProjectsChangesManager(ReferencesModifier modifier)
        {
            this.modifier = modifier;
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
    }
}