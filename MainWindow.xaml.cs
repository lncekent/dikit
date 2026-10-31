using System.Windows;
using dikit.ViewModels;

namespace dikit;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
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

        MessageBoxResult result = MessageBox.Show(
            this,
            $"Delete \"{note.Title}\"? A recoverable copy is kept under backups/deleted.",
            "Delete note",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes && DataContext is NotesManagerViewModel manager)
        {
            manager.DeleteNote(note);
        }
    }
}
