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

        Task<IEnumerable<string>> GetPackages(string term = null);

        Task<bool> InstallPackage(Project project, string packageName, string version);

        Task<IEnumerable<string>> GetVersion(string packageName);

        /// <summary>
        /// Tells the installer to request packages on every keystroke
        /// </summary>
        bool EnableDynamicSearch { get; }
    }
}
