using System.Collections.Generic;
using System.ComponentModel;

namespace ProjectReferencesManager.Model
{
    public class Project : INotifyPropertyChanged
    {
        public string GUID { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        private IEnumerable<Project> referencedProjects;

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


        public event PropertyChangedEventHandler PropertyChanged;
    }
}