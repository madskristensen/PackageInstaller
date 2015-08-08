using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EnvDTE;
using Newtonsoft.Json.Linq;

namespace PackageInstaller
{
    class Bower : BasePackageProvider
    {
        public override string Name
        {
            get { return "Bower"; }
        }

        public override async Task<IEnumerable<string>> GetPackages()
        {
            string file = Path.Combine(Path.GetTempPath(), "bower-registry.txt");
            string url = "https://bower-component-list.herokuapp.com/";

            return await UpdateFileCache(file, url);
        }

        private static Regex _regex = new Regex(@"([\s]+)- (?<version>(\d)([\S]+))", RegexOptions.Compiled);

        public async override Task<IEnumerable<string>> GetVersion(string packageName)
        {
            var start = new System.Diagnostics.ProcessStartInfo("cmd", "/c bower info " + packageName)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                ErrorDialog = false,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8,
            };

            ModifyPathVariable(start);
            List<string> list = new List<string>();

            using (var p = System.Diagnostics.Process.Start(start))
            {
                string output = await p.StandardOutput.ReadToEndAsync();
                p.WaitForExit();

                foreach (Match match in _regex.Matches(output))
                {
                    list.Add(match.Groups["version"].Value);
                }
            }

            return list;
        }

        public override void InstallPackage(Project project, string packageName, string version)
        {
            string arg = $"/c bower install {packageName} --save --no-color";
            string cwd = project.GetRootFolder();
            string json = Path.Combine(cwd, "bower.json");

            if (!File.Exists(json))
            {
                string content = "{\"name\":\"myproject\"}";
                File.WriteAllText(json, content, new UTF8Encoding(false));
                project.ProjectItems.AddFromFile(json);
            }

            CallCommand(arg, cwd);
        }

        private static async Task<IEnumerable<string>> UpdateFileCache(string file, string url)
        {
            IEnumerable<string> list;

            if (!File.Exists(file) || File.GetLastWriteTime(file) < DateTime.Now.AddDays(-1))
            {
                using (var client = new WebClient())
                {
                    string json = await client.DownloadStringTaskAsync(url);
                    list = ToList(json);
                    File.WriteAllLines(file, list);
                }
            }
            else
            {
                using (TextReader reader = File.OpenText(file))
                {
                    list = await Task.Run(() => File.ReadAllLines(file));
                }
            }

            return list;
        }

        private static IEnumerable<string> ToList(string json)
        {
            var array = JArray.Parse(json);

            return from obj in array
                   let children = obj.Children<JProperty>()
                   let name = children.First(prop => prop.Name == "name").Value.ToString()
                   let stars = int.Parse(children.First(prop => prop.Name == "stars").Value.ToString())
                   let updated = DateTime.Parse(children.First(prop => prop.Name == "updated").Value.ToString())
                   where stars > 3 && updated > DateTime.Now.AddMonths(-6)
                   orderby name, stars descending
                   select name;
        }
    }
}
