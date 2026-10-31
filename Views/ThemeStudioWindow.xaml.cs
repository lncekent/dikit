using System.Windows;
using System.Windows.Controls;
using dikit.ViewModels;

namespace dikit.Views;

public partial class ThemeStudioWindow : Window
{
    public ThemeStudioWindow()
    {
        InitializeComponent();
    }

    private void ColorTarget_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { IsChecked: true } radio
            && radio.Tag is string target
            && DataContext is ThemeStudioViewModel studio)
        {
            studio.EditingColorTarget = target;
        }
    }
}
