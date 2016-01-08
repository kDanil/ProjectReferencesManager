using ProjectReferencesManager.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System;

namespace ProjectReferencesManager.Tools
{
    public class ReferencesModifier
    {
        private readonly ProjectFileReader reader;

        public ReferencesModifier(ProjectFileReader reader)
        {
            this.reader = reader;
        }

        internal void AddReference(string projectPath, int depth, IEnumerable<AddedProject> newProjects)
        {
            var root = this.reader.ReadDocument(projectPath);
            var elementGroup = this.reader.ReadReferencesGroup(root);

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

        internal void RemoveReference(string projectPath, IEnumerable<RemovedProject> removedProjects)
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
    }
}