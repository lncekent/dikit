using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
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

    private void NoteCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is NoteViewModel note
            && DataContext is NotesManagerViewModel manager)
        {
            manager.OpenNote(note);
        }
    }

    private void NoteCardMenu_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.ContextMenu is { } menu)
        {
            menu.PlacementTarget = button;
            menu.IsOpen = true;
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        NoteViewModel? note = (sender as System.Windows.Controls.MenuItem)?.CommandParameter as NoteViewModel
            ?? (sender as FrameworkElement)?.DataContext as NoteViewModel;
        if (note is null)
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
