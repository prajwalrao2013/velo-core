using Avalonia.Controls;
using VeloTerminal.ViewModels;

namespace VeloTerminal.Views;

public partial class JournalView : UserControl
{
    public JournalView()
    {
        InitializeComponent();
    }

    private void OnReflectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        if (DataContext is JournalViewModel vm)
        {
            vm.TriggerSave();
        }
    }

    private void OnReflectionChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        if (DataContext is JournalViewModel vm)
        {
            vm.TriggerSave();
        }
    }

}
