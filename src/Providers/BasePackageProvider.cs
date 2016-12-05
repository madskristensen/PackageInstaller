using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using EnvDTE;

namespace PackageInstaller
{
    public abstract class BasePackageProvider : IPackageProvider
    {
        public abstract string Name { get; }

        public abstract ImageSource Icon { get; }

        public abstract string DefaultArguments { get; }

        public virtual async Task<IEnumerable<string>> GetPackagesAsync(string term)
        {
            if (string.IsNullOrEmpty(term))
                return Enumerable.Empty<string>();

            try
            {
                return await GetPackagesInternal(term);
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }

            return Enumerable.Empty<string>();
        }

        public abstract Task<IEnumerable<string>> GetPackagesInternal(string term = null);

        public abstract Task<bool> InstallPackage(Project project, string packageName, string version, string args = null);

        public async Task<IEnumerable<string>> GetVersionAsync(string packageName)
        {
            try
            {
                return await GetVersionInternal(packageName);
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }

            return Enumerable.Empty<string>();
        }

        public abstract Task<IEnumerable<string>> GetVersionInternal(string packageName);

        public virtual bool EnableDynamicSearch { get { return false; } }

        protected virtual async Task<bool> CallCommandAsync(string argument, string cwd)
        {
            ProcessStartInfo start = new ProcessStartInfo
            {
                WorkingDirectory = cwd,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe",
                Arguments = argument,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8,
            };

            ModifyPathVariable(start);

            try
            {
                using (var p = System.Diagnostics.Process.Start(start))
                {
                    var error = await p.StandardError.ReadToEndAsync();
                    var output = await p.StandardOutput.ReadToEndAsync();
                    p.WaitForExit();

                    Logger.Log(output, true);

                    if (p.ExitCode == 0)
                    {
                        await PackageInstallerPackage.UpdateStatus("Package installed");
                    }
                    else
                    {
                        await PackageInstallerPackage.UpdateStatus("An error installing package. See output window for details");
                        Logger.Log(error, true);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                await PackageInstallerPackage.UpdateStatus("An error installing package. See output window for details");
                Logger.Log(ex, true);
                return false;
            }
        }

        protected static void ModifyPathVariable(ProcessStartInfo start)
        {
            string path = ".\\node_modules\\.bin" + ";" + start.EnvironmentVariables["PATH"];

            var process = System.Diagnostics.Process.GetCurrentProcess();
            string ideDir = Path.GetDirectoryName(process.MainModule.FileName);

            if (Directory.Exists(ideDir))
            {
                string parent = Directory.GetParent(ideDir).Parent.FullName;

                string rc2Preview1Path = new DirectoryInfo(Path.Combine(parent, @"Web\External")).FullName;

                if (Directory.Exists(rc2Preview1Path))
                {
                    path += ";" + rc2Preview1Path;
                    path += ";" + rc2Preview1Path + "\\git";
                }
                else
                {
                    path += ";" + Path.Combine(ideDir, @"Extensions\Microsoft\Web Tools\External");
                    path += ";" + Path.Combine(ideDir, @"Extensions\Microsoft\Web Tools\External\git");
                }
            }

            start.EnvironmentVariables["PATH"] = path;
        }

        public override string ToString()
        {
            return Name;
        }

        public virtual string GetInstallArguments(string name, string version)
        {
            return null;
        }
    }
}
