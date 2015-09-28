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
    class Tsd : BasePackageProvider
    {
        private static bool _isDownloading;
        private static ImageSource _icon = BitmapFrame.Create(new Uri("pack://application:,,,/PackageInstaller;component/Resources/tsd.png", UriKind.RelativeOrAbsolute));

        public override string Name
        {
            get { return "TSD"; }
        }

        public override ImageSource Icon
        {
            get { return _icon; }
        }

        public override async Task<IEnumerable<string>> GetPackages(string term = null)
        {
            string file = Path.Combine(Path.GetTempPath(), "tsd-registry.txt");
            string url = "http://definitelytyped.org/tsd/data/repository.json";

            return await UpdateFileCache(file, url);
        }

        public async override Task<IEnumerable<string>> GetVersion(string packageName)
        {
            return await Task.FromResult(Enumerable.Empty<string>());
        }

        public override async Task<bool> InstallPackage(Project project, string packageName, string version)
        {
            if (!string.IsNullOrEmpty(version))
                packageName += $"@{version}";

            string arg = $"/c tsd install {packageName} -s";
            string cwd = project.GetRootFolder();

            return await CallCommand(arg, cwd);
        }

        private static async Task<IEnumerable<string>> UpdateFileCache(string file, string url)
        {
            if (!File.Exists(file))
            {
                using (var client = new WebClient())
                {
                    string json = await client.DownloadStringTaskAsync(url);
                    var list = ToList(json);
                    File.WriteAllLines(file, list);

                    return list;
                }
            }

            if (!_isDownloading && File.GetLastWriteTime(file) < DateTime.Now.AddDays(-1))
            {
                _isDownloading = true;

                System.Threading.ThreadPool.QueueUserWorkItem((o) =>
                {
                    try
                    {
                        using (var client = new WebClient())
                        {
                            string json = client.DownloadString(url);
                            var list = ToList(json);
                            File.WriteAllLines(file, list);
                        }
                    }
                    catch (Exception) { }

                    _isDownloading = false;
                });
            }

            return await Task.Run(() => File.ReadAllLines(file));
        }

        private static IEnumerable<string> ToList(string json)
        {
            var doc = JObject.Parse(json);

            return ((JArray)doc["content"])
                      .Select(prop => prop["name"].Value<string>());
        }
    }
}
