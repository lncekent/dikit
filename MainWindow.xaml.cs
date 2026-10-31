using System.ComponentModel;
using System.Windows;
using dikit.ViewModels;

namespace dikit;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (DataContext is NotesManagerViewModel manager && !manager.IsShuttingDown)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        base.OnClosing(e);
    }

    private void ThemeStudio_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is NotesManagerViewModel manager)
        {
            manager.OpenThemeStudio(null);
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not NoteViewModel note)
        {
            return;
        }

        System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show(
            this,
            $"Delete \"{note.Title}\"? A recoverable copy is kept under backups/deleted.",
            "Delete note",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);
        if (result == System.Windows.MessageBoxResult.Yes && DataContext is NotesManagerViewModel manager)
        {
            manager.DeleteNote(note);
        }
    }
}
