using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei_Model.ModelosEquipo;
using Shin_Megami_Tensei_Model.Presentacion;

namespace Shin_Megami_Tensei.Manejo.Services
{
    public static class SelectorObjetivo
    {
        public static IUnidad? ElegirEnemigoActivo(IVistaJuego vistaJuego, Equipo enemigo, string atacanteNombre)
        {
            var objetivos = enemigo.Unidades
                                   .Take(Equipo.ActiveSlots)
                                   .Where(unidad => unidad != null && unidad.EstaViva)
                                   .ToList();
            if (objetivos.Count == 0)
            {
                vistaJuego.ShowMessage("No hay objetivos.");
                return null;
            }

            var menu = FormateadorMenus.VistaDeObjetivos(objetivos);
            int idx = vistaJuego.ReadTarget(atacanteNombre, menu);
            if (idx == menu.CancelIndex + 1) return null;
            if (idx < 1 || idx > menu.Items.Count)
            {
                vistaJuego.ShowMessage("Selección de objetivo inválida.");
                return null;
            }
            return objetivos[idx - 1];
        }
        
        public static IUnidad? ElegirDeReservas(IVistaJuego view, Equipo equipo, bool onlyAlive)
        {
            var reserva = equipo.Unidades
                                .Skip(Equipo.ActiveSlots)
                                .Where(unidad => unidad is not null && (!onlyAlive || unidad.EstaViva))
                                .OrderBy(unidades => equipo.OrdenEnArchivoDe(unidades))
                                .ToList();

            if (reserva.Count == 0)
            {
                var opcionesSoloCancelar = new List<string> { "1-Cancelar" };
                view.ReadCustom("Seleccione un monstruo para invocar", opcionesSoloCancelar);
                return null;
            }

            var opciones = new List<string>();
            for (int i = 0; i < reserva.Count; i++)
            {
                var u = reserva[i];
                opciones.Add($"{i + 1}-{u.Nombre} HP:{u.Hp}/{u.HpMax} MP:{u.Mp}/{u.MpMax}");
            }
            opciones.Add($"{reserva.Count + 1}-Cancelar");

            int sel = view.ReadCustom("Seleccione un monstruo para invocar", opciones);
            if (sel == reserva.Count + 1) return null;
            if (sel < 1 || sel > reserva.Count)
            {
                view.ShowMessage("Selección inválida.");
                return null;
            }

            return reserva[sel - 1];
        }
    }
}
