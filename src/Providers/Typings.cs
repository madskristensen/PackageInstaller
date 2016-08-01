using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EnvDTE;
using Newtonsoft.Json.Linq;

namespace PackageInstaller
{
    class Typings : BasePackageProvider
    {
        private static ImageSource _icon = BitmapFrame.Create(new Uri("pack://application:,,,/PackageInstaller;component/Resources/typings.png", UriKind.RelativeOrAbsolute));

        public override string Name
        {
            get { return "Typings"; }
        }

        public override ImageSource Icon
        {
            get { return _icon; }
        }

        public override string DefaultArguments
        {
            get { return PackageInstallerPackage.Settings.TypingsArguments; }
        }

        public override bool EnableDynamicSearch
        {
            get { return true; }
        }

        public override async Task<IEnumerable<string>> GetPackagesInternal(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Enumerable.Empty<string>();

            string endpoint = "https://api.typings.org/search?query=";
            string url = endpoint + Uri.EscapeUriString(term);

            using (var client = new WebClient())
            {
                string json = await client.DownloadStringTaskAsync(url);
                return ToList(json);
            }
        }

        public async override Task<IEnumerable<string>> GetVersionInternal(string packageName)
        {
            return await Task.FromResult(Enumerable.Empty<string>());
        }

        private static IEnumerable<string> ToList(string json)
        {
            var root = JObject.Parse(json);
            var array = (JArray)root["results"];


            foreach (JObject obj in array)
            {
                yield return obj["name"].ToString();
            }
        }

        public override async Task<bool> InstallPackage(Project project, string packageName, string version, string args = null)
        {
            string installArgs = GetInstallArguments(packageName, version);

            string arg = $"/c {installArgs} {args}";
            string cwd = project.GetRootFolder();

            return await CallCommand(arg, cwd);
        }

        public override string GetInstallArguments(string name, string version)
        {
            string args = $"typings install {name}";

            if (!string.IsNullOrEmpty(version))
                args = $"{args}@{version}";

            return args;
        }
    }
}
