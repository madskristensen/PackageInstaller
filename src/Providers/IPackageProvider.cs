using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace PackageInstaller
{
    public interface IPackageProvider
    {
        string Name { get; }

        Task<IEnumerable<string>> GetPackages();

        void InstallPackage(Project project, string packageName, string version);

        Task<IEnumerable<string>> GetVersion(string packageName);
    }
}
