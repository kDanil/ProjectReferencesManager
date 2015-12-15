using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ProjectReferencesManager.Tools
{
    public class ProjectFileReader
    {
        public XElement ReadDocument(string filePath)
        {
            return XElement.Load(filePath, LoadOptions.PreserveWhitespace);
        }

        public XElement ReadReferencesGroup(XElement root)
        {
            return root.Elements()
                       .Single(e => e.Name.LocalName == "ItemGroup" && e.Elements().Any(ee => ee.Name.LocalName == "ProjectReference"));
        }

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