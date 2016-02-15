using System;
using EnvDTE;
using Microsoft.ApplicationInsights;
using Microsoft.VisualStudio.Shell.Interop;

namespace PackageInstaller
{
    public static class Logger
    {
        private static IVsOutputWindowPane pane;
        private static object _syncRoot = new object();
        private static IServiceProvider _provider;
        private static string _name;
        private static TelemetryClient _telemetry = GetAppInsightsClient();
        private static DTEEvents _events;

        public static void Initialize(IServiceProvider provider, string name)
        {
            _provider = provider;
            _name = name;

            var dte = (DTE)_provider.GetService(typeof(DTE));
            _events = dte.Events.DTEEvents;
            _events.OnBeginShutdown += delegate
            {
                _telemetry.Flush();
            };
        }

        public static void Log(string message, bool showOutputWindow = false)
        {
            if (string.IsNullOrEmpty(message))
                return;

            try
            {
                if (EnsurePane())
                {
                    pane.OutputString(DateTime.Now.ToString() + ": " + message + Environment.NewLine);

                    if (showOutputWindow)
                        pane.Activate();
                }
            }
            catch
            {
                // Do nothing
            }
        }

        public static void Log(Exception ex, bool showOutputWindow = false)
        {
            if (ex != null)
            {
                Log(ex.ToString(), showOutputWindow);
#if !DEBUG
                _telemetry.TrackException(new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(ex));
#endif
            }
        }

        public static void PackageInstall(string providerName, string packageName)
        {
#if !DEBUG
            var evt = new Microsoft.ApplicationInsights.DataContracts.EventTelemetry(providerName);
            evt.Properties.Add("Package", packageName);

            _telemetry.TrackEvent(evt);
#endif
        }

        private static TelemetryClient GetAppInsightsClient()
        {
            TelemetryClient client = new TelemetryClient();
            client.InstrumentationKey = "56089e10-7937-4edd-b3e9-585cac8a8a88";
            client.Context.Component.Version = Vsix.Version;
            client.Context.Session.Id = Guid.NewGuid().ToString();
            client.Context.User.Id = (Environment.UserName + Environment.MachineName).GetHashCode().ToString();

            return client;
        }

        private static bool EnsurePane()
        {
            if (pane == null)
            {
                Guid guid = Guid.NewGuid();
                IVsOutputWindow output = (IVsOutputWindow)_provider.GetService(typeof(SVsOutputWindow));
                output.CreatePane(ref guid, _name, 1, 1);
                output.GetPane(ref guid, out pane);
            }

            return pane != null;
        }
    }
}
