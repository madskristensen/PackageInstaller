using System;
using Microsoft.VisualStudio.Shell.Interop;

namespace PackageInstaller
{
    public static class Logger
    {
        private static IVsOutputWindowPane pane;
        private static object _syncRoot = new object();
        private static IServiceProvider _provider;
        private static string _name;

        public static void Initialize(IServiceProvider provider, string name)
        {
            _provider = provider;
            _name = name;
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
            }
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
