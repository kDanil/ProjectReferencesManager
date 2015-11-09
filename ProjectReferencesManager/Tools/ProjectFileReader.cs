using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ProjectReferencesManager.Tools
{
    public class ProjectFileReader
    {
        public IEnumerable<ProjectFileInfo> Read(string filePath)
        {
            return XElement.Load(filePath)
                           .Elements()
                           .Where(e => e.Name.LocalName == "ItemGroup")
                           .Elements()
                           .Where(e => e.Name.LocalName == "ProjectReference")
                           .Elements()
                           .Where(e => e.Name.LocalName == "Project")
                           .Select(e => new ProjectFileInfo()
                           {
                               GUID = GUIDFormatter.Format(e.Value)
                           })
                           .ToArray();
        }
    }
}