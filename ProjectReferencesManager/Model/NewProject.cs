﻿namespace ProjectReferencesManager.Model
{
    public class NewProject : IProject
    {
        public NewProject(IProject project)
        {
            this.GUID = project.GUID;
            this.Name = project.Name;
            this.Path = project.Path;
        }

        public string GUID { get; private set; }

        public string Name { get; private set; }

        public string Path { get; private set; }
    }
}