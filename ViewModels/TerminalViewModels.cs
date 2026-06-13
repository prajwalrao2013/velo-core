using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

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
}

public partial class ChartViewModel : ObservableObject
{
    public ObservableCollection<ISeries> Series { get; set; }
    public ObservableCollection<ISeries> VolumeSeries { get; set; }
    
    [ObservableProperty] private string _selectedSymbol = "NIFTY50";
    [ObservableProperty] private string _selectedTimeframe = "30s";
    [ObservableProperty] private bool _ghostModeOn = false;

    public ChartViewModel()
    {
        // Generate mock candles
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
            vols.Add(r.Next(1000, 5000));
            p = close;
        }

        Series = new ObservableCollection<ISeries> {
            new CandlesticksSeries<FinancialPoint> {
                Values = candles,
                UpFill = new SolidColorPaint(SKColor.Parse("#22C55E")),
                UpStroke = new SolidColorPaint(SKColor.Parse("#22C55E")) { StrokeThickness = 2 },
                DownFill = new SolidColorPaint(SKColor.Parse("#EF4444")),
                DownStroke = new SolidColorPaint(SKColor.Parse("#EF4444")) { StrokeThickness = 2 }
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

public partial class MarketWatchViewModel : ObservableObject
{
    public ObservableCollection<Instrument> Instruments { get; } = new();

    public MarketWatchViewModel()
    {
        Instruments.Add(new Instrument("NIFTY50", "NSE · INDEX", 22388.12, 22389.12, GenerateSparkline()));
        Instruments.Add(new Instrument("BANKNIFTY", "NSE · INDEX", 48050.00, 48051.50, GenerateSparkline()));
        Instruments.Add(new Instrument("FINNIFTY", "NSE · INDEX", 21200.00, 21201.00, GenerateSparkline()));
        Instruments.Add(new Instrument("SENSEX", "BSE · INDEX", 73500.00, 73510.00, GenerateSparkline()));
        Instruments.Add(new Instrument("MIDCAP150", "NSE · INDEX", 18200.00, 18202.00, GenerateSparkline()));
    }

    private double[] GenerateSparkline()
    {
        var r = new Random();
        var pts = new double[20];
        double p = 100;
        for(int i=0; i<20; i++){ p += (r.NextDouble()-0.5)*2; pts[i]=p; }
        return pts;
    }
}
public record Instrument(string Symbol, string Subtext, double Bid, double Ask, double[] Sparkline);

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

public partial class ExecutionViewModel : ObservableObject
{
    [ObservableProperty] private string _symbol = "NIFTY50";
    [ObservableProperty] private string _ltp = "22,388.12";
    [ObservableProperty] private string _change = "+0.42%";
    [ObservableProperty] private string _orderType = "MKT";
    [ObservableProperty] private string _qty = "1";
    [ObservableProperty] private string _stopLoss = "22350";
    [ObservableProperty] private string _target = "22450";
    [ObservableProperty] private string _marginRequired = "₹89,556.48";
    [ObservableProperty] private string _riskReward = "RR · 1 : 2.4";
}

public partial class PositionsViewModel : ObservableObject
{
    public ObservableCollection<Position> OpenPositions { get; } = new();
    [ObservableProperty] private string _totalPnL = "+₹10,400";
    
    public PositionsViewModel()
    {
        OpenPositions.Add(new Position("NIFTY50", "L", 1, "22,250", "22,388", "+₹6,900", "+0.62%"));
        OpenPositions.Add(new Position("BANKNIFTY", "S", 1, "48,120", "48,050", "+₹3,500", "+0.14%"));
    }
}
public record Position(string Symbol, string Type, int Qty, string Entry, string Ltp, string PnlAmount, string PnlPct);

public partial class PsychologyViewModel : ObservableObject
{
    [ObservableProperty] private int _disciplineScore = 78;
    [ObservableProperty] private string _streakDays = "🔥 7-day rule-following streak";
    [ObservableProperty] private string _currentMood = "😐 Neutral — logged against trades";
    [ObservableProperty] private string _pomodoroTime = "21:14";
    [ObservableProperty] private double _pomodoroProgress = 0.85;

    public ObservableCollection<Habit> Habits { get; } = new();

    public PsychologyViewModel()
    {
        Habits.Add(new Habit(true, "Pre-market brief done", ""));
        Habits.Add(new Habit(true, "Position size rule followed", ""));
        Habits.Add(new Habit(false, "Max 5 trades/day", "3/5 used"));
        Habits.Add(new Habit(false, "No trading after 2pm", ""));
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
