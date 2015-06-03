using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BuildOptimization
{
    class Program
    {
        static void Main(string[] args)
        {
            var buildTree = new BuildTree();
            buildTree.RootPath = @"D:\one\AD\MSODS\Core\src\";
            buildTree.Mapping.Add("$(administrationpath)", Path.Combine(buildTree.RootPath, @"dev\om\administration"));
            buildTree.Mapping.Add("$(cacheservicepath)", Path.Combine(buildTree.RootPath, @"dev\cacheservice"));
            buildTree.Mapping.Add("$(coexistencepath)", Path.Combine(buildTree.RootPath, @"dev\coexistence"));
            buildTree.Mapping.Add("$(commonpath)", Path.Combine(buildTree.RootPath, @"dev\common"));
            buildTree.Mapping.Add("$(configurationpath)", Path.Combine(buildTree.RootPath, @"dev\configuration"));
            buildTree.Mapping.Add("$(deploymentpath)", Path.Combine(buildTree.RootPath, @"dev\mgmtsys\deployment"));
            buildTree.Mapping.Add("$(devpath)", Path.Combine(buildTree.RootPath, "dev"));
            buildTree.Mapping.Add("$(dms)", Path.Combine(buildTree.RootPath, @"dev\mgmtsys\dms"));
            buildTree.Mapping.Add("$(dspath)", Path.Combine(buildTree.RootPath, @"dev\ds"));
            buildTree.Mapping.Add("$(functionalitymanagementpath)", Path.Combine(buildTree.RootPath, @"dev\common\functionalitymanagement"));
            buildTree.Mapping.Add("$(identitypath)", Path.Combine(buildTree.RootPath, @"dev\identity"));
            buildTree.Mapping.Add("$(inetroot)", @"d:\one\ad\msods\core");
            buildTree.Mapping.Add("$(pushendpointspath)", Path.Combine(buildTree.RootPath, @"dev\pushendpoints"));
            buildTree.Mapping.Add("$(reportingpath)", Path.Combine(buildTree.RootPath, @"dev\mgmtsys\Reporting"));
            buildTree.Mapping.Add("$(restpath)", Path.Combine(buildTree.RootPath, @"dev\restservices"));
            buildTree.Mapping.Add("$(testpath)", Path.Combine(buildTree.RootPath, @"test"));
            buildTree.Mapping.Add("$(windowsfabricpath)", Path.Combine(buildTree.RootPath, @"dev\windowsfabric"));
            buildTree.Mapping.Add("$(workflowspath)", Path.Combine(buildTree.RootPath, @"dev\om\workflows"));

            var output = buildTree.Analyze();

            var excludeDirectories = new List<string>();
            excludeDirectories.Add(@"D:\one\AD\MSODS\Core\src\dev\AzureExpressDeployment\TopologyCreator\");
            excludeDirectories.Add(@"D:\one\AD\MSODS\Core\src\dev\coexistence\dirsyncclient");
            excludeDirectories.Add(@"D:\one\AD\MSODS\Core\src\dev\coexistence\DirSyncClientV2\");
            excludeDirectories.Add(@"D:\one\AD\MSODS\Core\src\dev\ds\InstantOn\");
            excludeDirectories.Add(@"D:\one\AD\MSODS\Core\src\dev\LiveSiteWorker\MsodsLiveSiteClient\");
            excludeDirectories.Add(@"D:\one\AD\MSODS\Core\src\dev\mgmtsys\synthetictransaction\monitoringstx");
            excludeDirectories.Add(@"D:\one\AD\MSODS\Core\src\test\ds\datapopulator\dscorecommon\");
            excludeDirectories.Add(@"D:\one\AD\MSODS\Core\src\dev\ds\lib\ServiceInstanceMoveClient\");
            excludeDirectories.Add(@"D:\one\AD\MSODS\Core\src\dev\ds\SelectiveSync\");
            excludeDirectories.Add(@"D:\one\AD\MSODS\Core\src\dev\ds\tasks\DSTasks.SampleHost\");
            excludeDirectories.Add(@"D:\one\AD\MSODS\Core\src\dev\ds\webservices\SyncService.SampleHost\");
            excludeDirectories.Add(@"D:\one\AD\MSODS\Core\src\dev\Notifications\Examples\SubscriptionStore.ExtensionExample\");
            excludeDirectories.Add(@"D:\one\AD\MSODS\Core\src\dev\Notifications\Store\SubscriptionStore.ExchangeExtension.ConsoleTests\");

            Console.WriteLine("Total Projects: {0}", output.Projects.Count);
            Console.WriteLine("Projects not referenced: {0}", output.ProjectsNotReferenced.Count);
            Console.WriteLine("dirs.proj file references a project not found: {0}", output.DirsProjectFileNotFound.Count);
            Console.WriteLine("*.csproj file references a project not found: {0}", output.ProjectReferencesNotFound.Count);

            Console.WriteLine();
            Console.WriteLine("Exclude directories:");
            foreach (var excludeDirectory in excludeDirectories)
            {
                Console.WriteLine(excludeDirectory);
            }

            Console.WriteLine();
            Console.WriteLine("Projects not referenced:");
            foreach (var projectNotReferenced in output.ProjectsNotReferenced)
            {
                bool shouldKeep = false;
                foreach (var excludeDirectory in excludeDirectories)
                {
                    if (projectNotReferenced.StartsWith(excludeDirectory))
                    {
                        shouldKeep = true;
                        break;
                    }
                }

                if (!shouldKeep)
                {
                    Console.WriteLine(projectNotReferenced);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Commands to delete directories:");
            foreach (var projectNotReferenced in output.ProjectsNotReferenced)
            {
                bool shouldKeep = false;
                foreach (var excludeDirectory in excludeDirectories)
                {
                    if (projectNotReferenced.StartsWith(excludeDirectory))
                    {
                        shouldKeep = true;
                        break;
                    }
                }

                if (!shouldKeep)
                {
                    Console.WriteLine("git rm -r \"{0}\"", Path.GetDirectoryName(projectNotReferenced));
                }
            }

            Console.WriteLine();
            Console.WriteLine("dirs.proj file references a project not found:");
            foreach (var dirsProjectFileNotFound in output.DirsProjectFileNotFound)
            {
                Console.WriteLine(dirsProjectFileNotFound);
            }

            Console.WriteLine();
            Console.WriteLine("*.csproj file references a project not found:");
            foreach (var projectReferencesNotFound in output.ProjectReferencesNotFound)
            {
                Console.WriteLine(projectReferencesNotFound);
            }
        }
    }
}
