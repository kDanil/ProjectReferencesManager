using ProjectReferencesManager.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProjectReferencesManager
{
    public static class ProjectsExtensions
    {
        public static Project FindOriginalProject(this IProject project, IEnumerable<Project> solutionProjects)
        {
            return new[] { project }.FindOriginalProjects(solutionProjects).Single();
        }

        public static IEnumerable<Project> FindOriginalProjects(this IEnumerable<IProject> projects, IEnumerable<Project> solutionProjects)
        {
            return solutionProjects.Where(p => projects.Any(ap => ap.GUID == p.GUID));
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
    }
}