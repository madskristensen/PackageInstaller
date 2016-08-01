using System;
using System.IO;
using EnvDTE;
using EnvDTE80;

namespace PackageInstaller
{
    static class ProjectHelpers
    {
        private static DTE2 _dte = PackageInstallerPackage._dte;

        public static Project GetSelectedProject()
        {
            var items = (Array)_dte.ToolWindows.SolutionExplorer.SelectedItems;

            foreach (UIHierarchyItem selItem in items)
            {
                Project project = selItem.Object as Project;

                if (project != null)
                    return project;

                ProjectItem item = selItem.Object as ProjectItem;

                if (item != null)
                    return item.ContainingProject;
            }

            return null;
        }

        public static string GetRootFolder(this Project project)
        {
            if (string.IsNullOrEmpty(project.FullName))
                return null;

            string fullPath;

            try
            {
                fullPath = project.Properties.Item("FullPath").Value as string;
            }
            catch (ArgumentException)
            {
                try
                {
                    // MFC projects don't have FullPath, and there seems to be no way to query existence
                    fullPath = project.Properties.Item("ProjectDirectory").Value as string;
                }
                catch (ArgumentException)
                {
                    // Installer projects have a ProjectPath.
                    fullPath = project.Properties.Item("ProjectPath").Value as string;
                }
            }

            if (string.IsNullOrEmpty(fullPath))
                return File.Exists(project.FullName) ? Path.GetDirectoryName(project.FullName) : null;

            if (Directory.Exists(fullPath))
                return fullPath;

            if (File.Exists(fullPath))
                return Path.GetDirectoryName(fullPath);

            return null;
        }

        public static void AddFileToProject(this Project project, string file, string itemType = null)
        {
            if (project.Kind.Equals("{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}", StringComparison.OrdinalIgnoreCase)) // ASP.NET 5 projects
                return;

            try
            {
                ProjectItem item = project.ProjectItems.AddFromFile(file);

                if (string.IsNullOrEmpty(itemType) || project.Kind.Equals("{E24C65DC-7377-472B-9ABA-BC803B73C61A}", StringComparison.OrdinalIgnoreCase)) // Website
                    return;

                item.Properties.Item("ItemType").Value = "None";
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}
