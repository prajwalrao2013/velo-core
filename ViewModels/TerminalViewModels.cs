using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using VeloTerminal.Models;
using VeloTerminal.Services;

namespace VeloTerminal.ViewModels;

// MESSAGES
public class TradeExecutedMessage : ValueChangedMessage<TradeOrder>
{
    public TradeExecutedMessage(TradeOrder value) : base(value) {}
}

public partial class MainViewModel : ObservableObject
{
    public ChartViewModel ChartVM { get; } = new();
    public MarketWatchViewModel MarketWatchVM { get; } = new();
    public SmartMoneyViewModel SmartMoneyVM { get; } = new();
    public MacroCalendarViewModel MacroCalendarVM { get; } = new();
    public SeasonalityViewModel SeasonalityVM { get; } = new();
    public FlowMapViewModel FlowMapVM { get; } = new();
    public CorrelationViewModel CorrelationVM { get; } = new();
    public ExecutionViewModel ExecutionVM { get; } = new();
    public PositionsViewModel PositionsVM { get; } = new();
    public PsychologyViewModel PsychologyVM { get; } = new();
    public NewsTickerViewModel NewsTickerVM { get; } = new();

    private ObservableObject _journalWorkspace;
    private ObservableObject _arenaWorkspace;
    private ObservableObject _pulseWorkspace;

    [ObservableProperty] private ObservableObject _currentWorkspaceView;

    private readonly CancellationTokenSource _cts = new();

    public MainViewModel()
    {
        CurrentWorkspaceView = ChartVM; // default
        _journalWorkspace = new JournalWorkspaceViewModel();
        _arenaWorkspace = new ArenaWorkspaceViewModel();
        _pulseWorkspace = new PulseWorkspaceViewModel();

        var simulator = new MarketSimulatorService();
        simulator.StartStreaming(_cts.Token);
    }

    [RelayCommand]
    private void SwitchWorkspace(string workspace)
    {
        CurrentWorkspaceView = workspace switch {
            "TERMINAL" => ChartVM,
            "JOURNAL" => _journalWorkspace,
            "ARENA" => _arenaWorkspace,
            "PULSE" => _pulseWorkspace,
            _ => ChartVM
        };
    }
}

public partial class JournalWorkspaceViewModel : ObservableObject
{
    public ObservableCollection<JournalEntry> Entries { get; } = new();

    public JournalWorkspaceViewModel()
    {
        WeakReferenceMessenger.Default.Register<TradeExecutedMessage>(this, (r, m) => ((JournalWorkspaceViewModel)r).Receive(m));
        Entries.Add(new JournalEntry(new TradeOrder("ORD001", "NIFTY50", 50, true, 22000.50, DateTime.Now.AddMinutes(-30)), EmotionalState.Confident, "Initial breakout. Clean volume confirmation."));
    }

    public void Receive(TradeExecutedMessage message)
    {
        var order = message.Value;
        Dispatcher.UIThread.InvokeAsync(() => {
            string action = order.IsBuy ? "BUY" : "SELL";
            Entries.Insert(0, new JournalEntry(order, EmotionalState.Neutral, $"Executed {action} {order.Qty}x @ {order.EntryPrice:N2} via Arena Engine."));
        });
    }
}

public class ArenaIdea
{
    public string User { get; set; } = "";
    public string Tag { get; set; } = "";
    public string Body { get; set; } = "";
    public string Pnl { get; set; } = "";
}

public partial class ArenaWorkspaceViewModel : ObservableObject
{
    public ObservableCollection<ArenaIdea> Ideas { get; } = new();

    public ArenaWorkspaceViewModel()
    {
        Ideas.Add(new ArenaIdea { User = "@TraderRex", Tag = "NIFTY · SCALP", Body = "Clean break above 22,400 supply zone. Looking for momentum continuation towards 22,500 over the next hour.", Pnl = "+1.3% P&L" });
        Ideas.Add(new ArenaIdea { User = "@AlphaSeeker", Tag = "RELIANCE · SWING", Body = "Forming a massive multi-week cup and handle pattern. Entering half position here, will add on confirmation.", Pnl = "+0.8% P&L" });
        Ideas.Add(new ArenaIdea { User = "@MacroBear", Tag = "BANKNIFTY · SHORT", Body = "Yields spiking again, RBI hawkish tones. Fading the morning gap up near 48,200 resistance.", Pnl = "+2.1% P&L" });
    }
}

public class PulseAsset
{
    public string Name { get; set; } = "";
    public string Value { get; set; } = "";
    public string Change { get; set; } = "";
    public string ColorHex { get; set; } = "#A1A1AA";
}

public partial class PulseWorkspaceViewModel : ObservableObject
{
    public ObservableCollection<PulseAsset> Assets { get; } = new();

    public PulseWorkspaceViewModel()
    {
        string up = "#22C55E";
        string down = "#EF4444";
        Assets.Add(new PulseAsset { Name = "NIFTY50", Value = "22,388.10", Change = "+0.42%", ColorHex = up });
        Assets.Add(new PulseAsset { Name = "S&P 500", Value = "5,123.69", Change = "+1.10%", ColorHex = up });
        Assets.Add(new PulseAsset { Name = "NASDAQ", Value = "16,210.50", Change = "+1.40%", ColorHex = up });
        Assets.Add(new PulseAsset { Name = "GOLD", Value = "$2,150.12", Change = "+0.20%", ColorHex = up });
        Assets.Add(new PulseAsset { Name = "CRUDE OIL", Value = "$82.50", Change = "-1.10%", ColorHex = down });
        Assets.Add(new PulseAsset { Name = "USD/INR", Value = "82.85", Change = "-0.05%", ColorHex = down });
        Assets.Add(new PulseAsset { Name = "BTC", Value = "$68,400", Change = "+2.40%", ColorHex = up });
        Assets.Add(new PulseAsset { Name = "VIX", Value = "13.20", Change = "-4.10%", ColorHex = down });
        Assets.Add(new PulseAsset { Name = "10Y YIELD", Value = "4.05%", Change = "-0.02%", ColorHex = down });
    }
}

public partial class ChartViewModel : ObservableObject, IRecipient<TickDataMessage>
{
    public ObservableCollection<ISeries> Series { get; set; } = new();
    public ObservableCollection<ISeries> VolumeSeries { get; set; } = new();
    
    public Axis[] XAxes { get; set; }
    public Axis[] VolumeXAxes { get; set; }
    public Axis[] YAxes { get; set; }
    public Axis[] VolumeYAxes { get; set; }
    
    [ObservableProperty] private string _selectedSymbol = "NIFTY50";
    [ObservableProperty] private string _selectedTimeframe = "5m";
    [ObservableProperty] private bool _ghostModeOn = false;

    private ObservableCollection<FinancialPoint> _ghostValues = new();
    private bool _isSyncing = false;

    private static readonly List<(FinancialPoint Candle, double Volume)> _baselineData = GenerateBaselineData();

    private static List<(FinancialPoint Candle, double Volume)> GenerateBaselineData()
    {
        var data = new List<(FinancialPoint, double)>();
        double p = 22000;
        var r = new Random(42);
        // Generate 1-minute data for a few days
        DateTime startTime = DateTime.Today.AddDays(-10);
        for (int i = 0; i < 10000; i++)
        {
            double open = p;
            double close = p + (r.NextDouble() - 0.5) * 15;
            double high = Math.Max(open, close) + r.NextDouble() * 10;
            double low = Math.Min(open, close) - r.NextDouble() * 10;
            
            data.Add((new FinancialPoint(startTime.AddMinutes(i), high, open, close, low), r.Next(100, 1000)));
            p = close;
        }
        return data;
    }

    [RelayCommand]
    private void ChangeTimeframe(string timeframe)
    {
        SelectedTimeframe = timeframe;
        GenerateMockData(timeframe);
    }

    private void GenerateMockData(string timeframe)
    {
        var candles = new ObservableCollection<FinancialPoint>();
        var vols = new ObservableCollection<double>();
        _ghostValues.Clear();

        int groupMinutes = timeframe switch {
            "1m" => 1,
            "5m" => 5,
            "15m" => 15,
            "1h" => 60,
            "D" => 1440,
            _ => 5
        };

        var grouped = _baselineData
            .GroupBy(d => {
                var dt = d.Candle.Date;
                var ticks = dt.Ticks;
                var groupTicks = TimeSpan.FromMinutes(groupMinutes).Ticks;
                return new DateTime(ticks - (ticks % groupTicks));
            })
            .OrderBy(g => g.Key)
            .TakeLast(150); // Keep last 150 candles for the view

        int i = 0;
        foreach (var group in grouped)
        {
            var list = group.ToList();
            double open = list.First().Candle.Open.GetValueOrDefault(0);
            double close = list.Last().Candle.Close.GetValueOrDefault(0);
            double high = list.Max(x => x.Candle.High).GetValueOrDefault(0);
            double low = list.Min(x => x.Candle.Low).GetValueOrDefault(0);
            double vol = list.Sum(x => x.Volume);

            candles.Add(new FinancialPoint(group.Key, high, open, close, low));
            
            double ghostY = open + (Math.Sin(i * 0.2) * 80);
            _ghostValues.Add(new FinancialPoint(group.Key, ghostY+20, ghostY-20, ghostY, ghostY));

            vols.Add(vol);
            i++;
        }

        int stepSeconds = groupMinutes * 60;

        if (Series != null && Series.Count > 0)
        {
            var mainSeries = (CandlesticksSeries<FinancialPoint>)Series[0];
            mainSeries.Values = candles;

            var volSeries = (ColumnSeries<double>)VolumeSeries[0];
            volSeries.Values = vols;
            
            if (Series.Count > 1) {
                Series[1].Values = _ghostValues;
            }
            
            if (XAxes != null && XAxes.Length > 0 && XAxes[0] is DateTimeAxis dtX)
            {
                dtX.UnitWidth = TimeSpan.FromSeconds(stepSeconds).Ticks;
                dtX.MinStep = TimeSpan.FromSeconds(stepSeconds * 6).Ticks;
                dtX.MinLimit = null;
                dtX.MaxLimit = null;
            }
            
            if (VolumeXAxes != null && VolumeXAxes.Length > 0 && VolumeXAxes[0] is DateTimeAxis vDtX)
            {
                vDtX.UnitWidth = TimeSpan.FromSeconds(stepSeconds).Ticks;
                vDtX.MinStep = TimeSpan.FromSeconds(stepSeconds * 6).Ticks;
                vDtX.MinLimit = null;
                vDtX.MaxLimit = null;
            }
        }
        else
        {
            Series = new ObservableCollection<ISeries> {
                new CandlesticksSeries<FinancialPoint> {
                    Values = candles,
                    UpFill = new SolidColorPaint(SKColor.Parse("#22C55E")),
                    UpStroke = new SolidColorPaint(SKColor.Parse("#22C55E")) { StrokeThickness = 2 },
                    DownFill = new SolidColorPaint(SKColor.Parse("#EF4444")),
                    DownStroke = new SolidColorPaint(SKColor.Parse("#EF4444")) { StrokeThickness = 2 },
                    AnimationsSpeed = TimeSpan.FromMilliseconds(50)
                }
            };

            VolumeSeries = new ObservableCollection<ISeries> {
                new ColumnSeries<double> {
                    Values = vols,
                    Fill = new SolidColorPaint(SKColor.Parse("#3F3F46"))
                }
            };
        }
    }

    public ChartViewModel()
    {
        WeakReferenceMessenger.Default.Register(this);
        GenerateMockData("5m");

        // Phase 6 Chart Axes Formatting & Sync
        var mainX = new DateTimeAxis(TimeSpan.FromSeconds(5), date => date.ToString("HH:mm:ss"))
        {
            MinStep = TimeSpan.FromSeconds(30).Ticks, // Spaced out
            TextSize = 11,
            Padding = new LiveChartsCore.Drawing.Padding(0, 10, 0, 0),
            LabelsPaint = new SolidColorPaint(SKColor.Parse("#A1A1AA"))
        };
        var volX = new DateTimeAxis(TimeSpan.FromSeconds(5), date => "")
        {
            IsVisible = false // Hide volume labels
        };
        
        mainX.PropertyChanged += (s, e) => {
            if (_isSyncing) return;
            if (e.PropertyName == nameof(Axis.MinLimit) || e.PropertyName == nameof(Axis.MaxLimit)) {
                _isSyncing = true;
                volX.MinLimit = mainX.MinLimit;
                volX.MaxLimit = mainX.MaxLimit;
                _isSyncing = false;
            }
        };
        volX.PropertyChanged += (s, e) => {
            if (_isSyncing) return;
            if (e.PropertyName == nameof(Axis.MinLimit) || e.PropertyName == nameof(Axis.MaxLimit)) {
                _isSyncing = true;
                mainX.MinLimit = volX.MinLimit;
                mainX.MaxLimit = volX.MaxLimit;
                _isSyncing = false;
            }
        };

        XAxes = new[] { mainX };
        VolumeXAxes = new[] { volX };

        YAxes = new[] { new Axis { TextSize = 11, LabelsPaint = new SolidColorPaint(SKColor.Parse("#A1A1AA")) } };
        VolumeYAxes = new[] { new Axis { TextSize = 9, LabelsPaint = new SolidColorPaint(SKColor.Parse("#52525B")) } };
    }

    partial void OnGhostModeOnChanged(bool value)
    {
        var mainSeries = (CandlesticksSeries<FinancialPoint>)Series[0];
        if (value)
        {
            mainSeries.UpFill = new SolidColorPaint(SKColor.Parse("#6622C55E"));
            mainSeries.UpStroke = new SolidColorPaint(SKColor.Parse("#6622C55E")) { StrokeThickness = 2 };
            mainSeries.DownFill = new SolidColorPaint(SKColor.Parse("#66EF4444"));
            mainSeries.DownStroke = new SolidColorPaint(SKColor.Parse("#66EF4444")) { StrokeThickness = 2 };

            Series.Add(new LineSeries<FinancialPoint> {
                Values = _ghostValues,
                GeometryFill = null,
                GeometryStroke = null,
                Stroke = new SolidColorPaint(SKColor.Parse("#A855F7")) { 
                    StrokeThickness = 2, 
                    PathEffect = new DashEffect(new float[] { 5, 5 }) 
                }
            });
        }
        else
        {
            mainSeries.UpFill = new SolidColorPaint(SKColor.Parse("#22C55E"));
            mainSeries.UpStroke = new SolidColorPaint(SKColor.Parse("#22C55E")) { StrokeThickness = 2 };
            mainSeries.DownFill = new SolidColorPaint(SKColor.Parse("#EF4444"));
            mainSeries.DownStroke = new SolidColorPaint(SKColor.Parse("#EF4444")) { StrokeThickness = 2 };

            if (Series.Count > 1) Series.RemoveAt(1);
        }
    }

    public void Receive(TickDataMessage message)
    {
        var tick = message.Value;
        if (tick.Symbol == SelectedSymbol)
        {
            Dispatcher.UIThread.InvokeAsync(() => {
                var coll = Series[0].Values as ObservableCollection<FinancialPoint>;
                if (coll == null || coll.Count == 0) return;
                
                var last = coll[coll.Count - 1];
                if (last == null) return;
                
                double high = Math.Max(last.High ?? tick.LastPrice, tick.LastPrice);
                double low = Math.Min(last.Low ?? tick.LastPrice, tick.LastPrice);
                
                coll[coll.Count - 1] = new FinancialPoint(last.Date, high, last.Open ?? tick.LastPrice, tick.LastPrice, low);
            });
        }
    }
}

public partial class MarketWatchViewModel : ObservableObject, IRecipient<TickDataMessage>
{
    public ObservableCollection<Instrument> Instruments { get; } = new();

    public MarketWatchViewModel()
    {
        WeakReferenceMessenger.Default.Register(this);

        Instruments.Add(new Instrument("NIFTY50", "NSE · INDEX", 22000.00, 22001.00, GenerateSparkline(), 18000, 23000));
        Instruments.Add(new Instrument("BANKNIFTY", "NSE · INDEX", 48050.00, 48051.50, GenerateSparkline(), 42000, 49000));
        Instruments.Add(new Instrument("FINNIFTY", "NSE · INDEX", 21200.00, 21201.00, GenerateSparkline(), 19000, 22000));
        Instruments.Add(new Instrument("SENSEX", "BSE · INDEX", 73500.00, 73510.00, GenerateSparkline(), 65000, 75000));
        Instruments.Add(new Instrument("MIDCAP150", "NSE · INDEX", 18200.00, 18202.00, GenerateSparkline(), 12000, 19000));
    }

    private double[] GenerateSparkline()
    {
        var r = new Random();
        var pts = new double[20];
        double p = 100;
        for(int i=0; i<20; i++){ p += (r.NextDouble()-0.5)*2; pts[i]=p; }
        return pts;
    }

    public void Receive(TickDataMessage message)
    {
        var tick = message.Value;
        var instrument = Instruments.FirstOrDefault(i => i.Symbol == tick.Symbol);
        if (instrument != null)
        {
            Dispatcher.UIThread.InvokeAsync(() => {
                var index = Instruments.IndexOf(instrument);
                Instruments[index] = instrument with { Bid = tick.Bid, Ask = tick.Ask };
            });
        }
    }
}
public record Instrument(string Symbol, string Subtext, double Bid, double Ask, double[] Sparkline, double Low52, double High52);

public partial class SmartMoneyViewModel : ObservableObject
{
    [ObservableProperty] private string _fiiNet = "+₹2,340 Cr";
    [ObservableProperty] private string _diiNet = "-₹890 Cr";
    [ObservableProperty] private string _maxPain = "22,400";
    [ObservableProperty] private string _maxPainChange = "↑ 12pts";
    [ObservableProperty] private string _pcr = "0.82";
    [ObservableProperty] private string _oiChange = "+1.2L";
}

public partial class MacroCalendarViewModel : ObservableObject
{
    public ObservableCollection<MacroEvent> Events { get; } = new();
    public MacroCalendarViewModel()
    {
        Events.Add(new MacroEvent("09:00", "RBI Policy", "HIGH", "in 2h 14m"));
        Events.Add(new MacroEvent("11:30", "IIP Data", "MED", "in 4h 44m"));
        Events.Add(new MacroEvent("14:00", "US CPI", "HIGH", "in 7h 14m"));
    }
}
public record MacroEvent(string Time, string Name, string Impact, string Countdown);

public partial class ExecutionViewModel : ObservableObject, IRecipient<TickDataMessage>
{
    [ObservableProperty] private string _symbol = "NIFTY50";
    [ObservableProperty] private string _ltp = "22,000.50";
    [ObservableProperty] private string _change = "+0.42%";
    
    // Order Types
    [ObservableProperty] private bool _isMkt = true;
    [ObservableProperty] private bool _isLmt = false;
    [ObservableProperty] private bool _isSl = false;
    [ObservableProperty] private bool _isSlm = false;

    [ObservableProperty] private string _qty = "50";
    [ObservableProperty] private double _targetPrice = 22100;
    [ObservableProperty] private double _stopLossPrice = 21950;
    
    [ObservableProperty] private double _currentPrice = 22000.50;

    [ObservableProperty] private string _marginRequired = "₹89,556.48";
    
    // Pre-Trade Gate
    [ObservableProperty] private bool _isTradeGateActive = false;
    [ObservableProperty] private string _gateMessage = "";

    // RR Bar
    public double RiskWidth => Math.Max(0, CurrentPrice - StopLossPrice);
    public double RewardWidth => Math.Max(0, TargetPrice - CurrentPrice);

    public ExecutionViewModel()
    {
        WeakReferenceMessenger.Default.Register(this);
        PropertyChanged += (s,e) => {
            if (e.PropertyName == nameof(CurrentPrice) || e.PropertyName == nameof(StopLossPrice) || e.PropertyName == nameof(TargetPrice))
            {
                OnPropertyChanged(nameof(RiskWidth));
                OnPropertyChanged(nameof(RewardWidth));
            }
        };
    }

    [RelayCommand]
    private async Task ExecuteTradeAsync(string action)
    {
        GateMessage = $"CONFIRM {action} — ADHERE TO DISCIPLINE";
        IsTradeGateActive = true;
        await Task.Delay(1000);
        IsTradeGateActive = false;
        
        bool isBuy = action == "BUY";
        var order = new TradeOrder(Guid.NewGuid().ToString("N").Substring(0, 8), Symbol, int.Parse(Qty), isBuy, CurrentPrice, DateTime.Now);
        WeakReferenceMessenger.Default.Send(new TradeExecutedMessage(order));
    }

    public void Receive(TickDataMessage message)
    {
        var tick = message.Value;
        if (tick.Symbol == Symbol)
        {
            Dispatcher.UIThread.InvokeAsync(() => {
                Ltp = tick.LastPrice.ToString("N2");
                CurrentPrice = tick.LastPrice;
            });
        }
    }
}

public partial class PositionsViewModel : ObservableObject, IRecipient<TickDataMessage>
{
    public ObservableCollection<Position> OpenPositions { get; } = new();
    [ObservableProperty] private string _totalPnL = "+₹10,400";
    
    public PositionsViewModel()
    {
        WeakReferenceMessenger.Default.Register<TickDataMessage>(this, (r,m) => ((PositionsViewModel)r).Receive(m));
        WeakReferenceMessenger.Default.Register<TradeExecutedMessage>(this, (r,m) => ((PositionsViewModel)r).Receive(m));
    }

    public void Receive(TickDataMessage message)
    {
        var tick = message.Value;
        var pos = OpenPositions.FirstOrDefault(p => p.Symbol == tick.Symbol);
        if (pos != null)
        {
            Dispatcher.UIThread.InvokeAsync(() => {
                var index = OpenPositions.IndexOf(pos);
                double entry = double.Parse(pos.Entry.Replace(",", ""));
                double pnl = (tick.LastPrice - entry) * pos.Qty;
                if (pos.Type == "S") pnl = -pnl;
                
                OpenPositions[index] = pos with { 
                    Ltp = tick.LastPrice.ToString("N2"),
                    PnlAmount = (pnl >= 0 ? "+₹" : "-₹") + Math.Abs(pnl).ToString("N0")
                };
            });
        }
    }

    public void Receive(TradeExecutedMessage message)
    {
        var order = message.Value;
        Dispatcher.UIThread.InvokeAsync(() => {
            OpenPositions.Add(new Position(order.Symbol, order.IsBuy ? "L" : "S", order.Qty, order.EntryPrice.ToString("N2"), order.EntryPrice.ToString("N2"), "+₹0", "0.00%"));
        });
    }
}
public record Position(string Symbol, string Type, int Qty, string Entry, string Ltp, string PnlAmount, string PnlPct);

public partial class PsychologyViewModel : ObservableObject
{
    [ObservableProperty] private int _disciplineScore = 78;
    [ObservableProperty] private string _streakDays = "🔥 7-day rule-following streak";
    [ObservableProperty] private string _currentMood = "😐 Neutral — logged against trades";
    [ObservableProperty] private string _pomodoroTime = "25:00";
    [ObservableProperty] private double _pomodoroProgress = 1.0;

    private DispatcherTimer _timer;
    private int _remainingSeconds = 25 * 60;

    public ObservableCollection<Habit> Habits { get; } = new();

    public PsychologyViewModel()
    {
        Habits.Add(new Habit(true, "Pre-market brief done", ""));
        Habits.Add(new Habit(true, "Position size rule followed", ""));
        Habits.Add(new Habit(false, "Max 5 trades/day", "3/5 used"));
        Habits.Add(new Habit(false, "No trading after 2pm", ""));

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (s, e) => {
            if (_remainingSeconds > 0) _remainingSeconds--;
            PomodoroTime = $"{_remainingSeconds / 60:D2}:{_remainingSeconds % 60:D2}";
            PomodoroProgress = _remainingSeconds / (25.0 * 60.0);
        };
        _timer.Start();
    }
}
public record Habit(bool IsChecked, string Text, string Subtext);

public partial class NewsTickerViewModel : ObservableObject
{
    public ObservableCollection<TickerItem> Items { get; } = new();
    
    public NewsTickerViewModel()
    {
        Items.Add(new TickerItem("ECO", "Unemployment figures dropped to 3.8% — lowest since 2020"));
        Items.Add(new TickerItem("GEO", "Unexpected geopolitical tensions in Eastern Europe escalate"));
        Items.Add(new TickerItem("ECO", "RBI Rate decision: Hold at 6.5% — third consecutive pause"));
        Items.Add(new TickerItem("MKT", "Global markets showing broad risk-on sentiment"));
        Items.Add(new TickerItem("ECO", "India CPI at 4.2% — within RBI comfort band"));
        Items.Add(new TickerItem("MKT", "FII net buyers for 4th straight session — ₹2,340 Cr inflow"));
        Items.Add(new TickerItem("GEO", "Oil supply disruption fears push Brent above $92"));
        Items.Add(new TickerItem("ECO", "US Non-Farm Payrolls beat expectations at 287K vs 230K forecast"));
    }
}
public record TickerItem(string Category, string Text);

public partial class SeasonalityViewModel : ObservableObject { }
public partial class FlowMapViewModel : ObservableObject { }
public partial class CorrelationViewModel : ObservableObject { }
