using System;
using System.ComponentModel.Design;
using System.Windows;
using System.Windows.Interop;
using EnvDTE;
using EnvDTE80;
using Microsoft;
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
                var menuItem = new OleMenuCommand(ShowInstallDialogAsync, menuCommandID);
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
            var dte = (DTE2)ServiceProvider.GetService(typeof(DTE));
            var button = (OleMenuCommand)sender;
            _project = ProjectHelpers.GetSelectedProject() ?? GetActiveDocumentProject(dte);

            button.Enabled = button.Visible = _project != null;
        }

        private async void ShowInstallDialogAsync(object sender, EventArgs e)
        {
            var dte = (DTE2)ServiceProvider.GetService(typeof(DTE));
            Assumes.Present(dte);
            Project project = _project ?? ProjectHelpers.GetSelectedProject() ?? GetActiveDocumentProject(dte);

            if (project == null)
                return;

            InstallDialog dialog = new InstallDialog(ServiceProvider, new Bower(), new Jspm(), new Npm(), new NuGet(), new Tsd(), new Typings(), new Yarn());

            dialog.Owner = Application.Current.MainWindow;

            var result = dialog.ShowDialog();

            if (!dialog.DialogResult.HasValue || !dialog.DialogResult.Value)
                return;

            try
            {
                await PackageInstallerPackage.AnimateStatusBarAsync(true);
                await PackageInstallerPackage.UpdateStatusAsync($"Installing {dialog.Package} package from {dialog.Provider.Name}...");
                Logger.Log($"Installing {dialog.Package} package from {dialog.Provider.Name}...");

                await dialog.Provider.InstallPackage(project, dialog.Package, dialog.Version, dialog.Arguments);
            }
            finally
            {
                await PackageInstallerPackage.AnimateStatusBarAsync(false);
                await PackageInstallerPackage.HideStatusAsync(3000);
            }
        }

        private static Project GetActiveDocumentProject(DTE2 dte)
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
