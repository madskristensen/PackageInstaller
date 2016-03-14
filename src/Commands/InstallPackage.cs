using System;
using System.ComponentModel.Design;
using System.Windows.Interop;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace PackageInstaller
{
    internal sealed class InstallPackage
    {
        private readonly Package package;
        private Project _project;

        private InstallPackage(Package package)
        {
            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(PackageGuids.guidVSPackageCmdSet, PackageIds.InstallPackageId);
                var menuItem = new OleMenuCommand(ShowInstallDialog, menuCommandID);
                menuItem.BeforeQueryStatus += BeforeQueryStatus;
                commandService.AddCommand(menuItem);
            }
        }

        public static InstallPackage Instance { get; private set; }

        private IServiceProvider ServiceProvider
        {
            get { return package; }
        }

        public static void Initialize(Package package)
        {
            Instance = new InstallPackage(package);
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            var dte = (DTE)ServiceProvider.GetService(typeof(DTE));
            var button = (OleMenuCommand)sender;
            _project = ProjectHelpers.GetSelectedProject() ?? GetActiveDocumentProject(dte);

            button.Enabled = button.Visible = _project != null;
        }

        private async void ShowInstallDialog(object sender, EventArgs e)
        {
            var dte = (DTE)ServiceProvider.GetService(typeof(DTE));
            Project project = _project ?? ProjectHelpers.GetSelectedProject() ?? GetActiveDocumentProject(dte);

            if (project == null)
                return;

            InstallDialog dialog = new InstallDialog(ServiceProvider, new Bower(), new Jspm(), new Npm(), new NuGet(), new Tsd());

            var hwnd = new IntPtr(dte.MainWindow.HWnd);
            System.Windows.Window window = (System.Windows.Window)HwndSource.FromHwnd(hwnd).RootVisual;
            dialog.Owner = window;

            var result = dialog.ShowDialog();

            if (!dialog.DialogResult.HasValue || !dialog.DialogResult.Value)
                return;

            VSPackage.AnimateStatusBar(true);
            VSPackage.UpdateStatus($"Installing {dialog.Package} package from {dialog.Provider.Name}...");

            await dialog.Provider.InstallPackage(project, dialog.Package, dialog.Version, dialog.Arguments);

            Logger.PackageInstall(dialog.Provider.Name, dialog.Package);

            VSPackage.AnimateStatusBar(false);
        }

        private static Project GetActiveDocumentProject(DTE dte)
        {
            if (dte.ActiveWindow == null || dte.ActiveWindow.Type != vsWindowType.vsWindowTypeDocument)
                return null;

            var doc = dte.ActiveDocument;

            if (doc == null || string.IsNullOrEmpty(doc.FullName))
                return null;

            ProjectItem item = dte.Solution.FindProjectItem(doc.FullName);

            return item?.ContainingProject;
        }
    }
}
