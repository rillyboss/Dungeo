using TestRPGGame.Entities;
using TestRPGGame.Entities.Player;
using TestRPGGame.Interfaces;

namespace TestRPGGame.Blazor.Services;

/// <summary>
/// Manages game state and provides reactive updates to Blazor components
/// </summary>
public class GameStateService
{
    public event Action? OnStateChanged;
    public event Action<string>? OnMessage;
    public event Action<List<string>, TaskCompletionSource<string>>? OnChoiceRequired;

    public GameCore? CurrentGame { get; set; }
    public bool IsInCombat { get; set; }
    public bool IsGameActive { get; set; }
    public List<string> EventLog { get; } = new();

    // Pending user choice
    public List<string>? PendingChoices { get; set; }
    public TaskCompletionSource<string>? PendingChoiceTcs { get; set; }

    public void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }

    public void AddEventLog(string message)
    {
        EventLog.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
        OnMessage?.Invoke(message);
        NotifyStateChanged();
    }

    public void RequestChoice(List<string> choices, TaskCompletionSource<string> tcs)
    {
        PendingChoices = choices;
        PendingChoiceTcs = tcs;
        OnChoiceRequired?.Invoke(choices, tcs);
        NotifyStateChanged();
    }

    public void SelectChoice(string choice)
    {
        if (PendingChoiceTcs != null)
        {
            PendingChoiceTcs.SetResult(choice);
            PendingChoices = null;
            PendingChoiceTcs = null;
            NotifyStateChanged();
        }
    }

    public void ClearEventLog()
    {
        EventLog.Clear();
        NotifyStateChanged();
    }
}
