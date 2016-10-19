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
    [ProvideOptionPage(typeof(Settings), "Web", Vsix.Name, 101, 111, true, new[] { "npm", "tsd", "jspm", "bower", "nuget", "yarn" }, ProvidesLocalizedCategoryName = false)]
    [Guid(PackageGuids.guidVSPackageString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class PackageInstallerPackage : Package
    {
        public static DTE2 _dte;
        internal static Settings Settings;

        protected override void Initialize()
        {
            _dte = GetService(typeof(DTE)) as DTE2;
            Settings = (Settings)GetDialogPage(typeof(Settings));

            Logger.Initialize(this, Vsix.Name);
            InstallPackage.Initialize(this);
        }

        public static void UpdateStatus(string text)
        {
            ThreadHelper.Generic.BeginInvoke(DispatcherPriority.ApplicationIdle, () =>
            {
                _dte.StatusBar.Text = text;
            });
        }

        public static void AnimateStatusBar(bool animate)
        {
            ThreadHelper.Generic.BeginInvoke(DispatcherPriority.ApplicationIdle, () =>
            {
                _dte.StatusBar.Animate(animate, vsStatusAnimation.vsStatusAnimationGeneral);
            });
        }
    }
}
