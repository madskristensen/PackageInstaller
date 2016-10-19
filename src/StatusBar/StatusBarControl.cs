using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using EnvDTE80;

namespace PackageInstaller
{
    class StatusbarControl : TextBlock
    {
        private Settings _options;
        private DTE2 _dte;

        public StatusbarControl(Settings options, DTE2 dte)
        {
            _options = options;
            _dte = dte;

            Foreground = Brushes.White;
            Margin = new Thickness(5, 4, 10, 0);
            FontWeight = FontWeights.SemiBold;
            Visibility = Visibility.Collapsed;
        }

        public void SetVisibility(Visibility visibility)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    Visibility = visibility;
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            });
        }
    }
}