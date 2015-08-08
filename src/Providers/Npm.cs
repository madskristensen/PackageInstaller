using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Newtonsoft.Json.Linq;

namespace PackageInstaller
{
    class Npm : BasePackageProvider
    {
        public override string Name
        {
            get { return "npm"; }
        }

        public override async Task<IEnumerable<string>> GetPackages()
        {
            string file = Path.Combine(Path.GetTempPath(), "bower-registry.txt");
            string url = "https://bower-component-list.herokuapp.com/";

            return await UpdateFileCache(file, url);
        }

        public async override Task<IEnumerable<string>> GetVersion(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
                return Enumerable.Empty<string>();

            string url = $"http://registry.npmjs.org/{packageName}";
            string json = "{}";

            using (var client = new WebClient())
            {
                json = await client.DownloadStringTaskAsync(url);
            }

            var array = JObject.Parse(json);
            var time = array["time"];

            if (time == null)
                return Enumerable.Empty<string>();

            var props = time.Children<JProperty>();

            return from version in props
                   where char.IsNumber(version.Name[0])
                   orderby version.Name descending
                   select version.Name;
    }

        public override void InstallPackage(Project project, string packageName, string version)
        {
            string arg = $"/c npm install {packageName} --save";
            string cwd = project.GetRootFolder();
            string json = Path.Combine(cwd, "package.json");

            if (!File.Exists(json))
            {
                string content = "{\"name\":\"myproject\", \"version\":\"1.0.0\"}";
                File.WriteAllText(json, content, new UTF8Encoding(false));
                project.ProjectItems.AddFromFile(json);
            }

            CallCommand(arg, cwd);
        }

        private static Task<IEnumerable<string>> UpdateFileCache(string file, string url)
        {
            return Task.FromResult(Enumerable.Empty<string>());
            //IEnumerable<string> list;

            //if (!File.Exists(file) || File.GetLastWriteTime(file) < DateTime.Now.AddDays(-1))
            //{
            //    using (var client = new WebClient())
            //    {
            //        string json = await client.DownloadStringTaskAsync(url);
            //        list = ToList(json);
            //        File.WriteAllLines(file, list);
            //    }
            //}
            //else
            //{
            //    using (TextReader reader = File.OpenText(file))
            //    {
            //        list = await Task.Run(() => File.ReadAllLines(file));
            //    }
            //}

            //return list;
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
