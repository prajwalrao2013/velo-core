using Avalonia.Controls;
using VeloTerminal.ViewModels;

namespace VeloTerminal.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosed(System.EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.Shutdown();
        }
        base.OnClosed(e);
    }
}
