using System.Windows;

namespace ProjectReferencesManager.Tools
{
    public interface IUserInteraction
    {
        void ShowError(string test);
    }

    public class UserInteraction : IUserInteraction
    {
        public void ShowError(string test)
        {
            MessageBox.Show(test, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}