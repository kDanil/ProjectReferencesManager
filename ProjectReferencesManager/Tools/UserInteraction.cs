using System.Windows;

namespace ProjectReferencesManager.Tools
{
    public class UserInteraction
    {
        public void ShowError(string test)
        {
            MessageBox.Show(test, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}