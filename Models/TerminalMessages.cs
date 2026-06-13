using CommunityToolkit.Mvvm.Messaging.Messages;

namespace VeloTerminal.Models;

public class TradeOrderMessage : ValueChangedMessage<TradeOrder>
{
    public TradeOrderMessage(TradeOrder value) : base(value) {}
}

public class TiltWarningMessage {}

public class DisciplineScoreMessage : ValueChangedMessage<int>
{
    public DisciplineScoreMessage(int score) : base(score) {}
}

public class JournalEntrySavedMessage : ValueChangedMessage<JournalEntry>
{
    public JournalEntrySavedMessage(JournalEntry value) : base(value) {}
}
