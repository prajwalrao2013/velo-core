using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using VeloTerminal.Models;

namespace VeloTerminal.ViewModels;

public partial class ScalperViewModel : ObservableRecipient, IRecipient<TickDataMessage>
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredMargin))]
    private int _orderQuantity = 120;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredMargin))]
    private double _currentAsk = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredMargin))]
    private double _currentBid = 0;

    [ObservableProperty]
    private string _executionFeedback = string.Empty;

    public double RequiredMargin => (OrderQuantity * CurrentAsk) / 5;

    public ScalperViewModel()
    {
        IsActive = true;
    }

    public void Receive(TickDataMessage message)
    {
        Dispatcher.UIThread.Post(() => {
            CurrentAsk = message.Value.Ask;
            CurrentBid = message.Value.Bid;
        });
    }

    [RelayCommand]
    private async Task BuyAsync()
    {
        var order = new TradeOrder(Guid.NewGuid().ToString(), "NIFTY50", OrderQuantity, true, CurrentAsk, DateTime.Now);
        WeakReferenceMessenger.Default.Send(new TradeOrderMessage(order));
        await FlashFeedback("BUY FILLED");
    }

    [RelayCommand]
    private async Task SellAsync()
    {
        var order = new TradeOrder(Guid.NewGuid().ToString(), "NIFTY50", OrderQuantity, false, CurrentBid, DateTime.Now);
        WeakReferenceMessenger.Default.Send(new TradeOrderMessage(order));
        await FlashFeedback("SELL FILLED");
    }

    private async Task FlashFeedback(string msg)
    {
        ExecutionFeedback = msg;
        await Task.Delay(1000); // UI feedback loop without thread block
        ExecutionFeedback = string.Empty;
    }
}
