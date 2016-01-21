using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

namespace ProjectReferencesManager.Model
{
    public interface IProject
    {
        string GUID { get; }

        string Name { get; }

        string Path { get; }
    }

    public class Project : IProject, INotifyPropertyChanged
    {
        private IEnumerable<IProject> referencedProjects;
        private IEnumerable<IProject> dependentProjects;

        public event PropertyChangedEventHandler PropertyChanged;

        public string GUID { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public IEnumerable<IProject> ReferencedProjects
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
                    this.PropertyChanged.Raise(() => this.HasChangedProjects);
                }
            }
        }

        public bool HasChangedProjects
        {
            get
            {
                return this.ReferencedProjects.Any(p => p.IsChangedProject()) ||
                       this.DependentProjects.Any(p => p.IsChangedProject());
            }
        }

        public IEnumerable<IProject> DependentProjects
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
                    this.PropertyChanged.Raise(() => this.HasChangedProjects);
                }
            }
        }
    }
}