using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using VeloTerminal.Interfaces;
using VeloTerminal.Models;

namespace VeloTerminal.Services;

public class MarketSimulatorService : IMockMarketSimulator
{
    private double _currentPrice;
    private double _volatilityMultiplier = 1.0;
    private readonly object _volLock = new();
    private readonly Random _random = new();

    public MarketSimulatorService()
    {
        // Align simulator to exactly where the historical data left off
        _currentPrice = VeloTerminal.ViewModels.ChartViewModel.LastBaselinePrice;
    }

    public void StartStreaming(CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                double currentVol;
                lock (_volLock) currentVol = _volatilityMultiplier;

                // Geometric Brownian Motion step mock
                double drift = 0.00001; 
                double shock = (_random.NextDouble() - 0.5) * 2.0; // -1 to 1
                
                double change = _currentPrice * (drift + (shock * 0.0005 * currentVol));
                _currentPrice += change;
                
                double spread = 0.50 * currentVol;
                var tick = new TickData(
                    "NIFTY50", 
                    Math.Round(_currentPrice, 2), 
                    Math.Round(_currentPrice - spread, 2), 
                    Math.Round(_currentPrice + spread, 2), 
                    DateTime.Now);

                // Publish decoupled memory-safe message via WeakReferenceMessenger
                WeakReferenceMessenger.Default.Send(new TickDataMessage(tick));

                await Task.Delay(250, cancellationToken); // 4 ticks per second
            }
        }, cancellationToken);
    }

    public void SetVolatilityMultiplier(double multiplier)
    {
        lock (_volLock) _volatilityMultiplier = multiplier;
    }
}
