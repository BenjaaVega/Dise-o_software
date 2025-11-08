using Shin_Megami_Tensei_Model.ModelosEquipo;

namespace Shin_Megami_Tensei.Manejo.Helpers
{
    public static class ColaRondaHelper
    {
        public static IUnidad? SafeUnidad(Equipo equipo, int slot) =>
            (slot >= 0 && slot < equipo.Unidades.Count) ? equipo.Unidades[slot] : null;

        public static List<int> ConstruirColaSlotsAlInicio(Equipo equipo) =>
            Enumerable.Range(0, Equipo.ActiveSlots)
                      .Select(slot => new { slot, unidad = SafeUnidad(equipo, slot) })
                      .Where(par => par.unidad is not null && par.unidad.EstaViva)
                      .OrderByDescending(par => par.unidad!.Stats.Spd)
                      .ThenBy(par => par.slot)
                      .Select(par => par.slot)
                      .ToList();

        public static int? SiguienteSlotConVivo(Equipo equipo, List<int> colaSlots)
        {
            foreach (var slot in colaSlots)
            {
                var unidad = SafeUnidad(equipo, slot);
                if (unidad is not null && unidad.EstaViva) return slot;
            }
            return null;
        }

        public static bool SlotTieneVivo(Equipo equipo, int slot)
        {
            var unidad = SafeUnidad(equipo, slot);
            return unidad is not null && unidad.EstaViva;
        }

        public static void RotarCola(List<int> colaSlots)
        {
            if (colaSlots.Count == 0) return;
            int first = colaSlots[0];
            colaSlots.RemoveAt(0);
            colaSlots.Add(first);
        }

        public static void InsertarNuevosSlotsAntesDelActor(Equipo equipo, List<int> colaSlots, int slotActorQueActuo)
        {
            int posActor = colaSlots.LastIndexOf(slotActorQueActuo);
            if (posActor < 0) posActor = colaSlots.Count;

            var nuevos = new List<int>();
            for (int slot = 0; slot < Equipo.ActiveSlots; slot++)
            {
                var unidad = SafeUnidad(equipo, slot);
                if (unidad is not null && unidad.EstaViva && !colaSlots.Contains(slot))
                    nuevos.Add(slot);
            }

            if (nuevos.Count == 0) return;

            nuevos.Sort();

            int insertPos = (posActor >= 0 && posActor <= colaSlots.Count) ? posActor : colaSlots.Count;
            foreach (var slotAInsertar in nuevos)
            {
                colaSlots.Insert(insertPos, slotAInsertar);
                insertPos++;
            }
        }

        public static List<string> OrdenComoTexto(Equipo equipo, List<int> colaSlots)
        {
            var nombres = new List<string>();
            for (int indice = 0; indice < colaSlots.Count; indice++)
            {
                var slot = colaSlots[indice];
                var unidad = SafeUnidad(equipo, slot);
                var nombre = (unidad is not null && unidad.EstaViva) ? unidad.Nombre : "";
                nombres.Add($"{indice + 1}-{nombre}");
            }
            return nombres;
        }
    }
}
