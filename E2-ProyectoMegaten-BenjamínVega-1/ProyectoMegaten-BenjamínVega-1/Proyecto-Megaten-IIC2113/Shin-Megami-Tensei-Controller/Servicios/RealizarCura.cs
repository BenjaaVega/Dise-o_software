using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei_Model.ModelosEquipo;
using Shin_Megami_Tensei.Manejo.Helpers;

namespace Shin_Megami_Tensei.Manejo.Services
{
    public static class RealizarCura
    {
        public enum HealKind { None, HealSingle, HealParty, ReviveSingle, ReviveParty }

        public static HealKind Clasificar(string effectRaw)
        {
            var s = (effectRaw ?? string.Empty).ToLowerInvariant();
            bool aliados   = s.Contains("all allies") || s.Contains("everyone") || s.Contains("party");
            bool revive   = s.Contains("revive");
            bool curarHp  = s.Contains("heals hp") || s.Contains("fully heals") || s.Contains("greatly heals");

            if (revive && aliados) return HealKind.ReviveParty;
            if (revive)           return HealKind.ReviveSingle;
            if (curarHp && aliados) return HealKind.HealParty;
            if (curarHp)           return HealKind.HealSingle;
            return HealKind.None;
        }

        public static bool CuraEquipo(IVistaJuego vistaJuego, Equipo propio, IUnidad actual, int powerPct, bool sacrificaTodoHpUsuario)
        {
            var objetivos = propio.Unidades.Take(Equipo.ActiveSlots)
                               .Where(u => u != null && u.EstaViva).ToList();
            if (objetivos.Count == 0)
            {
                vistaJuego.ShowMessage("No hay aliados disponibles para curar.");
                return false;
            }

            var lines = new List<string>();
            foreach (var unidad in objetivos)
            {
                int heal = Math.Max(0, (int)Math.Floor(unidad.HpMax * (powerPct / 100.0)));
                UnidadHelper.CurarUnidad(unidad, heal);
                lines.Add($"{actual.Nombre} cura a {unidad.Nombre}");
                lines.Add($"{unidad.Nombre} recibe {heal} de HP");
                lines.Add($"{unidad.Nombre} termina con HP:{unidad.Hp}/{unidad.HpMax}");
            }

            if (sacrificaTodoHpUsuario)
            {
                actual.RecibirDano(actual.Hp);
                lines.Add($"{actual.Nombre} sacrifica todo su HP");
            }

            vistaJuego.ShowMessage(string.Join("\n", lines));
            return true;
        }

        public static bool ReviveEquipo(IVistaJuego view, Equipo propio, IUnidad actual, int powerPct, bool sacrificaTodoHpUsuario)
        {
            var objetivos = propio.Unidades
                                  .Where(u => u != null && !u.EstaViva)
                                  .OrderBy(u => propio.OrdenEnArchivoDe(u))
                                  .ToList();
            if (objetivos.Count == 0)
            {
                view.ShowMessage("No hay aliados muertos.");
                return false;
            }

            var lines = new List<string>();
            foreach (var u in objetivos)
            {
                int hpSet = Math.Max(0, (int)Math.Floor(u.HpMax * (powerPct / 100.0)));
                UnidadHelper.SetHp(u, hpSet);
                lines.Add($"{actual.Nombre} revive a {u.Nombre}");
                lines.Add($"{u.Nombre} recibe {hpSet} de HP");
                lines.Add($"{u.Nombre} termina con HP:{u.Hp}/{u.HpMax}");
            }

            if (sacrificaTodoHpUsuario)
            {
                actual.RecibirDano(actual.Hp);
                lines.Add($"{actual.Nombre} sacrifica todo su HP");
            }

            view.ShowMessage(string.Join("\n", lines));
            return true;
        }
        public static bool EfectoSolo(IVistaJuego view, Equipo propio, IUnidad actual, int powerPct, bool revive)
        {
            List<IUnidad> candidatos;
            if (!revive)
            {
                candidatos = propio.Unidades.Take(Equipo.ActiveSlots)
                              .Where(u => u != null && u.EstaViva).ToList();
                if (candidatos.Count == 0)
                {
                    view.ShowMessage("No hay aliados disponibles para curar.");
                    return false;
                }
            }
            else
            {
                candidatos = propio.Unidades
                    .Where(u => u != null && !u.EstaViva)
                    .OrderBy(u => propio.OrdenEnArchivoDe(u))
                    .ToList();

                if (candidatos.Count == 0)
                {
                    var menuVacio = Shin_Megami_Tensei_Model.Presentacion.FormateadorMenus.VistaDeObjetivos(candidatos);
                    view.ReadTarget(actual.Nombre, menuVacio);
                    return false;
                }
            }

            var menu = Shin_Megami_Tensei_Model.Presentacion.FormateadorMenus.VistaDeObjetivos(candidatos);
            int idx = view.ReadTarget(actual.Nombre, menu);
            if (idx == menu.CancelIndex + 1) return false;
            if (idx < 1 || idx > menu.Items.Count)
            {
                view.ShowMessage("Selección de objetivo inválida.");
                return false;
            }

            var objetivo = candidatos[idx - 1];

            if (revive)
            {
                int hpSet = Math.Max(0, (int)Math.Floor(objetivo.HpMax * (powerPct / 100.0)));
                UnidadHelper.SetHp(objetivo, hpSet);
                var lines = new List<string>
                {
                    $"{actual.Nombre} revive a {objetivo.Nombre}",
                    $"{objetivo.Nombre} recibe {hpSet} de HP",
                    $"{objetivo.Nombre} termina con HP:{objetivo.Hp}/{objetivo.HpMax}"
                };
                view.ShowMessage(string.Join("\n", lines));
            }
            else
            {
                int heal = Math.Max(0, (int)Math.Floor(objetivo.HpMax * (powerPct / 100.0)));
                UnidadHelper.CurarUnidad(objetivo, heal);
                string msg =
                    $"{actual.Nombre} cura a {objetivo.Nombre}\n" +
                    $"{objetivo.Nombre} recibe {heal} de HP\n" +
                    $"{objetivo.Nombre} termina con HP:{objetivo.Hp}/{objetivo.HpMax}";
                view.ShowMessage(msg);
            }
            return true;
        }
    }
}
