using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PackageInstaller
{
    class Yarn : Npm
    {
        private static ImageSource _icon = BitmapFrame.Create(new Uri("pack://application:,,,/PackageInstaller;component/Resources/yarn.png", UriKind.RelativeOrAbsolute));

        public override string Name
        {
            get { return "Yarn"; }
        }

        public override ImageSource Icon
        {
            get { return _icon; }
        }

        public override string DefaultArguments
        {
            get { return PackageInstallerPackage.Settings.YarnArguments; }
        }

        public override string GetInstallArguments(string name, string version)
        {
            string args = $"yarn add {name}";

            if (!string.IsNullOrEmpty(version))
                args = $"{args}@{version}";

            return args;
        }
    }
}
