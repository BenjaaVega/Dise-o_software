using System.Collections.Concurrent;

namespace Shin_Megami_Tensei_GUI;

/// <summary>
/// Minimal in-process implementation of the Shin Megami Tensei GUI contract. The goal of this
/// implementation is to provide an easy integration point for the controller without depending on
/// the original desktop binaries. It prints the relevant information to the console so the behaviour
/// can be inspected when running the program locally.
/// </summary>
public sealed class SMTGUI
{
    private readonly ConcurrentDictionary<int, ITeamInfo> _teams = new();
    private readonly BlockingCollection<IClickedElement> _pendingClicks = new();
    private IState? _state;

    public void Start(Action startProgramCallback)
    {
        if (startProgramCallback is null)
            throw new ArgumentNullException(nameof(startProgramCallback));

        // The real library keeps the UI loop on the main thread. To keep things simple we just invoke
        // the callback synchronously.
        startProgramCallback();
    }

    public void Update(IState state)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        PrintState(state);
    }

    public void ShowEndGameMessage(string message)
    {
        Console.WriteLine($"[GUI] {message}");
    }

    public ITeamInfo GetTeamInfo(int playerId)
    {
        if (_teams.TryGetValue(playerId, out var info))
            return info;

        return new TeamInfo($"Jugador {playerId}", Array.Empty<string>(), Array.Empty<string>());
    }

    public void RegisterTeamInfo(int playerId, ITeamInfo info)
    {
        if (info is null)
            throw new ArgumentNullException(nameof(info));

        _teams[playerId] = info;
    }

    public void EnqueueClickedElement(IClickedElement element)
    {
        if (element is null)
            throw new ArgumentNullException(nameof(element));

        _pendingClicks.Add(element);
    }

    public IClickedElement GetClickedElement()
    {
        Console.WriteLine("[GUI] Waiting for user input. Write a command (e.g. button:Atacar):");
        while (true)
        {
            if (_pendingClicks.TryTake(out var element))
                return element;

            var raw = Console.ReadLine() ?? string.Empty;
            var elementFromConsole = ParseConsoleInput(raw);
            if (elementFromConsole is not null)
                return elementFromConsole;

            Console.WriteLine("[GUI] Invalid input, please try again.");
        }
    }

    private static IClickedElement? ParseConsoleInput(string raw)
    {
        raw = raw.Trim();
        if (raw.Length == 0)
            return null;

        if (!raw.Contains(':'))
            return new ClickedElement(ClickedElementType.Button, raw, null);

        var parts = raw.Split(':', 2, StringSplitOptions.TrimEntries);
        var kind = parts[0].ToLowerInvariant();
        var value = parts[1];

        return kind switch
        {
            "button" => new ClickedElement(ClickedElementType.Button, value, null),
            "board" => new ClickedElement(ClickedElementType.UnitInBoard, value, null),
            "reserve" => new ClickedElement(ClickedElementType.UnitInReserve, value, null),
            _ => null,
        };
    }

    private static void PrintState(IState state)
    {
        Console.WriteLine("[GUI] --- State update ---");
        Console.WriteLine("[GUI] Turns: {0} | Blinking: {1}", state.Turns, state.BlinkingTurns);
        Console.WriteLine("[GUI] Options: " + string.Join(", ", state.Options ?? Array.Empty<string>()));
        Console.WriteLine("[GUI] Order: " + string.Join(" -> ", state.Order ?? Array.Empty<string>()));
        PrintPlayer("J1", state.Player1);
        PrintPlayer("J2", state.Player2);
        Console.WriteLine("[GUI] --------------------");
    }

    private static void PrintPlayer(string tag, IPlayer player)
    {
        Console.WriteLine($"[GUI] Player {tag} board:");
        foreach (var unit in player.UnitsInBoard)
        {
            if (unit is null)
            {
                Console.WriteLine("[GUI]   - (empty)");
            }
            else
            {
                Console.WriteLine($"[GUI]   - {unit.Name} HP:{unit.HP}/{unit.MaxHP} MP:{unit.MP}/{unit.MaxMP}");
            }
        }

        Console.WriteLine($"[GUI] Player {tag} reserve:");
        foreach (var unit in player.UnitsInReserve)
        {
            Console.WriteLine($"[GUI]   - {unit.Name} HP:{unit.HP}/{unit.MaxHP} MP:{unit.MP}/{unit.MaxMP}");
        }
    }

    private sealed record ClickedElement(ClickedElementType Type, string Text, int? PlayerId) : IClickedElement;
}
