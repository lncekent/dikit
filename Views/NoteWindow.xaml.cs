using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using dikit.Infrastructure;
using dikit.Models;
using dikit.Services;
using dikit.ViewModels;
using Microsoft.Win32;

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

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        try
        {
            if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (Clipboard.ContainsImage())
                {
                    PasteClipboardImage();
                    e.Handled = true;
                    return;
                }

                if (Keyboard.FocusedElement is not TextBox && Clipboard.ContainsText())
                {
                    string text = Clipboard.GetText().Trim();
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
        BitmapSource? image = Clipboard.GetImage();
        if (image is null)
        {
            return;
        }

        if (!_viewModel.AddImageFromBitmap(image))
        {
            MessageBox.Show(
                this,
                "The image could not be saved into this note's assets.",
                "Paste image",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
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
        OpenFileDialog dialog = new OpenFileDialog
        {
            Title = "Add image to note",
            Filter = "Images|*.png;*.jpg;*.jpeg;*.gif;*.bmp|All files|*.*",
            CheckFileExists = true,
            Multiselect = false,
        };

        if (dialog.ShowDialog(this) == true && !_viewModel.AddImageFromFile(dialog.FileName))
        {
            MessageBox.Show(
                this,
                "That file could not be added as an image asset.",
                "Add image",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
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
        if ((sender as MenuItem)?.Parent is ContextMenu menu
            && menu.PlacementTarget is FrameworkElement target
            && target.DataContext is ContentBlock block)
        {
            _viewModel.RemoveBlock(block);
        }
    }

    private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
            this,
            "Delete this note? A recoverable copy is kept under backups/deleted.",
            "Delete note",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            _viewModel.DeleteCommand.Execute(null);
        }
    }
}
