using Shin_Megami_Tensei_Model.Combate;

namespace Shin_Megami_Tensei.Manejo.Helpers
{
    public static class CombateHelper
    {
        public static bool EquipoRivalDerrotado(Tablero tablero, bool esJ1)
        {
            var rival = esJ1 ? tablero.J2 : tablero.J1;

            bool porFlag = rival.Unidades.Where(unidad => unidad != null).All(unidad => !unidad.EstaViva);
            if (porFlag) return true;

            foreach (var unidad in rival.Unidades)
            {
                if (unidad == null) continue;
                if (unidad.Hp > 0) return false;
            }
            return true;
        }

        public static (string nombre, string tag) NombreEquipo(Tablero t, bool esJ1)
        {
            var propio = esJ1 ? t.J1 : t.J2;
            string nombre = propio.Unidades.FirstOrDefault(u => u != null)?.Nombre ?? (esJ1 ? "J1" : "J2");
            string tag = esJ1 ? "J1" : "J2";
            return (nombre, tag);
        }
    }
}