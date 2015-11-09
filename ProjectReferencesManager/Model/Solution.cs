using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace ProjectReferencesManager.Model
{
    public class Solution : INotifyPropertyChanged
    {
        private string fullPath;

        public string FullPath
        {
            get
            {
                return this.fullPath;
            }

            set
            {
                if (this.fullPath != value)
                {
                    this.fullPath = value;
                    this.PropertyChanged.Raise(() => this.FullPath);
                }
            }
        }

        public string FolderPath
        {
            get
            {
                return Path.GetDirectoryName(this.fullPath);
            }
        }

        private IEnumerable<Project> projects;

        public IEnumerable<Project> Projects
        {
            get
            {
                return this.projects;
            }

            set
            {
                if (this.projects != value)
                {
                    this.projects = value;
                    this.PropertyChanged.Raise(() => this.Projects);
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }
}