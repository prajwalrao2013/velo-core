using System;
using System.Threading;
using System.Threading.Tasks;
using VeloTerminal.Interfaces;
using VeloTerminal.Models;

namespace VeloTerminal.Services;

public class MacroEventService
{
    private readonly IMockMarketSimulator _marketSimulator;
    public event Action<EconomicEvent>? OnEventTriggered;
    private readonly Random _random = new();

    private readonly string[] _headlines = {
        "RBI Rate Hike decision released",
        "Inflation Data higher than expected",
        "Global Markets showing weakness",
        "Unexpected geopolitical tensions",
        "Unemployment figures dropped"
    };

    public MacroEventService(IMockMarketSimulator marketSimulator)
    {
        _marketSimulator = marketSimulator;
    }

    public void StartService(CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Wait randomly between 15 and 45 seconds for demo purposes
                await Task.Delay(_random.Next(15000, 45000), cancellationToken);

                var e = new EconomicEvent(
                    _headlines[_random.Next(_headlines.Length)],
                    _random.NextDouble() > 0.6 ? ImpactSeverity.High : ImpactSeverity.Low,
                    DateTime.Now.AddSeconds(10)
                );

                OnEventTriggered?.Invoke(e);

                // Artificially spike the volatility for 10 seconds
                _marketSimulator.SetVolatilityMultiplier(e.ImpactSeverity == ImpactSeverity.High ? 5.0 : 2.0);
                
                _ = Task.Run(async () => {
                    await Task.Delay(10000, cancellationToken);
                    _marketSimulator.SetVolatilityMultiplier(1.0); // normalize
                }, cancellationToken);
            }
        }, cancellationToken);
    }
}
