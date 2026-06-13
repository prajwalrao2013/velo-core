using CommunityToolkit.Mvvm.Messaging.Messages;

namespace VeloTerminal.Models;

public class TickDataMessage : ValueChangedMessage<TickData>
{
    public TickDataMessage(TickData value) : base(value) {}
}
