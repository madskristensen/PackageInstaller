using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    class Jspm : BasePackageProvider
    {
        private static bool _isDownloading;
        private static ImageSource _icon = BitmapFrame.Create(new Uri("pack://application:,,,/PackageInstaller;component/Resources/jspm.png", UriKind.RelativeOrAbsolute));

        public override string Name
        {
            get { return "JSPM"; }
        }

        public override ImageSource Icon
        {
            get { return _icon; }
        }

        public override async Task<IEnumerable<string>> GetPackages()
        {
            string file = Path.Combine(Path.GetTempPath(), "jspm-registry.txt");
            string url = "https://raw.githubusercontent.com/jspm/registry/master/registry.json";

            return await UpdateFileCache(file, url);
        }

        public async override Task<IEnumerable<string>> GetVersion(string packageName)
        {
            return await Task.FromResult(Enumerable.Empty<string>());
        }

        public override void InstallPackage(Project project, string packageName, string version)
        {
            if (!string.IsNullOrEmpty(version))
                packageName += $"@{version}";

            string arg = $"/c jspm install {packageName}";
            string cwd = project.GetRootFolder();
            string json = Path.Combine(cwd, "package.json");

            if (!File.Exists(json))
            {
                string content = "{\"name\":\"myproject\", \"version\":\"1.0.0\"}";
                File.WriteAllText(json, content, new UTF8Encoding(false));
                project.ProjectItems.AddFromFile(json);
            }

            if (IsJspmConfigured(json))
                CallCommand(arg, cwd);
            else
                ShowConsole(arg, cwd);
        }

        private bool IsJspmConfigured(string packageJsonFile)
        {
            try
            {
                JObject root = JObject.Parse(File.ReadAllText(packageJsonFile));
                var jspm = root.Children<JProperty>().FirstOrDefault(prop => prop.Name == "jspm");
                return jspm != null;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return false;
        }

        protected async void ShowConsole(string argument, string cwd)
        {
            ProcessStartInfo start = new ProcessStartInfo
            {
                WorkingDirectory = cwd,
                UseShellExecute = false,
                FileName = "cmd.exe",
                Arguments = argument,
                RedirectStandardError = true,
                StandardErrorEncoding = Encoding.UTF8,
            };

            ModifyPathVariable(start);

            try
            {
                var p = System.Diagnostics.Process.Start(start);
                var error = await p.StandardError.ReadToEndAsync();
                p.WaitForExit();
                p.Dispose();

                if (string.IsNullOrEmpty(error))
                {
                    VSPackage._dte.StatusBar.Text = "Package installed";
                }
                else
                {
                    VSPackage._dte.StatusBar.Text = "An error installing package. See output window for details";
                    Logger.Log(error, true);
                }
            }
            catch (Exception ex)
            {
                VSPackage._dte.StatusBar.Text = "An error installing package. See output window for details";
                Logger.Log(ex, true);
            }
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

            return doc.Children<JProperty>()
                      .OrderBy(prop => prop.Name)
                      .Select(prop => prop.Name);
        }
    }
}
