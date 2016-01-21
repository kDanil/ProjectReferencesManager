using ProjectReferencesManager.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProjectReferencesManager
{
    public static class ProjectsExtensions
    {
        public static IEnumerable<Project> FindOriginalProjects(this IEnumerable<IProject> projects, IEnumerable<Project> solutionProjects)
        {
            return solutionProjects.Where(p => projects.Any(ap => ap.AreEqual(p)));
        }

        public static bool IsChangedProject(this IProject project)
        {
            return project is AddedProject || project is RemovedProject;
        }

        public static string GetAbsolutePath(this IProject project, Solution solution)
        {
            return solution.FolderPath + Path.DirectorySeparatorChar + project.Path;
        }

        public static IEnumerable<T> GetFilteredProjects<T>(this IEnumerable<IProject> projects) where T : IProject
        {
            return projects.Where(pr => pr.IsChangedProject()).OfType<T>();
        }

        public static bool AreEqual(this IProject project, IProject otherProject)
        {
            return project.GUID == otherProject.GUID;
        }
    }

    public static class SolutionExtensions
    {
        public static IEnumerable<IProject> GetChangedProjects(this Solution solution)
        {
            var dependentProjects = solution.Projects.SelectMany(p => p.DependentProjects.Where(pr => pr.IsChangedProject()));
            var referencedProjects = solution.Projects.SelectMany(p => p.ReferencedProjects.Where(pr => pr.IsChangedProject()));

            return dependentProjects.Concat(referencedProjects);
        }
    }
}