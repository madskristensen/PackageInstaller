using System;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace PackageInstaller
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Version, IconResourceID = 400)] // Info on this package for Help/About
    [Guid(GuidList.guidVSPackageString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class VSPackage : Package
    {
        public const string Version = "1.0.0";
        public const string Name = "Package Installer";
        public static DTE2 _dte;

        protected override void Initialize()
        {
            base.Initialize();
            _dte = GetService(typeof(DTE)) as DTE2;

            Logger.Initialize(this, "Package Installer");
            InstallPackage.Initialize(this);
        }
    }
}
