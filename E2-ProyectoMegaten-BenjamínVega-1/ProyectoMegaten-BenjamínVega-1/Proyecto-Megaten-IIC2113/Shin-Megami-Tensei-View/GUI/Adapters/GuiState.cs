using System.Collections.Generic;
using Shin_Megami_Tensei_GUI;

namespace Shin_Megami_Tensei_View.GUI.Adapters;

public sealed class GuiState : IState
{
    private readonly GuiPlayer _player1 = new();
    private readonly GuiPlayer _player2 = new();
    private readonly List<string> _options = new();
    private readonly List<string> _order = new();

    public IPlayer Player1 => _player1;
    public IPlayer Player2 => _player2;

    public IEnumerable<string> Options => _options;

    public int Turns { get; private set; }
    public int BlinkingTurns { get; private set; }

    public IEnumerable<string> Order => _order;

    internal GuiPlayer Player1Adapter => _player1;
    internal GuiPlayer Player2Adapter => _player2;

    public void SetOrder(IEnumerable<string>? orden)
    {
        _order.Clear();
        if (orden is null) return;
        foreach (var line in orden)
        {
            if (!string.IsNullOrWhiteSpace(line))
                _order.Add(line);
        }
    }

    public void SetOptions(IEnumerable<string>? opciones)
    {
        _options.Clear();
        if (opciones is null) return;
        foreach (var option in opciones)
        {
            if (!string.IsNullOrWhiteSpace(option))
                _options.Add(option);
        }
    }

    public void SetTurns(int full, int blinking)
    {
        Turns = full;
        BlinkingTurns = blinking;
    }
}
