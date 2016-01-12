using ProjectReferencesManager.Model;
using System.Collections.Generic;
using System.Linq;

namespace ProjectReferencesManager.Tools
{
    public interface ICopyingManager
    {
        void Copy(IEnumerable<IProject> projects);

        IEnumerable<IProject> Paste();

        bool HasData();

        int Count();
    }

    public class CopyingManager : ICopyingManager
    {
        private List<IProject> projects = new List<IProject>();

        public void Copy(IEnumerable<IProject> projects)
        {
            this.projects.Clear();

            this.projects.AddRange(projects);
        }

        public IEnumerable<IProject> Paste()
        {
            var projects = this.projects.ToArray();

            this.projects.Clear();

            return projects;
        }

        public bool HasData()
        {
            return this.projects.Any();
        }

        public int Count()
        {
            return this.projects.Count;
        }
    }
}