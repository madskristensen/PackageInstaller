using System;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace PackageInstaller
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Version, IconResourceID = 400)] // Info on this package for Help/About
    [Guid(PackageGuids.guidVSPackageString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class VSPackage : Package
    {
        public const string Version = "1.0.0";
        public const string Name = "Package Installer";
        public static DTE2 _dte;
        private static Dispatcher _dispatcher;

        protected override void Initialize()
        {
            base.Initialize();
            _dte = GetService(typeof(DTE)) as DTE2;
            _dispatcher = Dispatcher.CurrentDispatcher;

            Logger.Initialize(this, "Package Installer");
            InstallPackage.Initialize(this);
        }

        public static void UpdateStatus(string text)
        {
            _dispatcher.BeginInvoke(new Action(() =>
            {
                _dte.StatusBar.Text = text;
            }), DispatcherPriority.ApplicationIdle, null);
        }

        public static void AnimateStatusBar(bool animate)
        {
            _dispatcher.BeginInvoke(new Action(() =>
            {
                _dte.StatusBar.Animate(animate, vsStatusAnimation.vsStatusAnimationGeneral);
            }), DispatcherPriority.ApplicationIdle, null);
        }
    }
}
