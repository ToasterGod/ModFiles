using System;
using System.Windows;

namespace ModFilesClient.Helpers
{
    static class ErrorMessage
    {
        public static void ShowError(Exception ex, string additionalText)
        {
            MessageBox.Show($"Error: {ex.HResult}\n\nError message: {ex.Message}\n{additionalText}");
        }
    }
}
