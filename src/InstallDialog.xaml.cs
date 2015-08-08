using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PackageInstaller
{
    public partial class InstallDialog : Window
    {
        private IEnumerable<IPackageProvider> _providers;

        public InstallDialog(params IPackageProvider[] providers)
        {
            InitializeComponent();

            _providers = providers;

            Loaded += (s, e) =>
            {
                cbName.Focus();

                cbVersion.ItemsSource = new[] { "Latest version" };
                cbVersion.GotFocus += VersionFocus;

                cbType.ItemsSource = _providers;
                cbType.DisplayMemberPath = nameof(IPackageProvider.Name);
                cbType.SelectionChanged += CbType_SelectionChanged;
                cbType.SelectedIndex = 0;
            };
        }

        private async void VersionFocus(object sender, RoutedEventArgs e)
        {
            cbVersion.ItemsSource = new[] { "Loading..." };

            var versions = await Provider.GetVersion(cbName.Text.Trim());

            if (versions.Any())
                cbVersion.ItemsSource = versions;
            else
                cbVersion.ItemsSource = new[] { "Latest version" };

            cbVersion.SelectedIndex = 0;
        }

        private void CbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetPackages();
        }

        public string Package
        {
            get { return cbName.Text; }
        }

        public IPackageProvider Provider
        {
            get { return (IPackageProvider)cbType.SelectedItem; }
        }

        private async void GetPackages()
        {
            cbName.ItemsSource = await Provider.GetPackages();
        }

        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
