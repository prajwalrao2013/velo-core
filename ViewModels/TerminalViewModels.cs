using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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

    [ObservableProperty] private ObservableObject _currentWorkspaceView;

    private readonly CancellationTokenSource _cts = new();

    public MainViewModel()
    {
        CurrentWorkspaceView = ChartVM; // default
        var simulator = new MarketSimulatorService();
        simulator.StartStreaming(_cts.Token);
    }

    [RelayCommand]
    private void SwitchWorkspace(string workspace)
    {
        CurrentWorkspaceView = workspace switch {
            "TERMINAL" => ChartVM,
            "JOURNAL" => new JournalWorkspaceViewModel(),
            "ARENA" => new ArenaWorkspaceViewModel(),
            "PULSE" => new PulseWorkspaceViewModel(),
            _ => ChartVM
        };
    }
}

public class JournalWorkspaceViewModel : ObservableObject { }
public class ArenaWorkspaceViewModel : ObservableObject { }
public class PulseWorkspaceViewModel : ObservableObject { }

public partial class ChartViewModel : ObservableObject, IRecipient<TickDataMessage>
{
    public ObservableCollection<ISeries> Series { get; set; }
    public ObservableCollection<ISeries> VolumeSeries { get; set; }
    
    [ObservableProperty] private string _selectedSymbol = "NIFTY50";
    [ObservableProperty] private string _selectedTimeframe = "5m";
    [ObservableProperty] private bool _ghostModeOn = false;

    private ObservableCollection<FinancialPoint> _ghostValues = new();

    public ChartViewModel()
    {
        WeakReferenceMessenger.Default.Register(this);

        var candles = new ObservableCollection<FinancialPoint>();
        var vols = new ObservableCollection<double>();
        double p = 22000;
        var r = new Random(42);
        for(int i=0; i<120; i++) {
            double open = p;
            double close = p + (r.NextDouble() - 0.5) * 60;
            double high = Math.Max(open, close) + r.NextDouble() * 20;
            double low = Math.Min(open, close) - r.NextDouble() * 20;
            candles.Add(new FinancialPoint(DateTime.Now.AddSeconds(i * 5), high, open, close, low));
            
            // Generate historical ghost mode path
            _ghostValues.Add(new FinancialPoint(DateTime.Now.AddSeconds(i * 5), high+200, open+200, close+200, low+200));

            vols.Add(r.Next(1000, 5000));
            p = close;
        }

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

        Instruments.Add(new Instrument("NIFTY50", "NSE · INDEX", 22000.00, 22001.00, GenerateSparkline(), 21000, 23000));
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

public partial class SeasonalityViewModel : ObservableObject
{
    public ObservableCollection<SeasonalityRow> Rows { get; } = new();
    public SeasonalityViewModel()
    {
        var r = new Random(1);
        for(int year = 2015; year <= 2025; year++)
        {
            var row = new SeasonalityRow { Year = year.ToString() };
            for(int m=0; m<12; m++) row.Months[m] = (r.NextDouble()-0.5)*8;
            Rows.Add(row);
        }
    }
}
public class SeasonalityRow 
{
    public string Year { get; set; } = "";
    public double[] Months { get; set; } = new double[12];
}

public partial class FlowMapViewModel : ObservableObject
{
    public ObservableCollection<FlowSector> Sectors { get; } = new();
    public FlowMapViewModel()
    {
        Sectors.Add(new FlowSector("BANK", 220, 1.4));
        Sectors.Add(new FlowSector("IT", 180, -0.8));
        Sectors.Add(new FlowSector("FMCG", 160, 0.5));
        Sectors.Add(new FlowSector("ENERGY", 140, 2.1));
        Sectors.Add(new FlowSector("PHARMA", 120, -0.2));
        Sectors.Add(new FlowSector("REALTY", 100, 3.4));
    }
}
public record FlowSector(string Name, double Size, double Return);

public partial class CorrelationViewModel : ObservableObject
{
    public ObservableCollection<ObservableCollection<double>> Matrix { get; } = new();
    public CorrelationViewModel()
    {
        double[,] data = {
            { 1.00, 0.87, -0.12, 0.34, -0.45 },
            { 0.87, 1.00, -0.18, 0.28, -0.50 },
            { -0.12, -0.18, 1.00, 0.22, -0.67 },
            { 0.34, 0.28, 0.22, 1.00, -0.31 },
            { -0.45, -0.50, -0.67, -0.31, 1.00 }
        };
        for(int i=0; i<5; i++){
            var row = new ObservableCollection<double>();
            for(int j=0; j<5; j++) row.Add(data[i,j]);
            Matrix.Add(row);
        }
    }
}

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
        // Proceed with simulated or real trade
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
        WeakReferenceMessenger.Default.Register(this);
        OpenPositions.Add(new Position("NIFTY50", "L", 50, "21900.00", "22000.50", "+₹5,025", "+0.46%"));
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
