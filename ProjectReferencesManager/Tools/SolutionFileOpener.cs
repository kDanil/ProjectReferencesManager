using Microsoft.Win32;

namespace ProjectReferencesManager.Tools
{
    public interface ISolutionFileOpener
    {
        string Open();
    }

    public class SolutionFileOpener  : ISolutionFileOpener
    {
        public string Open()
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.FileName = "*.sln";

            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }

            return string.Empty;
        }
    }
}