using System;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace PackageInstaller
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [ProvideOptionPage(typeof(Settings), "Web", Vsix.Name, 101, 111, true, new[] { "npm", "tsd", "jspm", "bower", "nuget" }, ProvidesLocalizedCategoryName = false)]
    [Guid(PackageGuids.guidVSPackageString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class PackageInstallerPackage : Package
    {
        public static DTE2 _dte;
        private static Dispatcher _dispatcher;
        internal static Settings Settings;

        protected override void Initialize()
        {
            base.Initialize();
            _dte = GetService(typeof(DTE)) as DTE2;
            _dispatcher = Dispatcher.CurrentDispatcher;
            Settings = (Settings)GetDialogPage(typeof(Settings));

            Logger.Initialize(this, Vsix.Name);
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
