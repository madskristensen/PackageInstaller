using System;
using System.ComponentModel.Design;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace PackageInstaller
{
    internal sealed class InstallPackage
    {
        public const int CommandId = PackageCommands.InstallPackageId;
        public static readonly Guid CommandSet = GuidList.guidVSPackageCmdSet;
        private readonly Package package;
        private Project _project;

        private InstallPackage(Package package)
        {
            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new OleMenuCommand(ShowInstallDialog, menuCommandID);
                menuItem.BeforeQueryStatus += BeforeQueryStatus;
                commandService.AddCommand(menuItem);
            }
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            var button = (OleMenuCommand)sender;
            _project = ProjectHelpers.GetSelectedProject();

            button.Enabled = button.Visible = _project != null;
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

        private async void ShowInstallDialog(object sender, EventArgs e)
        {
            Project project = _project ?? ProjectHelpers.GetSelectedProject();

            if (project == null)
                return;

            InstallDialog dialog = new InstallDialog(ServiceProvider, new NuGet(), new Bower(), new Npm(), new Jspm());
            var result = dialog.ShowDialog();

            if (!dialog.DialogResult.HasValue || !dialog.DialogResult.Value)
                return;

            VSPackage.UpdateStatus($"Installing {dialog.Package} package from {dialog.Provider.Name}...");

            await dialog.Provider.InstallPackage(project, dialog.Package, dialog.Version);
        }
    }
}
