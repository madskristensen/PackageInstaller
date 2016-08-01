using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json.Linq;
using NuGet.VisualStudio;

namespace PackageInstaller
{
    class NuGet : BasePackageProvider
    {
        private static ImageSource _icon = BitmapFrame.Create(new Uri("pack://application:,,,/PackageInstaller;component/Resources/nuget.png", UriKind.RelativeOrAbsolute));

        public override string Name
        {
            get { return "NuGet"; }
        }

        public override ImageSource Icon
        {
            get { return _icon; }
        }

        public override string DefaultArguments
        {
            get { return null; }
        }

        public override bool EnableDynamicSearch
        {
            get { return true; }
        }

        public override async Task<IEnumerable<string>> GetPackagesInternal(string term)
        {
            // The API index: https://api.nuget.org/v3/index.json
            string endpoint = "https://api-v3search-0.nuget.org/autocomplete?q=";
            string url = endpoint + Uri.EscapeUriString(term);

            using (var client = new WebClient())
            {
                string json = await client.DownloadStringTaskAsync(url);
                return ToList(json);
            }
        }

        public async override Task<IEnumerable<string>> GetVersionInternal(string packageName)
        {
            string url = $"http://api-v3search-0.nuget.org/autocomplete?id={Uri.EscapeUriString(packageName)}&prerelease=true";

            using (var client = new WebClient())
            {
                string json = await client.DownloadStringTaskAsync(url);
                return ToList(json).Reverse();
            }
        }

        private static IEnumerable<string> ToList(string json)
        {
            var root = JObject.Parse(json);
            var array = (JArray)root["data"];

            return array.Select(a => a.ToString());
        }

        public override async Task<bool> InstallPackage(Project project, string packageName, string version, string args = null)
        {
            var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            var installer = componentModel.GetService<IVsPackageInstaller>();

            return await System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    installer.InstallPackage(null, project, packageName, (Version)null, false);
                    PackageInstallerPackage.UpdateStatus("Package installed");
                    return true;
                }
                catch (Exception ex)
                {
                    PackageInstallerPackage.UpdateStatus("An error installing package. See output window for details");
                    Logger.Log(ex);
                    return false;
                }
            });
        }
    }
}
