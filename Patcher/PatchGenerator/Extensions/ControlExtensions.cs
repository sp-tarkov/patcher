using System.Windows;
using System.Windows.Controls;

namespace PatchGenerator.Extensions
{
    public static class ControlExtensions
    {
        public static void DispatcherSetValue(this ProgressBar pb, int Value)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                pb.Value = Value;
            });
        }

        public static void DispatcherSetIndetermination(this ProgressBar pb, bool Indeterminate)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                pb.IsIndeterminate = Indeterminate;
            });
        }

        public static void DispaatcherSetContent(this ContentControl cc, object content)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                cc.Content = content;
            });
        }

        public static void DispatcherSetText(this TextBlock tb, string Text)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                tb.Text = Text;
            });
        }

        public static void DispatcherSetEnabled(this UIElement uie, bool Enabled)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                uie.IsEnabled = Enabled;
            });
        }
    }
}
