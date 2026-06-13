using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Measure;
using LiveChartsCore.Defaults;
using SkiaSharp;
using VeloTerminal.Models;
using VeloTerminal.Interfaces;
using VeloTerminal.Services;

namespace VeloTerminal.ViewModels;

public partial class MainWindowViewModel : ObservableRecipient, IRecipient<TickDataMessage>, IRecipient<TiltWarningMessage>, IRecipient<DisciplineScoreMessage>
{
    private readonly IMarketDataClient _marketClient;
    private readonly MacroEventService _macroEventService;
    private readonly PsychologyEngine _psychologyEngine;
    private readonly CancellationTokenSource _cts = new();

    [ObservableProperty]
    private TickData _latestTick = new("NIFTY50", 0, 0, 0, DateTime.Now);

    public ObservableCollection<EconomicEvent> Events { get; } = new();

    [ObservableProperty]
    private string _pomodoroText = "25:00";

    [ObservableProperty]
    private bool _isTiltWarningActive = false;
    
    [ObservableProperty]
    private string _tiltWarningText = string.Empty;

    public ScalperViewModel ScalperVM { get; }
    public JournalViewModel JournalVM { get; }
    public AnalyticsViewModel AnalyticsVM { get; }

    [ObservableProperty]
    private int _disciplineScore = 100;

    [ObservableProperty]
    private bool _isScoreCritical = false;

    private int _pomodoroSeconds = 25 * 60;
    private bool _isPomodoroRunning = false;

    public ObservableCollection<FinancialPoint> PricePoints { get; } = new();
    public ISeries[] ChartSeries { get; set; }
    public ObservableCollection<RectangularSection> Sections { get; } = new();

    private DateTime _currentCandleTime;
    private readonly TimeSpan _candleDuration = TimeSpan.FromSeconds(5);

    public MainWindowViewModel()
    {
        var simulator = new MarketSimulatorService();
        _marketClient = simulator; 
        _macroEventService = new MacroEventService(simulator);
        
        // Ensure PsychologyEngine is running silently as an independent service
        _psychologyEngine = new PsychologyEngine(); 
        
        ScalperVM = new ScalperViewModel();

        var journalService = new JournalService();
        JournalVM = new JournalViewModel(journalService);
        AnalyticsVM = new AnalyticsViewModel(journalService);

        ChartSeries = new ISeries[]
        {
            new CandlesticksSeries<FinancialPoint>
            {
                Values = PricePoints,
                UpFill = new SolidColorPaint(SKColors.SpringGreen),
                UpStroke = new SolidColorPaint(SKColors.SpringGreen) { StrokeThickness = 2 },
                DownFill = new SolidColorPaint(SKColors.Red),
                DownStroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 2 }
            }
        };

        IsActive = true; 
        
        _macroEventService.OnEventTriggered += e => Dispatcher.UIThread.InvokeAsync(() => {
            Events.Insert(0, e);
            if (Events.Count > 10) Events.RemoveAt(Events.Count - 1);

            // Chart Annotation Logic
            int idx = PricePoints.Count > 0 ? PricePoints.Count - 1 : 0;
            var color = e.ImpactSeverity == ImpactSeverity.High ? SKColors.Red.WithAlpha(50) : SKColors.Orange.WithAlpha(50);
            Sections.Add(new RectangularSection
            {
                Xi = idx,
                Xj = idx + 1,
                Fill = new SolidColorPaint(color),
                Label = e.Headline,
                LabelSize = 14,
                LabelPaint = new SolidColorPaint(SKColors.Gray)
            });
            if (Sections.Count > 10) Sections.RemoveAt(0);
        });

        _marketClient.StartStreaming(_cts.Token);
        _macroEventService.StartService(_cts.Token);
    }

    public void Receive(TickDataMessage message)
    {
        var tick = message.Value;
        Dispatcher.UIThread.InvokeAsync(() => {
            LatestTick = tick;

            if (PricePoints.Count == 0 || tick.Timestamp - _currentCandleTime >= _candleDuration)
            {
                _currentCandleTime = tick.Timestamp;
                PricePoints.Add(new FinancialPoint(_currentCandleTime, tick.LastPrice, tick.LastPrice, tick.LastPrice, tick.LastPrice));
                if (PricePoints.Count > 100) PricePoints.RemoveAt(0);
            }
            else
            {
                var lastPoint = PricePoints[PricePoints.Count - 1];
                lastPoint.Close = tick.LastPrice;
                if (tick.LastPrice > lastPoint.High) lastPoint.High = tick.LastPrice;
                if (tick.LastPrice < lastPoint.Low) lastPoint.Low = tick.LastPrice;
            }
        });
    }

    public void Receive(TiltWarningMessage message)
    {
        Dispatcher.UIThread.InvokeAsync(() => {
            IsTiltWarningActive = true;
            TiltWarningText = "High frequency detected. Consider a 25-minute Pomodoro reset.";
            StartPomodoro();
        });
    }

    public void Receive(DisciplineScoreMessage message)
    {
        Dispatcher.UIThread.InvokeAsync(() => {
            DisciplineScore = message.Value;
            IsScoreCritical = DisciplineScore < 50;
        });
    }

    private void StartPomodoro()
    {
        if (_isPomodoroRunning) return;
        _isPomodoroRunning = true;
        Task.Run(async () => {
            while (_pomodoroSeconds > 0 && !_cts.IsCancellationRequested)
            {
                await Task.Delay(1000, _cts.Token);
                _pomodoroSeconds--;
                Dispatcher.UIThread.Post(() => {
                    var span = TimeSpan.FromSeconds(_pomodoroSeconds);
                    PomodoroText = span.ToString(@"mm\:ss");
                });
            }
            _isPomodoroRunning = false;
        });
    }

    protected override void OnDeactivated()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        base.OnDeactivated();
    }

    public void Shutdown()
    {
        _cts.Cancel();
        _cts.Dispose();
        IsActive = false;
        ScalperVM.IsActive = false;
    }
}
