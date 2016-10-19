using System;
using System.Runtime.InteropServices;
using System.Windows;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using task = System.Threading.Tasks.Task;

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
        private static StatusbarControl _control;

        protected override void Initialize()
        {
            _dte = GetService(typeof(DTE)) as DTE2;
            Settings = (Settings)GetDialogPage(typeof(Settings));

            Logger.Initialize(this, Vsix.Name);
            InstallPackage.Initialize(this);

            _control = new StatusbarControl(Settings, _dte);

            var injector = new StatusBarInjector(Application.Current.MainWindow);
            injector.InjectControl(_control);
        }

        public static async task UpdateStatus(string text)
        {
            await ThreadHelper.Generic.InvokeAsync(() =>
            {
                _control.Text = text;
                _control.SetVisibility(Visibility.Visible);
            });
        }

        public static async task HideStatus(int wait = 0)
        {
            if (wait > 0)
                await task.Delay(wait);

            _control.Text = "";
            _control.SetVisibility(Visibility.Collapsed);
        }

        public static async task AnimateStatusBar(bool animate)
        {
            await ThreadHelper.Generic.InvokeAsync(() =>
            {
                _dte.StatusBar.Animate(animate, vsStatusAnimation.vsStatusAnimationGeneral);
            });
        }
    }
}
