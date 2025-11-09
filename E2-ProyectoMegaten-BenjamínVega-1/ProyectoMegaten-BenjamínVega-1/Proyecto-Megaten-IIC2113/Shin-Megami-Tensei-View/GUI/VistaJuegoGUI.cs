using System;
using Shin_Megami_Tensei_GUI;
using Shin_Megami_Tensei_Model.Combate;
using Shin_Megami_Tensei_Model.ModelosEquipo;
using Shin_Megami_Tensei_Model.Presentacion.ViewModels;
using Shin_Megami_Tensei_Model.Presentacion.VistaModelos;
using Shin_Megami_Tensei_View.GUI.Adapters;
using Shin_Megami_Tensei_View.GUI.Mappers;

namespace Shin_Megami_Tensei_View.GUI;

public sealed class VistaJuegoGUI : IVistaJuego
{
    private readonly SMTGUI _window;
    private readonly GuiState _state = new();

    public VistaJuegoGUI()
        : this(new SMTGUI())
    {
    }

    public VistaJuegoGUI(SMTGUI window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
    }

    public SMTGUI Window => _window;

    public void ShowTeamFiles(IReadOnlyList<string> archivos)
    {
        // La GUI no requiere mostrar archivos disponibles.
    }

    public int ReadFileIndex() => 0;

    public void ShowInvalidTeamsAndExit() => _window.ShowEndGameMessage("Equipo inválido");

    public void ShowRoundHeader(string samuraiNombre, string tag)
    {
        // El encabezado de ronda se maneja visualmente en la GUI.
    }

    public void ShowBoard(SnapshotTablero snapshot)
    {
        if (snapshot is null) return;
        SnapshotMapper.ApplyBoard(_state, snapshot);
        _window.Update(_state);
    }

    public void ShowTurns(SnapshotTurno snapshot)
    {
        if (snapshot is null) return;
        SnapshotMapper.ApplyTurns(_state, snapshot);
        _window.Update(_state);
    }

    public void ShowOrder(IReadOnlyList<string> orden)
    {
        _state.SetOrder(orden);
        _window.Update(_state);
    }

    public int ReadActionFor(string unidadNombre, MenuAcciones menu)
    {
        var opciones = menu?.Opciones ?? Array.Empty<string>();
        _state.SetOptions(opciones);
        _window.Update(_state);
        return GuiInput.WaitMenu(_window, menu);
    }

    public int ReadTarget(string atacanteNombre, MenuObjetivo menu)
    {
        var opciones = GuiInput.BuildObjectiveOptions(menu);
        _state.SetOptions(opciones);
        _window.Update(_state);
        return GuiInput.WaitTarget(_window, menu);
    }

    public int ReadSkill(string unidadNombre, SkillMenu menu)
    {
        var opciones = GuiInput.BuildSkillOptions(menu);
        _state.SetOptions(opciones);
        _window.Update(_state);
        return GuiInput.WaitMenu(_window, menu);
    }

    public int ReadCustom(string header, IReadOnlyList<string> opciones)
    {
        _state.SetOptions(opciones);
        _window.Update(_state);
        return GuiInput.WaitCustom(_window, header, opciones);
    }

    public void ShowAttackResolution(string atacante, string objetivo, int dano, int hpFinal, int hpMax)
        => GuiAnnouncements.Attack(_window, atacante, objetivo, dano, hpFinal, hpMax);

    public void ShowShotResolution(string atacante, string objetivo, int dano, int hpFinal, int hpMax)
        => GuiAnnouncements.Shot(_window, atacante, objetivo, dano, hpFinal, hpMax);

    public void ShowTurnsConsumption(int fullUsed, int blinkingUsed, int blinkingGained)
        => GuiAnnouncements.Turns(_window, fullUsed, blinkingUsed, blinkingGained);

    public void ShowSurrender(string nombre, string tag)
        => _window.ShowEndGameMessage($"{nombre} ({tag}) se rinde");

    public void ShowWinner(string nombre, string tag)
        => _window.ShowEndGameMessage($"¡{nombre} ({tag}) ha ganado!");

    public void ShowMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        _window.ShowEndGameMessage(message);
    }

    public void ShowSkillResolution(
        string usuario,
        string skill,
        string objetivo,
        int dano,
        int hpFinal,
        int hpMax,
        string? notaAfinidad = null)
        => GuiAnnouncements.Skill(_window, usuario, skill, objetivo, dano, hpFinal, hpMax, notaAfinidad);

    public void ShowElementalResolution(
        string atacante,
        string objetivo,
        Elemento elemento,
        HitOutcome outcome,
        int dano,
        int hpFinal,
        int hpMax,
        bool objetivoEliminado = false)
        => GuiAnnouncements.Elemental(_window, atacante, objetivo, elemento, outcome, dano, hpFinal, hpMax, objetivoEliminado);
}
