using Avalonia.Controls;
using Avalonia.Interactivity;

namespace NetCloud.Client.Avalonia;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void SetupButton_Click(object? sender, RoutedEventArgs e)
    {
        var setupWindow = new SetupWindow();
        setupWindow.Show();
    }
}