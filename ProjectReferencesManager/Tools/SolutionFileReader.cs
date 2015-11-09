using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProjectReferencesManager.Tools
{
    public class SolutionFileReader
    {
        public IEnumerable<ProjectFileInfo> Read(string filePath)
        {
            var content = File.ReadAllLines(filePath);

            foreach (var line in content)
            {
                var matches = Regex.Matches(line, "Project\\(\\\"\\{(.*)\\}\\\"\\) \\= \\\"(.*)\\\"\\, \\\"(.*)\\\"\\, \\\"\\{(.*)\\}\\\"");
                if (matches.Count > 0)
                {
                    var values = matches.Cast<Match>().SelectMany(o => o.Groups.Cast<Capture>().Skip(1).Select(c => c.Value)).ToArray();

                    var name = values[1];
                    var path = values[2];
                    var guid = values[3];

                    if (path == name)
                    {
                        continue;
                    }

                    yield return new ProjectFileInfo()
                    {
                        Name = name,
                        Path = path,
                        GUID = GUIDFormatter.Format(guid)
                    };
                }
            }
        }
    }

    public class ProjectFileInfo
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public string GUID { get; set; }
    }
}