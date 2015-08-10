using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PackageInstaller
{
    public partial class InstallDialog : Window
    {
        private IEnumerable<IPackageProvider> _providers;
        private string _lastSearch;
        private const string LATEST = "Latest version";
        private const string LOADING = "Loading...";

        public InstallDialog(params IPackageProvider[] providers)
        {
            InitializeComponent();

            _providers = providers;

            Loaded += (s, e) =>
            {
                cbName.Focus();

                cbVersion.ItemsSource = new[] { LATEST };
                cbVersion.GotFocus += VersionFocus;

                cbType.ItemsSource = _providers;
                cbType.DisplayMemberPath = nameof(IPackageProvider.Name);
                cbType.SelectionChanged += CbType_SelectionChanged;
                cbType.SelectedIndex = 0;
            };
        }

        private async void VersionFocus(object sender, EventArgs e)
        {
            if (cbName.Text == _lastSearch)
                return;

            _lastSearch = cbName.Text;

            cbVersion.ItemsSource = new[] { LOADING };
            cbVersion.SelectedIndex = 0;

            var versions = await Provider.GetVersion(cbName.Text.Trim());

            if (versions.Any())
                cbVersion.ItemsSource = versions;
            else
                cbVersion.ItemsSource = new[] { LATEST };

            //cbVersion.SelectedIndex = 0;
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

        public string Version
        {
            get
            {
                if (cbVersion.Text == LATEST || cbVersion.Text == LOADING || string.IsNullOrWhiteSpace(cbVersion.Text))
                    return null;

                return cbVersion.Text;
            }
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
