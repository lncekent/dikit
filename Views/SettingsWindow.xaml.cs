using System.Diagnostics;
using System.Windows;
using dikit.ViewModels;

namespace dikit.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void OpenDataFolder_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not NotesManagerViewModel manager)
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", $"\"{manager.DataFolderPath}\"")
            {
                UseShellExecute = true,
            });
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception
            or InvalidOperationException or System.IO.FileNotFoundException)
        {
        }
    }
}
