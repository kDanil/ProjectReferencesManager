using System;
using System.Windows;

namespace ProjectReferencesManager.Tools
{
    public interface IUserInteraction
    {
        void ShowError(string test);
        bool Ask(string question);
    }

    public class UserInteraction : IUserInteraction
    {
        public bool Ask(string question)
        {
            return MessageBox.Show(question, "Question", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public void ShowError(string test)
        {
            MessageBox.Show(test, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}