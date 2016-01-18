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
            return projects.OrderBy(p => p.Name).ToArray();
        }
    }
}