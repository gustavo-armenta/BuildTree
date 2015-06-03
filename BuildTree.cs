using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace BuildOptimization
{
    public class BuildTree
    {
        public string RootPath { get; set; }
        public Dictionary<string, string> Mapping { get; set; }

        private Dictionary<string, int> referencedProjects;
        private Dictionary<string, string> lowercaseProjectMap;
        private Output output;

        public BuildTree()
        {
            this.Mapping = new Dictionary<string, string>();
        }
        public Output Analyze()
        {
            output = new Output();

            referencedProjects = new Dictionary<string, int>();
            lowercaseProjectMap = new Dictionary<string, string>();
            var files = Directory.GetFiles(this.RootPath, "*.csproj", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                output.Projects.Add(file);
                referencedProjects.Add(file.ToLowerInvariant(), 0);
                lowercaseProjectMap.Add(file.ToLowerInvariant(), file);
            }

            TraverseDirsProj();

            foreach (var project in referencedProjects)
            {
                if (project.Value == 0)
                {
                    var dir = Path.GetDirectoryName(project.Key);
                    bool shouldKeep = false;
                    foreach(var project2 in referencedProjects)
                    {
                        var dir2 = Path.GetDirectoryName(project2.Key);
                        if(project2.Value > 0 && string.Equals(dir, dir2))
                        {
                            shouldKeep = true;
                            continue;
                        }
                    }

                    if (!shouldKeep)
                    {
                        output.ProjectsNotReferenced.Add(lowercaseProjectMap[project.Key]);
                    }
                }
            }

            return output;
        }

        private void TraverseDirsProj()
        {
            var dirsItems = Directory.GetFiles(this.RootPath, "dirs.proj", SearchOption.TopDirectoryOnly);
            var queue = new Queue<string>();
            queue.Enqueue(Path.Combine(this.RootPath, "dirs.proj").ToLowerInvariant());

            while (queue.Count > 0)
            {
                var dirs = queue.Dequeue();
                
                XmlDocument doc = new XmlDocument();
                doc.Load(dirs);
                XmlNode root = doc.DocumentElement;
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("a", "http://schemas.microsoft.com/developer/msbuild/2003");
                XmlNodeList nodeList = root.SelectNodes("//a:ProjectFile", nsmgr);

                foreach (var item in nodeList)
                {
                    var projectFile = ((XmlNode)item).Attributes["Include"].Value;
                    projectFile = projectFile.Replace("*", "");
                    projectFile = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(dirs), projectFile));
                    projectFile = projectFile.ToLowerInvariant();

                    if (string.Equals("dirs.proj", Path.GetFileName(projectFile)))
                    {
                        queue.Enqueue(projectFile);
                    }

                    if (!projectFile.EndsWith(".csproj"))
                    {
                        continue;
                    }

                    if (referencedProjects.ContainsKey(projectFile))
                    {
                        referencedProjects[projectFile] += 1;
                        TraverseProjectReferences(projectFile);
                    }
                    else
                    {
                        output.DirsProjectFileNotFound.Add(string.Join(",", dirs, projectFile));
                    }
                }
            }
        }

        private void TraverseProjectReferences(string projectFile)
        {
            if (!projectFile.EndsWith(".csproj"))
            {
                return;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(projectFile);
            XmlNode root = doc.DocumentElement;
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("a", "http://schemas.microsoft.com/developer/msbuild/2003");
            XmlNodeList nodeList = root.SelectNodes("//a:ProjectReference", nsmgr);

            foreach (var item in nodeList)
            {
                var projectReference = ((XmlNode)item).Attributes["Include"].Value;
                projectReference = projectReference.ToLowerInvariant();

                if (!projectReference.EndsWith(".csproj"))
                {
                    continue;
                }

                if (projectReference.StartsWith("$"))
                {
                    var mapKey = projectReference.Substring(0, projectReference.IndexOf(')') + 1);
                    if (!this.Mapping.ContainsKey(mapKey))
                    {
                        throw new Exception(string.Format("missing entry on BuildTree.Mapping: {0}", projectReference));
                    }

                    var mapValue = this.Mapping[mapKey.ToLowerInvariant()];
                    projectReference = projectReference.Replace(mapKey, mapValue);
                }
                else
                {
                    projectReference = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(projectFile), projectReference));
                }

                projectReference = projectReference.ToLowerInvariant();
                if (File.Exists(projectReference))
                {
                    if (referencedProjects.ContainsKey(projectReference))
                    {
                        referencedProjects[projectReference] += 1;
                        if (referencedProjects[projectReference] == 1)
                        {
                            TraverseProjectReferences(projectReference);
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("not found in referenceProjects: {0}", projectReference));
                    }
                }
                else
                {
                    output.ProjectReferencesNotFound.Add(string.Join(",", projectFile, projectReference));
                }
            }
        }
    }

    public class Output
    {
        public List<string> Projects { get; set; }
        public List<string> ProjectsNotReferenced { get; set; }
        public List<string> DirsProjectFileNotFound { get; set; }
        public List<string> ProjectReferencesNotFound { get; set; }

        public Output()
        {
            this.DirsProjectFileNotFound = new List<string>();
            this.ProjectReferencesNotFound = new List<string>();
            this.Projects = new List<string>();
            this.ProjectsNotReferenced = new List<string>();
        }
    }
}
