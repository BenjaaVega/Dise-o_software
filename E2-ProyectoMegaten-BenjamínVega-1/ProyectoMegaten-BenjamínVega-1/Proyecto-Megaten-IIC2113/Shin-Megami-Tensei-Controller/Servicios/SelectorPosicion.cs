using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei_Model.ModelosEquipo;

namespace Shin_Megami_Tensei.Manejo.Services
{
    public static class SelectorPosicion
    {
        public static int? SlotActivoParaInvocar(IVistaJuego vistaJuego, Equipo equipo)
        {
            var opciones = new List<string>();
            var indices  = new List<int>();

            for (int pos = 1; pos < Equipo.ActiveSlots; pos++)
            {
                indices.Add(pos);
                var unidad = UnidadASalvo(equipo, pos);
                if (unidad is null || !unidad.EstaViva)
                    opciones.Add($"{indices.Count}-Vacío (Puesto {pos + 1})");
                else
                    opciones.Add($"{indices.Count}-{unidad.Nombre} HP:{unidad.Hp}/{unidad.HpMax} MP:{unidad.Mp}/{unidad.MpMax} (Puesto {pos + 1})");
            }
            opciones.Add($"{indices.Count + 1}-Cancelar");

            int sel = vistaJuego.ReadCustom("Seleccione una posición para invocar", opciones);
            if (sel == indices.Count + 1) return null;
            if (sel < 1 || sel > indices.Count)
            {
                vistaJuego.ShowMessage("Selección inválida.");
                return null;
            }
            return indices[sel - 1];
        }

        private static IUnidad? UnidadASalvo(Equipo equipo, int slot) =>
            (slot >= 0 && slot < equipo.Unidades.Count) ? equipo.Unidades[slot] : null;
    }
}