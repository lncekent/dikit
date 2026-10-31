using System.Windows;
using System.Windows.Input;
using dikit.Infrastructure;
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

    private void SyncPlacement()
    {
        if (!_placementReady || WindowState != WindowState.Normal)
        {
            return;
        }

        WindowGeometry.Capture(this, _viewModel.Model.Window);
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

    private void MenuButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.ContextMenu is { } menu)
        {
            menu.PlacementTarget = button;
            menu.IsOpen = true;
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
