using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EnvDTE;
using Newtonsoft.Json.Linq;

namespace PackageInstaller
{
    class Npm : BasePackageProvider
    {
        private static ImageSource _icon = BitmapFrame.Create(new Uri("pack://application:,,,/PackageInstaller;component/Resources/npm.png", UriKind.RelativeOrAbsolute));

        public override string Name
        {
            get { return "npm"; }
        }

        public override ImageSource Icon
        {
            get { return _icon; }
        }

        public override bool EnableDynamicSearch
        {
            get { return true; }
        }

        public override async Task<IEnumerable<string>> GetPackages(string term = null)
        {
            if (string.IsNullOrEmpty(term))
                return Enumerable.Empty<string>();

            string url = $"https://ac.cnstrc.com/autocomplete/{Uri.EscapeUriString(term)}?autocomplete_key=CD06z4gVeqSXRiDL2ZNK";

            using (var client = new WebClient())
            {
                string json = await client.DownloadStringTaskAsync(url);
                return ToList(json);
            }
        }

        private static IEnumerable<string> ToList(string json)
        {
            var root = JObject.Parse(json);
            var array = (JArray)root["sections"]["packages"];

            return array.Select(a => a["value"].ToString());
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

        public override async Task<bool> InstallPackage(Project project, string packageName, string version)
        {
            if (!string.IsNullOrEmpty(version))
                packageName += $"@{version}";

            string arg = $"/c npm install {packageName} --save";
            string cwd = project.GetRootFolder();
            string json = Path.Combine(cwd, "package.json");

            if (!File.Exists(json))
            {
                string content = "{\"name\":\"myproject\", \"version\":\"1.0.0\"}";
                File.WriteAllText(json, content, new UTF8Encoding(false));
                project.ProjectItems.AddFromFile(json);
            }

            AddAdditionalFiles(project, cwd, packageName);
            return await CallCommand(arg, cwd);
        }

        private static void AddAdditionalFiles(Project project, string cwd, string packageName)
        {
            string file = string.Empty;
            string content = string.Empty;

            if (packageName == "gulp")
            {
                file = "gulpfile.js";
                content = "var gulp = require(\"gulp\");" + Environment.NewLine;
            }
            else if (packageName == "grunt")
            {
                file = "gruntfile.js";
                content = "var gulp = require(\"grunt\");" + Environment.NewLine;
            }
            else if (packageName == "broccoli")
            {
                file = "brocfile.js";
            }

            string fullName = Path.Combine(cwd, file);

            if (!string.IsNullOrEmpty(file) && !File.Exists(fullName))
            {
                try
                {
                    File.WriteAllText(fullName, content);
                    VSPackage._dte.ItemOperations.OpenFile(fullName);
                    project.AddFileToProject(fullName, "None");
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }
        }
    }
}
