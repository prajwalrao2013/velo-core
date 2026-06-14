using System;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace VeloTerminal.Models;

public class TickDataMessage : ValueChangedMessage<TickData>
{
    public TickDataMessage(TickData value) : base(value) {}
}

public class TradeExecutedMessage : ValueChangedMessage<TradeOrder>
{
    public TradeExecutedMessage(TradeOrder value) : base(value) {}
}

public record TickData(string Symbol, double LastPrice, double Bid, double Ask, DateTime Timestamp);

public record TradeOrder(string OrderId, string Symbol, int Qty, bool IsBuy, double EntryPrice, DateTime Timestamp);

public enum EmotionalState
{
    Neutral,
    Anxious,
    FOMO,
    Confident,
    Revenge
}

public class JournalEntry
{
    public TradeOrder Order { get; set; }
    public EmotionalState EmotionalState { get; set; }
    public string Notes { get; set; }

    public JournalEntry(TradeOrder order, EmotionalState emotionalState = EmotionalState.Neutral, string notes = "")
    {
        Order = order;
        EmotionalState = emotionalState;
        Notes = notes;
    }
}

public enum ImpactSeverity
{
    Low,
    High
}

public record EconomicEvent(string Headline, ImpactSeverity ImpactSeverity, DateTime ExpirationTime);
