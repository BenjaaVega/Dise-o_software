using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Shin_Megami_Tensei_Model.Combate;
using Shin_Megami_Tensei_Model.ModelosEquipo;
using Shin_Megami_Tensei_Model.Presentacion.VistaModelos;
using Shin_Megami_Tensei_Model.Presentacion.ViewModels;
using global::Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei_View.VistaGUI;

public sealed class VistaJuegoGui : IVistaJuego
{
    private readonly VistaJuegoConsola? _fallback;
    private readonly GuiLibrary? _library;
    private readonly GuiProxyFactory? _factory;
    private readonly object? _gui;
    private readonly GuiStateModel? _state;
    private readonly List<OptionEntry> _currentOptions = new();
    private readonly Dictionary<string, OptionEntry> _optionLookup = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _sync = new();

    public VistaJuegoGui(View? fallbackView = null)
    {
        if (fallbackView is not null)
        {
            _fallback = new VistaJuegoConsola(fallbackView);
        }

        try
        {
            _library = GuiLibrary.Load();
            _factory = new GuiProxyFactory(_library);
            _gui = _library.CreateGui();
            _state = new GuiStateModel();
        }
        catch
        {
            _library = null;
            _factory = null;
            _gui = null;
            _state = null;
        }
    }

    private bool IsGuiReady => _gui is not null && _library is not null && _factory is not null && _state is not null;

    public void Run(Action startProgram)
    {
        if (startProgram is null) throw new ArgumentNullException(nameof(startProgram));

        if (IsGuiReady)
        {
            _library!.Start(_gui!, startProgram);
        }
        else
        {
            startProgram();
        }
    }

    public void ShowTeamFiles(IReadOnlyList<string> archivos)
    {
        if (!IsGuiReady)
        {
            _fallback?.ShowTeamFiles(archivos);
            return;
        }

        var opts = new List<OptionEntry>();
        var lista = archivos ?? Array.Empty<string>();
        for (int i = 0; i < lista.Count; i++)
        {
            var label = string.Create(CultureInfo.InvariantCulture, $"{i + 1}: {lista[i]}");
            opts.Add(new OptionEntry(label, i));
        }

        SetMenu("Selecciona un archivo de equipo", opts);
    }

    public int ReadFileIndex()
    {
        if (!IsGuiReady)
        {
            return _fallback?.ReadFileIndex() ?? 0;
        }

        var result = WaitForOptionSelection(0);
        ClearMenu();
        return result;
    }

    public void ShowInvalidTeamsAndExit()
    {
        if (!IsGuiReady)
        {
            _fallback?.ShowInvalidTeamsAndExit();
            return;
        }

        DisplayMessage("Al menos un equipo es inválido.");
        ClearMenu();
    }

    public void ShowRoundHeader(string samuraiNombre, string tag)
    {
        if (!IsGuiReady)
        {
            _fallback?.ShowRoundHeader(samuraiNombre, tag);
            return;
        }

        DisplayMessage(string.Create(CultureInfo.InvariantCulture, $"Ronda de {samuraiNombre} ({tag})"));
    }

    public void ShowBoard(SnapshotTablero snapshot)
    {
        if (!IsGuiReady)
        {
            _fallback?.ShowBoard(snapshot);
            return;
        }

        _state!.Player1.UpdateBoard(snapshot?.J1Slots);
        _state.Player2.UpdateBoard(snapshot?.J2Slots);
        Refresh();
    }

    public void ShowTurns(SnapshotTurno snapshot)
    {
        if (!IsGuiReady)
        {
            _fallback?.ShowTurns(snapshot);
            return;
        }

        _state!.Turns = snapshot?.Full ?? 0;
        _state.BlinkingTurns = snapshot?.Blinking ?? 0;
        Refresh();
    }

    public void ShowOrder(IReadOnlyList<string> ordenNumerado)
    {
        if (!IsGuiReady)
        {
            _fallback?.ShowOrder(ordenNumerado);
            return;
        }

        _state!.ClearOrder();
        if (ordenNumerado is not null)
        {
            foreach (var line in ordenNumerado)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    _state.Order.Add(line);
            }
        }
        Refresh();
    }

    public int ReadActionFor(string unidadNombre, MenuAcciones menu)
    {
        if (!IsGuiReady)
        {
            return _fallback?.ReadActionFor(unidadNombre, menu) ?? -1;
        }

        var opts = (menu?.Opciones ?? Array.Empty<string>())
            .Select((text, index) => new OptionEntry(text, index + 1))
            .ToList();

        SetMenu(string.Create(CultureInfo.InvariantCulture, $"Seleccione una acción para {unidadNombre}"), opts);
        var value = WaitForOptionSelection(-1);
        ClearMenu();
        return value;
    }

    public int ReadTarget(string atacanteNombre, MenuObjetivo menu)
    {
        if (!IsGuiReady)
        {
            return _fallback?.ReadTarget(atacanteNombre, menu) ?? menu.CancelIndex + 1;
        }

        var opts = new List<OptionEntry>();
        var items = menu?.Items ?? Array.Empty<ItemObjetivo>();
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var label = string.Create(
                CultureInfo.InvariantCulture,
                $"{i + 1}-{item.Nombre} HP:{item.Hp}/{item.HpMax} MP:{item.Mp}/{item.MpMax}");
            opts.Add(new OptionEntry(label, i + 1));
        }

        opts.Add(new OptionEntry(string.Create(CultureInfo.InvariantCulture, $"{menu.CancelIndex + 1}-Cancelar"), menu.CancelIndex + 1));

        SetMenu(string.Create(CultureInfo.InvariantCulture, $"Seleccione un objetivo para {atacanteNombre}"), opts);
        var value = WaitForOptionSelection(menu.CancelIndex + 1);
        ClearMenu();
        return value;
    }

    public int ReadSkill(string unidadNombre, SkillMenu menu)
    {
        if (!IsGuiReady)
        {
            return _fallback?.ReadSkill(unidadNombre, menu) ?? menu.CancelIndex + 1;
        }

        var opts = new List<OptionEntry>();
        var items = menu?.Items ?? Array.Empty<SkillItem>();
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            string label = item.Costo.HasValue
                ? string.Create(CultureInfo.InvariantCulture, $"{i + 1}-{item.Nombre} MP:{item.Costo.Value}")
                : string.Create(CultureInfo.InvariantCulture, $"{i + 1}-{item.Nombre}");
            opts.Add(new OptionEntry(label, i + 1));
        }

        opts.Add(new OptionEntry(string.Create(CultureInfo.InvariantCulture, $"{menu.CancelIndex + 1}-Cancelar"), menu.CancelIndex + 1));

        SetMenu(string.Create(CultureInfo.InvariantCulture, $"Seleccione una habilidad para que {unidadNombre} use"), opts);
        var value = WaitForOptionSelection(menu.CancelIndex + 1);
        ClearMenu();
        return value;
    }

    public int ReadCustom(string header, IReadOnlyList<string> opciones)
    {
        if (!IsGuiReady)
        {
            return _fallback?.ReadCustom(header, opciones) ?? 0;
        }

        var opts = (opciones ?? Array.Empty<string>())
            .Select((text, index) => new OptionEntry(text, index + 1))
            .ToList();

        SetMenu(header, opts);
        var value = WaitForOptionSelection(opciones?.Count ?? 0);
        ClearMenu();
        return value;
    }

    public void ShowAttackResolution(string atacante, string objetivo, int dano, int hpFinal, int hpMax)
    {
        if (!IsGuiReady)
        {
            _fallback?.ShowAttackResolution(atacante, objetivo, dano, hpFinal, hpMax);
            return;
        }

        DisplayMessage(string.Create(CultureInfo.InvariantCulture,
            $"{atacante} ataca a {objetivo} causando {dano} de daño. {objetivo} termina con HP:{hpFinal}/{hpMax}."));
    }

    public void ShowShotResolution(string atacante, string objetivo, int dano, int hpFinal, int hpMax)
    {
        if (!IsGuiReady)
        {
            _fallback?.ShowShotResolution(atacante, objetivo, dano, hpFinal, hpMax);
            return;
        }

        DisplayMessage(string.Create(CultureInfo.InvariantCulture,
            $"{atacante} dispara a {objetivo} causando {dano} de daño. {objetivo} termina con HP:{hpFinal}/{hpMax}."));
    }

    public void ShowTurnsConsumption(int fullUsed, int blinkingUsed, int blinkingGained)
    {
        if (!IsGuiReady)
        {
            _fallback?.ShowTurnsConsumption(fullUsed, blinkingUsed, blinkingGained);
            return;
        }

        DisplayMessage(string.Create(
            CultureInfo.InvariantCulture,
            $"Se han consumido {fullUsed} Full Turn(s) y {blinkingUsed} Blinking Turn(s). Se han obtenido {blinkingGained} Blinking Turn(s)."));
    }

    public void ShowSurrender(string nombre, string tag)
    {
        if (!IsGuiReady)
        {
            _fallback?.ShowSurrender(nombre, tag);
            return;
        }

        DisplayMessage(string.Create(CultureInfo.InvariantCulture, $"{nombre} ({tag}) se ha rendido."));
    }

    public void ShowWinner(string nombre, string tag)
    {
        if (!IsGuiReady)
        {
            _fallback?.ShowWinner(nombre, tag);
            return;
        }

        DisplayMessage(string.Create(CultureInfo.InvariantCulture, $"{nombre} ({tag}) ha ganado la partida."));
    }

    public void ShowMessage(string message)
    {
        if (!IsGuiReady)
        {
            _fallback?.ShowMessage(message);
            return;
        }

        if (!string.IsNullOrWhiteSpace(message))
        {
            DisplayMessage(message);
        }
    }

    public void ShowSkillResolution(
        string usuario,
        string skill,
        string objetivo,
        int dano,
        int hpFinal,
        int hpMax,
        string? notaAfinidad = null)
    {
        if (!IsGuiReady)
        {
            _fallback?.ShowSkillResolution(usuario, skill, objetivo, dano, hpFinal, hpMax, notaAfinidad);
            return;
        }

        var texto = string.Create(CultureInfo.InvariantCulture,
            $"{usuario} usa {skill} sobre {objetivo} causando {dano} de daño. {objetivo} termina con HP:{hpFinal}/{hpMax}.");
        if (!string.IsNullOrWhiteSpace(notaAfinidad))
        {
            texto = string.Create(CultureInfo.InvariantCulture, $"{texto} ({notaAfinidad})");
        }
        DisplayMessage(texto);
    }

    public void ShowElementalResolution(
        string atacante,
        string objetivo,
        Elemento elemento,
        HitOutcome outcome,
        int dano,
        int hpFinal,
        int hpMax,
        bool objetivoEliminado = false)
    {
        if (!IsGuiReady)
        {
            _fallback?.ShowElementalResolution(atacante, objetivo, elemento, outcome, dano, hpFinal, hpMax, objetivoEliminado);
            return;
        }

        var texto = string.Create(CultureInfo.InvariantCulture,
            $"{atacante} ejecuta un ataque {elemento} sobre {objetivo} ({outcome}) causando {dano} de daño. {objetivo} termina con HP:{hpFinal}/{hpMax}.");
        if (objetivoEliminado)
        {
            texto = string.Create(CultureInfo.InvariantCulture, $"{texto} {objetivo} ha sido eliminado.");
        }
        DisplayMessage(texto);
    }

    private void SetMenu(string? prompt, IReadOnlyList<OptionEntry> entries)
    {
        if (!IsGuiReady) return;

        _currentOptions.Clear();
        _optionLookup.Clear();
        _state!.ClearOptions();

        if (!string.IsNullOrWhiteSpace(prompt))
        {
            _state.Options.Add(prompt);
        }

        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Label)) continue;
            _currentOptions.Add(entry);
            _optionLookup[Normalize(entry.Label)] = entry;
            _state.Options.Add(entry.Label);
        }

        Refresh();
    }

    private int WaitForOptionSelection(int fallback)
    {
        if (!IsGuiReady) return fallback;

        while (true)
        {
            var clicked = _library!.GetClickedElement(_gui!);
            if (_library.TryGetButtonText(clicked, out var text) && !string.IsNullOrWhiteSpace(text))
            {
                var normalized = Normalize(text);
                if (_optionLookup.TryGetValue(normalized, out var entry))
                {
                    return entry.Value;
                }

                var parsed = ExtractLeadingNumber(text);
                if (parsed.HasValue)
                {
                    var match = _currentOptions.FirstOrDefault(o => o.Value == parsed.Value);
                    if (match != null)
                        return match.Value;
                }
            }
        }
    }

    private void ClearMenu()
    {
        if (!IsGuiReady) return;
        _currentOptions.Clear();
        _optionLookup.Clear();
        _state!.ClearOptions();
        Refresh();
    }

    private void DisplayMessage(string message)
    {
        if (!IsGuiReady) return;
        if (string.IsNullOrWhiteSpace(message)) return;
        _library!.ShowMessage(_gui!, message);
    }

    private void Refresh()
    {
        if (!IsGuiReady) return;
        lock (_sync)
        {
            var state = _factory!.CreateState(_state!);
            _library!.Update(_gui!, state);
        }
    }

    private static string Normalize(string text)
        => text.Trim();

    private static int? ExtractLeadingNumber(string text)
    {
        text = text ?? string.Empty;
        var digits = new List<char>();
        foreach (var ch in text)
        {
            if (char.IsDigit(ch)) digits.Add(ch);
            else if (digits.Count > 0) break;
        }

        return digits.Count > 0 && int.TryParse(new string(digits.ToArray()), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : (int?)null;
    }

    private readonly record struct OptionEntry(string Label, int Value);
}
