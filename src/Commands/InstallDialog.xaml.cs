using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;

namespace PackageInstaller
{
    public partial class InstallDialog : Window
    {
        private SettingsManager _settings;
        private IEnumerable<IPackageProvider> _providers;
        private string _lastSearch;
        private const string LATEST = "Latest version";
        private const string LOADING = "Loading...";

        public InstallDialog(IServiceProvider serviceProvider, params IPackageProvider[] providers)
        {
            InitializeComponent();

            _providers = providers;

            Loaded += (s, e) =>
            {
                _settings = new ShellSettingsManager(serviceProvider);

                Closing += StoreLastUsed;
                cbName.Focus();

                cbVersion.ItemsSource = new[] { LATEST };
                cbVersion.GotFocus += VersionFocus;

                cbType.ItemsSource = _providers;
                cbType.DisplayMemberPath = nameof(IPackageProvider.Name);
                cbType.SelectionChanged += TypeChanged;

                string lastUsed = GetLastUsed();

                if (string.IsNullOrEmpty(lastUsed))
                {
                    cbType.SelectedIndex = 0;
                }
                else
                {
                    var provider = providers.FirstOrDefault(p => p.Name.Equals(lastUsed, StringComparison.OrdinalIgnoreCase));
                    if (provider != null)
                        cbType.SelectedItem = provider;
                }
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

            if (string.IsNullOrWhiteSpace(cbVersion.Text))
                cbVersion.SelectedIndex = 0;
        }

        private void TypeChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = (ComboBox)sender;
            IPackageProvider provider = (IPackageProvider)box.SelectedItem;


            Icon = provider.Icon;
            Title = $"Install {provider.Name} package";

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

        private void cbName_TextChanged(object sender, RoutedEventArgs e)
        {
            cbVersion.IsEnabled = !string.IsNullOrWhiteSpace(cbName.Text);

            cbVersion.ItemsSource = new[] { LATEST };
            cbVersion.SelectedIndex = 0;

            if (!cbVersion.IsEnabled)
                cbVersion.Text = LATEST;
        }

        private string GetLastUsed()
        {
            try {
                SettingsStore store = _settings.GetReadOnlySettingsStore(SettingsScope.UserSettings);
                return store.GetString("PackageInstaller", "type", null);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return null;
            }
        }

        private void StoreLastUsed(object sender, EventArgs e)
        {
            Closing -= StoreLastUsed;

            try
            {
                WritableSettingsStore wstore = _settings.GetWritableSettingsStore(SettingsScope.UserSettings);

                if (!wstore.CollectionExists("PackageInstaller"))
                    wstore.CreateCollection("PackageInstaller");

                wstore.SetString("PackageInstaller", "type", cbType.Text);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}
