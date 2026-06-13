using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using VeloTerminal.Models;

namespace VeloTerminal.Services;

public class JournalService : IRecipient<TradeOrderMessage>
{
    private readonly string _filePath;
    public ObservableCollection<JournalEntry> Entries { get; } = new();

    private CancellationTokenSource? _debounceCts;

    public JournalService()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dir = Path.Combine(localAppData, "VeloTerminal");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "velo_journal.json");

        LoadJournal();
        
        WeakReferenceMessenger.Default.Register(this);
    }

    private void LoadJournal()
    {
        if (File.Exists(_filePath))
        {
            try
            {
                var json = File.ReadAllText(_filePath);
                var loaded = JsonSerializer.Deserialize<JournalEntry[]>(json);
                if (loaded != null)
                {
                    foreach (var entry in loaded)
                    {
                        Entries.Add(entry);
                    }
                }
            }
            catch { /* Ignore load errors for simulation */ }
        }
    }

    public void Receive(TradeOrderMessage message)
    {
        var entry = new JournalEntry(message.Value);
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => {
            Entries.Insert(0, entry);
        });
        
        SaveDebounced();
    }

    public void SaveDebounced()
    {
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(500, token); // 500ms debounce
                if (!token.IsCancellationRequested)
                {
                    SaveImmediated();
                }
            }
            catch (TaskCanceledException) { }
        }, token);
    }
    
    private void SaveImmediated()
    {
        lock (_filePath)
        {
            var json = JsonSerializer.Serialize(Entries, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);

            // Emit a generic verification that a manual journal save was accomplished to Psychology Engine lookup
            if (Entries.Count > 0)
            {
                WeakReferenceMessenger.Default.Send(new JournalEntrySavedMessage(Entries[0]));
            }
        }
    }
}
