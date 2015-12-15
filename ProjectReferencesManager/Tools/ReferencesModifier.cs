using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectReferencesManager.Model;
using System.Xml.Linq;
using System.IO;

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

            var solutionRelativePath = string.Join(Path.DirectorySeparatorChar.ToString(), Enumerable.Range(0, depth).Select(i => Path.DirectorySeparatorChar + ".."));

            foreach (var project in newProjects)
            {
                var projectReference = new XElement("ProjectReference", new XAttribute("Include", solutionRelativePath + Path.DirectorySeparatorChar + project.Path));
                var projectItem = new XElement("Project");
                projectItem.SetValue(project.GUID);
                var name = new XElement("Name");
                name.SetValue(project.Name);

                projectReference.Add(projectItem);
                projectReference.Add(name);

                elementGroup.Add(projectReference);
            }

            root.Save(projectPath);
        }
    }
}
