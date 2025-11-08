using Shin_Megami_Tensei_Model.ModelosEquipo;

namespace Shin_Megami_Tensei_Model.Combate
{
    public static class ReglasBatalla
    {
        public static bool EquipoActivoDerrotado(Equipo eq)
        {
            var activos = eq.Unidades.Take(Equipo.ActiveSlots);

            foreach (var unidad in activos)
            {
                if (unidad is null) continue;

                try
                {
                    var pHp = unidad.GetType().GetProperty("Hp");
                    if (pHp != null)
                    {
                        int hp = Convert.ToInt32(pHp.GetValue(unidad) ?? 0);
                        if (hp > 0) return false;
                        continue;
                    }
                    
                    if (!string.IsNullOrWhiteSpace(unidad.Nombre) && unidad.EstaViva)
                        return false;
                }
                catch
                {

                }
            }

            return true;
        }
    }
}