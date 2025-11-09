using Shin_Megami_Tensei_Model.Presentacion.ViewModels;
using Shin_Megami_Tensei_Model.Presentacion.VistaModelos;
using System.Linq;
using Shin_Megami_Tensei_View.VistaConsola;
using Shin_Megami_Tensei_GUI;

namespace Shin_Megami_Tensei_View.Gui;

public sealed class ShinMegamiTenseiGuiView : IVistaJuego
{
    private readonly SMTGUI _window;
    private readonly VistaJuegoConsola _fallback;
    private SnapshotTablero? _board;
    private SnapshotTurno? _turns;
    private IReadOnlyList<string> _order = Array.Empty<string>();
    private IEnumerable<string> _options = Array.Empty<string>();

    public ShinMegamiTenseiGuiView()
        : this(new SMTGUI(), View.BuildConsoleView())
    {
    }

    public ShinMegamiTenseiGuiView(SMTGUI window)
        : this(window, View.BuildConsoleView())
    {
    }

    public ShinMegamiTenseiGuiView(SMTGUI window, View view)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
        _fallback = new VistaJuegoConsola(view ?? throw new ArgumentNullException(nameof(view)));
    }

    public SMTGUI Window => _window;

    public void Start(Action startProgram)
    {
        _window.Start(startProgram);
    }

    public void ShowTeamFiles(IReadOnlyList<string> archivos)
    {
        _fallback.ShowTeamFiles(archivos);
    }

    public int ReadFileIndex()
    {
        return _fallback.ReadFileIndex();
    }

    public void ShowInvalidTeamsAndExit()
    {
        _window.ShowEndGameMessage("Al menos un equipo es inválido");
        _fallback.ShowInvalidTeamsAndExit();
    }

    public void ShowRoundHeader(string samuraiNombre, string jugadorTag)
    {
        _fallback.ShowRoundHeader(samuraiNombre, jugadorTag);
    }

    public void ShowBoard(SnapshotTablero snapshot)
    {
        _board = snapshot;
        UpdateWindowState();
        _fallback.ShowBoard(snapshot);
    }

    public void ShowTurns(SnapshotTurno snapshot)
    {
        _turns = snapshot;
        UpdateWindowState();
        _fallback.ShowTurns(snapshot);
    }

    public void ShowOrder(IReadOnlyList<string> ordenNumerado)
    {
        _order = ordenNumerado ?? Array.Empty<string>();
        UpdateWindowState();
        _fallback.ShowOrder(ordenNumerado);
    }

    public int ReadActionFor(string unidadNombre, MenuAcciones menu)
    {
        _options = menu?.Opciones ?? Array.Empty<string>();
        UpdateWindowState();
        return _fallback.ReadActionFor(unidadNombre, menu);
    }

    public int ReadTarget(string atacanteNombre, MenuObjetivo menu)
    {
        _options = menu?.Items?.Select((item, index) => $"{index + 1}: {item.Nombre}") ?? Array.Empty<string>();
        UpdateWindowState();
        return _fallback.ReadTarget(atacanteNombre, menu);
    }

    public int ReadSkill(string unidadNombre, SkillMenu menu)
    {
        _options = menu?.Items?.Select((item, index) => $"{index + 1}: {item.Nombre}") ?? Array.Empty<string>();
        UpdateWindowState();
        return _fallback.ReadSkill(unidadNombre, menu);
    }

    public int ReadCustom(string header, IReadOnlyList<string> opciones)
    {
        _options = opciones ?? Array.Empty<string>();
        UpdateWindowState();
        return _fallback.ReadCustom(header, opciones);
    }

    public void ShowAttackResolution(string atacante, string objetivo, int dano, int hpFinal, int hpMax)
    {
        _fallback.ShowAttackResolution(atacante, objetivo, dano, hpFinal, hpMax);
    }

    public void ShowShotResolution(string atacante, string objetivo, int dano, int hpFinal, int hpMax)
    {
        _fallback.ShowShotResolution(atacante, objetivo, dano, hpFinal, hpMax);
    }

    public void ShowTurnsConsumption(int fullUsed, int blinkingUsed, int blinkingGained)
    {
        _fallback.ShowTurnsConsumption(fullUsed, blinkingUsed, blinkingGained);
    }

    public void ShowSurrender(string nombre, string tag)
    {
        _window.ShowEndGameMessage($"{nombre} ({tag}) se rinde");
        _fallback.ShowSurrender(nombre, tag);
    }

    public void ShowWinner(string nombre, string tag)
    {
        _window.ShowEndGameMessage($"{nombre} ({tag}) gana la partida");
        _fallback.ShowWinner(nombre, tag);
    }

    public void ShowMessage(string message)
    {
        _window.ShowEndGameMessage(message);
        _fallback.ShowMessage(message);
    }

    public void ShowSkillResolution(string usuario, string skill, string objetivo, int dano, int hpFinal, int hpMax, string? notaAfinidad = null)
    {
        _fallback.ShowSkillResolution(usuario, skill, objetivo, dano, hpFinal, hpMax, notaAfinidad);
    }

    public void ShowElementalResolution(string atacante, string objetivo, Shin_Megami_Tensei_Model.ModelosEquipo.Elemento elemento, Shin_Megami_Tensei_Model.Combate.HitOutcome outcome, int dano, int hpFinal, int hpMax, bool objetivoEliminado = false)
    {
        _fallback.ShowElementalResolution(atacante, objetivo, elemento, outcome, dano, hpFinal, hpMax, objetivoEliminado);
    }

    private void UpdateWindowState()
    {
        if (_board is null || _turns is null)
            return;

        var player1 = CreatePlayer(_board.J1Slots);
        var player2 = CreatePlayer(_board.J2Slots);
        var state = new GuiState(player1, player2, _options, _turns.Full, _turns.Blinking, _order);
        _window.Update(state);
    }

    private static GuiPlayer CreatePlayer(IReadOnlyList<Slot> slots)
    {
        var boardUnits = slots
            ?.Select(slot => slot.Nombre is null
                ? null
                : new GuiUnit(slot.Nombre, slot.Hp ?? 0, slot.Mp ?? 0, slot.HpMax ?? 0, slot.MpMax ?? 0) as IUnit?)
            .ToArray() ?? Array.Empty<IUnit?>();
        return new GuiPlayer(boardUnits, Array.Empty<IUnit>());
    }
}
