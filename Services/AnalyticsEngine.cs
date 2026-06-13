using System;
using System.Collections.Generic;
using System.Linq;
using VeloTerminal.Models;

namespace VeloTerminal.Services;

public class AnalyticsEngine
{
    public double CalculateWinRate(IEnumerable<JournalEntry> entries)
    {
        if (!entries.Any()) return 0;
        double wins = entries.Count(e => MockIsWin(e.Order));
        return Math.Round((wins / entries.Count()) * 100, 2);
    }

    public double CalculateProfitFactor(IEnumerable<JournalEntry> entries)
    {
        double grossProfit = entries.Where(e => MockIsWin(e.Order)).Sum(e => MockPnL(e.Order));
        double grossLoss = Math.Abs(entries.Where(e => !MockIsWin(e.Order)).Sum(e => MockPnL(e.Order)));
        if (grossLoss == 0) return grossProfit > 0 ? 99.9 : 0;
        return Math.Round(grossProfit / grossLoss, 2);
    }

    public double CalculateTotalPnL(IEnumerable<JournalEntry> entries)
    {
        return Math.Round(entries.Sum(e => MockPnL(e.Order)), 2);
    }

    public Dictionary<EmotionalState, double> CalculateEmotionalHeatmap(IEnumerable<JournalEntry> entries)
    {
        var result = new Dictionary<EmotionalState, double>();
        foreach (EmotionalState state in Enum.GetValues(typeof(EmotionalState)))
        {
            var matched = entries.Where(e => e.EmotionalState == state).ToList();
            if (matched.Any())
            {
                result[state] = Math.Round(matched.Average(e => MockPnL(e.Order)), 2);
            }
            else
            {
                result[state] = 0;
            }
        }
        return result;
    }

    // Mock logic: Creates a pseudo-random deterministic P&L mock evaluating the Order ID execution.
    private bool MockIsWin(TradeOrder order) => GetDeterministicMockOffset(order) > 0;
    private double MockPnL(TradeOrder order) => GetDeterministicMockOffset(order) * order.Qty;
    
    private double GetDeterministicMockOffset(TradeOrder order)
    {
        var hash = order.OrderId.GetHashCode();
        bool isWin = (hash % 2) == 0; 
        double spread = (Math.Abs(hash) % 50) + 1;
        return isWin ? spread : -spread;
    }
}
