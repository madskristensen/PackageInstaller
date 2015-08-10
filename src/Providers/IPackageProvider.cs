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

        Task<IEnumerable<string>> GetPackages();

        void InstallPackage(Project project, string packageName, string version);

        Task<IEnumerable<string>> GetVersion(string packageName);
    }
}
