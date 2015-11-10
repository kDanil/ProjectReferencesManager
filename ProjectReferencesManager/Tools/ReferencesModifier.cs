using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectReferencesManager.Model;

namespace ProjectReferencesManager.Tools
{
    public class ReferencesModifier
    {
        private readonly ProjectFileReader reader;

        public ReferencesModifier(ProjectFileReader reader)
        {
            this.reader = reader;
        }

        internal void AddReference(Project selectedProject, string selectedProjectPath, IEnumerable<Project> projects)
        {
            // var itemGroupEleme
        }
    }
}
