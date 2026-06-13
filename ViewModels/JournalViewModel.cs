using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using VeloTerminal.Models;
using VeloTerminal.Services;

namespace VeloTerminal.ViewModels;

public partial class JournalViewModel : ObservableObject
{
    private readonly JournalService _journalService;

    public ObservableCollection<JournalEntry> Entries => _journalService.Entries;

    [ObservableProperty]
    private JournalEntry? _selectedEntry;

    public Array EmotionalStates => Enum.GetValues(typeof(EmotionalState));

    public JournalViewModel(JournalService journalService)
    {
        _journalService = journalService;
    }

    partial void OnSelectedEntryChanged(JournalEntry? value)
    {
        // Additional hooks if necessary when selected entry swaps
    }

    public void TriggerSave()
    {
        _journalService.SaveDebounced();
    }
}
