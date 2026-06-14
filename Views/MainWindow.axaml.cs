using Avalonia.Controls;
using Avalonia.Input;
using LiveChartsCore.SkiaSharpView.Avalonia;
using VeloTerminal.ViewModels;

namespace VeloTerminal.Views;

public partial class MainWindow : Window
{
    private bool _isDraggingLine = false;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Chart_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (sender is CartesianChart chart && DataContext is MainViewModel vm)
        {
            var chartVm = vm.CurrentWorkspaceView as ChartViewModel;
            if (chartVm != null && chartVm.IsDrawingModeOn)
            {
                var p = e.GetPosition(chart);
                var dataPoint = chart.ScalePixelsToData(new LiveChartsCore.Drawing.LvcPointD(p.X, p.Y));
                chartVm.StartDrawing(dataPoint.X, dataPoint.Y);
                _isDraggingLine = true;
                e.Handled = true;
            }
        }
    }

    private void Chart_PointerMoved(object sender, PointerEventArgs e)
    {
        if (_isDraggingLine && sender is CartesianChart chart && DataContext is MainViewModel vm)
        {
            var chartVm = vm.CurrentWorkspaceView as ChartViewModel;
            if (chartVm != null && chartVm.IsDrawingModeOn)
            {
                var p = e.GetPosition(chart);
                var dataPoint = chart.ScalePixelsToData(new LiveChartsCore.Drawing.LvcPointD(p.X, p.Y));
                chartVm.UpdateDrawing(dataPoint.X, dataPoint.Y);
                e.Handled = true;
            }
        }
    }

    private void Chart_PointerReleased(object sender, PointerReleasedEventArgs e)
    {
        if (_isDraggingLine && sender is CartesianChart chart && DataContext is MainViewModel vm)
        {
            var chartVm = vm.CurrentWorkspaceView as ChartViewModel;
            if (chartVm != null)
            {
                chartVm.EndDrawing();
            }
            _isDraggingLine = false;
        }
    }

    protected override void OnClosed(System.EventArgs e)
    {
        base.OnClosed(e);
    }
}
