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
        // The API index: https://api.nuget.org/v3/index.json
        private static string _baseUrl = "https://api-v2v3search-0.nuget.org/autocomplete";

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
            string url = $"{_baseUrl}?q={Uri.EscapeUriString(term)}";

            using (var client = new WebClient())
            {
                string json = await client.DownloadStringTaskAsync(url);
                return ToList(json);
            }
        }

        public async override Task<IEnumerable<string>> GetVersionInternal(string packageName)
        {
            string url = $"{_baseUrl}?id={Uri.EscapeUriString(packageName)}&prerelease=true";

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

            return await System.Threading.Tasks.Task.Run(async() =>
            {
                try
                {
                    installer.InstallPackage(null, project, packageName, (Version)null, false);
                    await PackageInstallerPackage.UpdateStatusAsync("Package installed");
                    return true;
                }
                catch (Exception ex)
                {
                    await PackageInstallerPackage.UpdateStatusAsync("An error installing package. See output window for details");
                    Logger.Log(ex);
                    return false;
                }
            });
        }
    }
}
