﻿using System.Collections.Generic;
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
        private IList<IProject> referencedProjects;
        private IList<IProject> dependentProjects;

        public event PropertyChangedEventHandler PropertyChanged;

        public string GUID { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public IList<IProject> ReferencedProjects
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

        public IList<IProject> DependentProjects
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