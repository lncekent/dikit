using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using dikit.Infrastructure;
using dikit.Models;
using dikit.Services;
using dikit.ViewModels;

namespace dikit.Views;

public partial class NoteWindow : Window
{
    private readonly NoteViewModel _viewModel;
    private bool _placementReady;

    public NoteWindow(NoteViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        DataContext = viewModel;
        WindowGeometry.ApplyTo(this, viewModel.Model.Window);
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        _placementReady = true;
        SyncPlacement();
    }

    protected override void OnLocationChanged(EventArgs e)
    {
        base.OnLocationChanged(e);
        SyncPlacement();
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        SyncPlacement();
    }

    protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        try
        {
            if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (System.Windows.Clipboard.ContainsImage())
                {
                    PasteClipboardImage();
                    e.Handled = true;
                    return;
                }

                if (Keyboard.FocusedElement is not System.Windows.Controls.TextBox
                    && System.Windows.Clipboard.ContainsText())
                {
                    string text = System.Windows.Clipboard.GetText().Trim();
                    if (LinkLauncher.IsValidHttpUri(text))
                    {
                        _viewModel.AddLinkFromUrl(text);
                        e.Handled = true;
                        return;
                    }
                }
            }
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
        }

        base.OnPreviewKeyDown(e);
    }

    private void SyncPlacement()
    {
        if (!_placementReady || WindowState != WindowState.Normal)
        {
            return;
        }

        WindowGeometry.Capture(this, _viewModel.Model.Window);
    }

    private void PasteClipboardImage()
    {
        BitmapSource? image = System.Windows.Clipboard.GetImage();
        if (image is null)
        {
            return;
        }

        if (!_viewModel.AddImageFromBitmap(image))
        {
            System.Windows.MessageBox.Show(
                this,
                "The image could not be saved into this note's assets.",
                "Paste image",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void AddImage_Click(object sender, RoutedEventArgs e)
    {
        Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Add image to note",
            Filter = "Images|*.png;*.jpg;*.jpeg;*.gif;*.bmp|All files|*.*",
            CheckFileExists = true,
            Multiselect = false,
        };

        if (dialog.ShowDialog(this) == true && !_viewModel.AddImageFromFile(dialog.FileName))
        {
            System.Windows.MessageBox.Show(
                this,
                "That file could not be added as an image asset.",
                "Add image",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
        }
    }

    private void MenuButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.ContextMenu is { } menu)
        {
            menu.PlacementTarget = button;
            menu.IsOpen = true;
        }
    }

    private void RemoveBlockMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as System.Windows.Controls.MenuItem)?.Parent
                is System.Windows.Controls.ContextMenu menu
            && menu.PlacementTarget is FrameworkElement target
            && target.DataContext is ContentBlock block)
        {
            _viewModel.RemoveBlock(block);
        }
    }

    private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show(
            this,
            "Delete this note? A recoverable copy is kept under backups/deleted.",
            "Delete note",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);
        if (result == System.Windows.MessageBoxResult.Yes)
        {
            _viewModel.DeleteCommand.Execute(null);
        }
    }
}
