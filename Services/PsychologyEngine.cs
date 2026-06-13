using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using VeloTerminal.Models;

namespace VeloTerminal.Services;

public class PsychologyEngine : IRecipient<TradeOrderMessage>, IRecipient<TiltWarningMessage>, IRecipient<JournalEntrySavedMessage>
{
    private readonly List<TradeInfo> _tradeHistory = new();
    private record TradeInfo(DateTime Timestamp, bool IsLoss);

    public int DisciplineScore { get; private set; } = 100;

    public PsychologyEngine()
    {
        WeakReferenceMessenger.Default.Register<TradeOrderMessage>(this);
        WeakReferenceMessenger.Default.Register<TiltWarningMessage>(this);
        WeakReferenceMessenger.Default.Register<JournalEntrySavedMessage>(this);
    }

    public void Receive(TradeOrderMessage message)
    {
        var order = message.Value;
        bool isLoss = new Random().NextDouble() > 0.5;
        _tradeHistory.Add(new TradeInfo(DateTime.Now, isLoss));
        EvaluatePsychologicalState();
    }

    public void Receive(TiltWarningMessage message)
    {
        AdjustDisciplineScore(-10);
    }

    public void Receive(JournalEntrySavedMessage message)
    {
        var notes = message.Value.Notes ?? string.Empty;
        var words = notes.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        // Reward thoughtful journaling
        if (words.Length > 15)
        {
            AdjustDisciplineScore(5);
        }
    }

    private void AdjustDisciplineScore(int delta)
    {
        DisciplineScore = Math.Clamp(DisciplineScore + delta, 0, 100);
        WeakReferenceMessenger.Default.Send(new DisciplineScoreMessage(DisciplineScore));
    }

    private void EvaluatePsychologicalState()
    {
        var recent2MinTrades = _tradeHistory
            .Where(t => (DateTime.Now - t.Timestamp).TotalMinutes <= 2)
            .ToList();

        bool has3ConsecutiveLosses = false;
        if (_tradeHistory.Count >= 3)
        {
            var last3 = _tradeHistory.TakeLast(3).ToList();
            has3ConsecutiveLosses = last3.All(t => t.IsLoss);
        }

        if (recent2MinTrades.Count >= 5 || has3ConsecutiveLosses)
        {
            WeakReferenceMessenger.Default.Send(new TiltWarningMessage());
            _tradeHistory.Clear();
        }
    }
}
