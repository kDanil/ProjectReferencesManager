using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace ProjectReferencesManager.Tools
{
    public class BindingErrorListener : DefaultTraceListener
    {
        private static BindingErrorListener listener;
        private static bool isSet = false;

        private StringBuilder message = new StringBuilder();

        public static void CloseTrace()
        {
            if (listener == null)
            {
                return;
            }

            listener.Flush();
            listener.Close();
            PresentationTraceSources.DataBindingSource.Listeners.Remove(listener);
            listener = null;

            isSet = false;
        }

        public static void SetTrace()
        {
            if (!isSet)
            {
                SetTrace(SourceLevels.Error, TraceOptions.None);

                isSet = true;
            }
        }

        public static void SetTrace(SourceLevels level, TraceOptions options)
        {
            if (listener == null)
            {
                listener = new BindingErrorListener();
                PresentationTraceSources.DataBindingSource.Listeners.Add(listener);
            }

            listener.TraceOutputOptions = options;
            PresentationTraceSources.DataBindingSource.Switch.Level = level;
        }

        public override void Write(string message)
        {
            this.message.Append(message);
        }

        public override void WriteLine(string message)
        {
            this.message.Append(message);

            var final = this.message.ToString();
            this.message = new StringBuilder();
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                MessageBox.Show(final, "Binding Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }), DispatcherPriority.Background);
        }
    }
}