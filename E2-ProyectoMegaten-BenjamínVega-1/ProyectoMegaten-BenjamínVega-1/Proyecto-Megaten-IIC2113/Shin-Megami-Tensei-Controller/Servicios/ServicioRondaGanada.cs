using Shin_Megami_Tensei_Model.Combate;

namespace Shin_Megami_Tensei.Manejo.Services
{
    public static class ServicioRondaGanada
    {
        public static (string Nombre, string Tag)? Detectar(Tablero tablero)
        {
            bool j1Derrotado = ReglasBatalla.EquipoActivoDerrotado(tablero.J1);
            bool j2Derrotado = ReglasBatalla.EquipoActivoDerrotado(tablero.J2);

            if (j1Derrotado && j2Derrotado)
                return (tablero.J2.Samurai.Nombre, "J2");

            if (j1Derrotado) return (tablero.J2.Samurai.Nombre, "J2");
            if (j2Derrotado) return (tablero.J1.Samurai.Nombre, "J1");
            return null;
        }
    }
}