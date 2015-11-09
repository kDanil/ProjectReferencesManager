using System.Collections.Generic;
using System.ComponentModel;

namespace ProjectReferencesManager.Model
{
    public class Project : INotifyPropertyChanged
    {
        private IEnumerable<Project> referencedProjects;
        private IEnumerable<Project> dependentProjects;

        public event PropertyChangedEventHandler PropertyChanged;

        public string GUID { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public IEnumerable<Project> ReferencedProjects
        {
            get
            {
                return this.referencedProjects;
            }

            set
            {
                if (this.referencedProjects != value)
                {
                    this.referencedProjects = value;
                    this.PropertyChanged.Raise(() => this.ReferencedProjects);
                }
            }
        }

        public IEnumerable<Project> DependentProjects
        {
            get
            {
                return this.dependentProjects;
            }

            set
            {
                if (this.dependentProjects != value)
                {
                    this.dependentProjects = value;
                    this.PropertyChanged.Raise(() => this.DependentProjects);
                }
            }
        }
    }
}