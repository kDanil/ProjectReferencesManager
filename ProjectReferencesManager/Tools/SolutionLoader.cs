using ProjectReferencesManager.Model;
using System.Collections.Generic;
using System.Linq;

namespace ProjectReferencesManager.Tools
{
    public interface ISolutionLoader
    {
        Solution Load(string fileName);
    }

    public class SolutionLoader : ISolutionLoader
    {
        private readonly IProjectFileReader reader;
        private readonly IProjectCollectionsModifier collectionModifier;
        private Solution solution;

        public SolutionLoader(IProjectFileReader reader, IProjectCollectionsModifier collectionModifier)
        {
            this.reader = reader;
            this.collectionModifier = collectionModifier;
        }

        public Solution Load(string fileName)
        {
            this.solution = new Solution()
            {
                FullPath = fileName,
                Projects = this.LoadProjects(fileName)
            };

            this.LoadProjectReferences();
            this.FindDependentProjects();

            return this.solution;
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

        private void FindDependentProjects()
        {
            foreach (var project in this.solution.Projects)
            {
                project.DependentProjects = this.collectionModifier.Prepare(this.solution.Projects.Except(new[] { project })
                                                                                         .Where(p => p.ReferencedProjects.Contains(project)));
            }
        }

        private void LoadProjectReferences()
        {
            foreach (var project in this.solution.Projects)
            {
                this.LoadReferencedProjects(project);
            }
        }

        private void LoadReferencedProjects(Project project)
        {
            var projectInfos = this.reader.Read(project.GetAbsolutePath(this.solution));

            var guids = projectInfos.Select(p => p.GUID).ToArray();

            project.ReferencedProjects = this.collectionModifier.Prepare(this.solution.Projects.Where(p => guids.Contains(p.GUID)));
        }
    }
}