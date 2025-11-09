using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Shin_Megami_Tensei_GUI;
using Shin_Megami_Tensei_Model.Presentacion.ViewModels;

namespace Shin_Megami_Tensei_View.GUI;

internal static class GuiInput
{
    private static readonly Regex OptionPrefixRegex = new("^[\\s]*([0-9]+)[\\:\\-]\\s*(.*)$", RegexOptions.Compiled);

    public static IReadOnlyList<string> BuildObjectiveOptions(MenuObjetivo menu)
    {
        if (menu is null) return Array.Empty<string>();

        var opciones = new List<string>();
        if (menu.Items != null)
        {
            for (int i = 0; i < menu.Items.Count; i++)
            {
                var it = menu.Items[i];
                opciones.Add($"{i + 1}-{it.Nombre} HP:{it.Hp}/{it.HpMax} MP:{it.Mp}/{it.MpMax}");
            }
        }

        opciones.Add($"{menu.CancelIndex + 1}-Cancelar");
        return opciones;
    }

    public static IReadOnlyList<string> BuildSkillOptions(SkillMenu menu)
    {
        if (menu is null) return Array.Empty<string>();

        var opciones = new List<string>();
        if (menu.Items != null)
        {
            for (int i = 0; i < menu.Items.Count; i++)
            {
                var it = menu.Items[i];
                if (it.Costo.HasValue)
                    opciones.Add($"{i + 1}-{it.Nombre} MP:{it.Costo.Value}");
                else
                    opciones.Add($"{i + 1}-{it.Nombre}");
            }
        }

        opciones.Add($"{menu.CancelIndex + 1}-Cancelar");
        return opciones;
    }

    public static int WaitMenu(SMTGUI window, MenuAcciones menu)
        => WaitForOption(window, menu?.Opciones ?? Array.Empty<string>());

    public static int WaitMenu(SMTGUI window, SkillMenu menu)
        => WaitForOption(window, BuildSkillOptions(menu));

    public static int WaitCustom(SMTGUI window, string header, IReadOnlyList<string> opciones)
        => WaitForOption(window, opciones ?? Array.Empty<string>());

    public static int WaitTarget(SMTGUI window, MenuObjetivo menu)
        => WaitForOption(window, BuildObjectiveOptions(menu));

    private static int WaitForOption(SMTGUI window, IReadOnlyList<string> opciones)
    {
        if (window is null) throw new ArgumentNullException(nameof(window));

        var listaOpciones = opciones ?? Array.Empty<string>();

        while (true)
        {
            var clicked = window.GetClickedElement();
            if (clicked is null) continue;

            if (clicked.Type == ClickedElementType.Button)
            {
                var idx = IndexOfOption(clicked.Text, listaOpciones);
                if (idx.HasValue) return idx.Value;
                continue;
            }

            if (clicked.Type == ClickedElementType.UnitInBoard || clicked.Type == ClickedElementType.UnitInReserve)
            {
                var idx = IndexOfOption(clicked.Text, listaOpciones);
                if (idx.HasValue) return idx.Value;
            }
        }
    }

    private static int? IndexOfOption(string? texto, IReadOnlyList<string> opciones)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        for (int i = 0; i < opciones.Count; i++)
        {
            var option = opciones[i];
            if (string.Equals(option, texto, StringComparison.OrdinalIgnoreCase))
                return i + 1;

            var normalizedOption = RemovePrefix(option);
            if (string.Equals(normalizedOption, texto, StringComparison.OrdinalIgnoreCase))
                return i + 1;
        }

        return null;
    }

    private static string RemovePrefix(string option)
    {
        if (string.IsNullOrEmpty(option)) return option;
        var match = OptionPrefixRegex.Match(option);
        if (match.Success) return match.Groups[2].Value.Trim();
        return option.Trim();
    }
}
