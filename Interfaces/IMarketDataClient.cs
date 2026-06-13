using System;
using System.Threading;
using VeloTerminal.Models;

namespace VeloTerminal.Interfaces;

public interface IMarketDataClient
{
    // C# event removed to prevent rigid coupling and memory leaks using traditional subscriptions.
    // Use CommunityToolkit.Mvvm WeakReferenceMessenger.Default.Register<TickDataMessage> instead.
    void StartStreaming(CancellationToken cancellationToken);
}

public interface IMockMarketSimulator : IMarketDataClient
{
    void SetVolatilityMultiplier(double multiplier);
}
