using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using VeloTerminal.Services;
using VeloTerminal.Models;

namespace VeloTerminal.ViewModels;

public partial class AnalyticsViewModel : ObservableObject
{
    private readonly JournalService _journalService;
    private readonly AnalyticsEngine _engine;

    [ObservableProperty] private string _winRate = "0%";
    [ObservableProperty] private string _profitFactor = "0.0";
    [ObservableProperty] private string _totalPnL = "₹0.00";

    public ISeries[] EmotionSeries { get; set; }
    public Axis[] XAxes { get; set; }

    public AnalyticsViewModel(JournalService journalService)
    {
        _journalService = journalService;
        _engine = new AnalyticsEngine();

        _journalService.Entries.CollectionChanged += (s, e) => CalculateMetrics();
        
        EmotionSeries = new ISeries[] { new ColumnSeries<double>() };
        XAxes = new Axis[] { new Axis { Labels = new string[] { "Neutral", "Anxious", "FOMO", "Confident", "Revenge" } } };

        CalculateMetrics();
    }

    private void CalculateMetrics()
    {
        var entries = _journalService.Entries.ToList();
        
        WinRate = $"{_engine.CalculateWinRate(entries)}%";
        ProfitFactor = $"{_engine.CalculateProfitFactor(entries)}";
        TotalPnL = $"₹{_engine.CalculateTotalPnL(entries):N2}";

        var heatmap = _engine.CalculateEmotionalHeatmap(entries);
        var vals = new double[] {
            heatmap[EmotionalState.Neutral],
            heatmap[EmotionalState.Anxious],
            heatmap[EmotionalState.FOMO],
            heatmap[EmotionalState.Confident],
            heatmap[EmotionalState.Revenge]
        };

        if (EmotionSeries[0] is ColumnSeries<double> col)
        {
            col.Values = vals;
        }
    }
}
