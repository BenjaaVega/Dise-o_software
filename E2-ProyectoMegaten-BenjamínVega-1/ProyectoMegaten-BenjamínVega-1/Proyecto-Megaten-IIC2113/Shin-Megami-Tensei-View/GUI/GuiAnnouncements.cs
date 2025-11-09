using System;
using System.Collections.Generic;
using System.Linq;
using Shin_Megami_Tensei_GUI;
using Shin_Megami_Tensei_Model.Combate;
using Shin_Megami_Tensei_Model.ModelosEquipo;

namespace Shin_Megami_Tensei_View.GUI;

internal static class GuiAnnouncements
{
    public static void Attack(SMTGUI window, string atacante, string objetivo, int dano, int hpFinal, int hpMax)
    {
        var lines = new[]
        {
            $"{atacante} ataca a {objetivo}",
            $"{objetivo} recibe {dano} de daño",
            $"{objetivo} termina con HP:{hpFinal}/{hpMax}"
        };
        Show(window, lines);
    }

    public static void Shot(SMTGUI window, string atacante, string objetivo, int dano, int hpFinal, int hpMax)
    {
        var lines = new[]
        {
            $"{atacante} dispara a {objetivo}",
            $"{objetivo} recibe {dano} de daño",
            $"{objetivo} termina con HP:{hpFinal}/{hpMax}"
        };
        Show(window, lines);
    }

    public static void Turns(SMTGUI window, int fullUsed, int blinkingUsed, int blinkingGained)
    {
        var lines = new[]
        {
            $"Se han consumido {fullUsed} Full Turn(s) y {blinkingUsed} Blinking Turn(s)",
            $"Se han obtenido {blinkingGained} Blinking Turn(s)"
        };
        Show(window, lines);
    }

    public static void Skill(
        SMTGUI window,
        string usuario,
        string skill,
        string objetivo,
        int dano,
        int hpFinal,
        int hpMax,
        string? notaAfinidad)
    {
        var lines = new List<string>
        {
            $"{usuario} usa {skill} sobre {objetivo}",
            $"{objetivo} recibe {dano} de daño",
            $"{objetivo} termina con HP:{hpFinal}/{hpMax}"
        };
        if (!string.IsNullOrWhiteSpace(notaAfinidad))
            lines.Add($"[{notaAfinidad}]");
        Show(window, lines);
    }

    public static void Elemental(
        SMTGUI window,
        string atacante,
        string objetivo,
        Elemento elemento,
        HitOutcome outcome,
        int dano,
        int hpFinal,
        int hpMax,
        bool objetivoEliminado)
    {
        var lines = new List<string>();

        string accion = elemento switch
        {
            Elemento.Phys  => "ataca",
            Elemento.Gun   => "dispara",
            Elemento.Fire  => "lanza fuego",
            Elemento.Ice   => "lanza hielo",
            Elemento.Elec  => "lanza electricidad",
            Elemento.Force => "lanza viento",
            Elemento.Light => "ataca con luz",
            Elemento.Dark  => "ataca con oscuridad",
            _              => "ataca"
        };
        lines.Add($"{atacante} {accion} a {objetivo}");

        var esOhko = elemento == Elemento.Light || elemento == Elemento.Dark;
        if (esOhko && dano < 0)
        {
            lines.Add($"{atacante} ha fallado el ataque");
            lines.Add($"{objetivo} termina con HP:{hpFinal}/{hpMax}");
            Show(window, lines);
            return;
        }

        switch (outcome)
        {
            case HitOutcome.Weak:
                lines.Add($"{objetivo} es débil contra el ataque de {atacante}");
                break;
            case HitOutcome.Resist:
                lines.Add($"{objetivo} es resistente el ataque de {atacante}");
                break;
            case HitOutcome.Nullified:
                lines.Add($"{objetivo} bloquea el ataque de {atacante}");
                if (hpMax == -1)
                {
                    Show(window, lines);
                    return;
                }
                lines.Add($"{objetivo} termina con HP:{hpFinal}/{hpMax}");
                Show(window, lines);
                return;
            case HitOutcome.Reflected:
                lines.Add($"{objetivo} devuelve {dano} daño a {atacante}");
                if (hpMax == -1)
                {
                    Show(window, lines);
                    return;
                }
                lines.Add($"{atacante} termina con HP:{hpFinal}/{hpMax}");
                Show(window, lines);
                return;
            case HitOutcome.Drained:
                lines.Add($"{objetivo} absorbe {dano} daño");
                if (hpMax == -1)
                {
                    Show(window, lines);
                    return;
                }
                lines.Add($"{objetivo} termina con HP:{hpFinal}/{hpMax}");
                Show(window, lines);
                return;
        }

        if (objetivoEliminado && esOhko)
        {
            lines.Add($"{objetivo} ha sido eliminado");
            lines.Add($"{objetivo} termina con HP:0/{hpMax}");
            Show(window, lines);
            return;
        }

        lines.Add($"{objetivo} recibe {dano} de daño");
        if (hpMax != -1)
            lines.Add($"{objetivo} termina con HP:{hpFinal}/{hpMax}");

        Show(window, lines);
    }

    private static void Show(SMTGUI window, IEnumerable<string> lines)
    {
        if (window is null) throw new ArgumentNullException(nameof(window));
        var texto = string.Join("\n", lines.Where(line => !string.IsNullOrWhiteSpace(line)));
        if (string.IsNullOrWhiteSpace(texto))
            return;
        window.ShowEndGameMessage(texto);
    }
}
