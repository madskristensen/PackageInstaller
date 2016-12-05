using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using EnvDTE;

namespace PackageInstaller
{
    public interface IPackageProvider
    {
        string Name { get; }

        ImageSource Icon { get;  }

        string DefaultArguments { get; }

        Task<IEnumerable<string>> GetPackagesAsync(string term = null);

        Task<bool> InstallPackage(Project project, string packageName, string version, string args = null);

        Task<IEnumerable<string>> GetVersionAsync(string packageName);

        /// <summary>
        /// Tells the installer to request packages on every keystroke
        /// </summary>
        bool EnableDynamicSearch { get; }

        string GetInstallArguments(string name, string version);
    }
}
