using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using EnvDTE;

namespace PackageInstaller
{
    public abstract class BasePackageProvider : IPackageProvider
    {
        private StringBuilder _error = new StringBuilder();
        public abstract string Name { get; }

        public abstract ImageSource Icon { get; }

        public abstract Task<IEnumerable<string>> GetPackages();

        public abstract void InstallPackage(Project project, string packageName, string version);

        public abstract Task<IEnumerable<string>> GetVersion(string packageName);

        protected virtual async void CallCommand(string argument, string cwd)
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

        private void P_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                _error.Append(e.Data);
        }

        protected static void ModifyPathVariable(ProcessStartInfo start)
        {
            string path = start.EnvironmentVariables["PATH"];

            string toolsDir = Environment.GetEnvironmentVariable("VS140COMNTOOLS");

            if (Directory.Exists(toolsDir))
            {
                string parent = Directory.GetParent(toolsDir).Parent.FullName;
                path += ";" + Path.Combine(parent, @"IDE\Extensions\Microsoft\Web Tools\External");
            }

            start.EnvironmentVariables["PATH"] = path;
        }
    }
}
