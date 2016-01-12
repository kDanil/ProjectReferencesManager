using ProjectReferencesManager.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ProjectReferencesManager.Tools
{
    public interface IReferencesModifier
    {
        void AddReference(string projectPath, IProject targetProject, IEnumerable<IProject> newProjects);

        void RemoveReference(string projectPath, IEnumerable<RemovedProject> removedProjects);
    }

    public class ReferencesModifier : IReferencesModifier
    {
        private readonly IProjectFileReader reader;

        public ReferencesModifier(IProjectFileReader reader)
        {
            this.reader = reader;
        }

        public void AddReference(string projectPath, IProject targetProject, IEnumerable<IProject> newProjects)
        {
            var root = this.reader.ReadDocument(projectPath);
            var elementGroup = this.reader.ReadReferencesGroup(root);
            var depth = this.GetPathDepth(targetProject.Path);

            var solutionRelativePath = string.Join("", Enumerable.Range(0, depth).Select(i => ".." + Path.DirectorySeparatorChar));

            foreach (var project in newProjects)
            {
                var projectReference = new XElement("ProjectReference", new XAttribute("Include", solutionRelativePath + project.Path));
                var projectItem = new XElement("Project");
                projectItem.SetValue("{" + project.GUID + "}");
                var name = new XElement("Name");
                name.SetValue(project.Name);

                projectReference.Add(projectItem);
                projectReference.Add(name);

                elementGroup.Add(projectReference);
            }

            root.Save(projectPath);
        }

        public void RemoveReference(string projectPath, IEnumerable<RemovedProject> removedProjects)
        {
            var root = this.reader.ReadDocument(projectPath);
            var elementGroup = this.reader.ReadReferencesGroup(root);

            elementGroup.Elements()
                        .Where(e => this.IsProjectToRemove(removedProjects, e))
                        .Remove();

            root.Save(projectPath);
        }

        private bool IsProjectToRemove(IEnumerable<RemovedProject> removedProjects, XElement e)
        {
            return e.Name.LocalName == "ProjectReference" && e.Elements().Any(n => n.Name.LocalName == "Project" && removedProjects.Any(p => p.GUID == GUIDFormatter.Format(n.Value)));
        }

        private int GetPathDepth(string path)
        {
            return path.Split(Path.DirectorySeparatorChar).Length - 1;
        }
    }
}