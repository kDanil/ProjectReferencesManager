using ProjectReferencesManager.Model;
using System.Collections.Generic;
using System.Linq;

namespace ProjectReferencesManager.Tools
{
    public interface IProjectCollectionsModifier
    {
        IEnumerable<IProject> Prepare(IEnumerable<IProject> projects);
    }

    public class ProjectCollectionsModifier : IProjectCollectionsModifier
    {
        public IEnumerable<IProject> Prepare(IEnumerable<IProject> projects)
        {
            return projects.OrderBy(this.GetProjectsOrder)
                           .ThenBy(p => p.Name)
                           .ToArray();
        }

        private object GetProjectsOrder(IProject project)
        {
            if (project is AddedProject)
            {
                return 1;
            }
            else
            if (project is RemovedProject)
            {
                return 2;
            }

            return 0;
        }
    }
}