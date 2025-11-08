using Shin_Megami_Tensei_Model.Combate;
using Shin_Megami_Tensei_Model.ModelosEquipo;

namespace Shin_Megami_Tensei.Manejo.Services
{
    public sealed class ServicioTurnoRonda
    {
        public Turnos CrearTurnosIniciales(Equipo equipo)
        {
            var activosVivos = equipo.Unidades.Take(Equipo.ActiveSlots).Count(unidad => unidad?.EstaViva == true);
            return Turnos.DesdeActivos(activosVivos);
        }

        public (int fullAntes, int blinkingAntes) Snapshot(Turnos tablero) => (tablero.Full, tablero.Blinking);

        public bool HuboConsumo(Turnos turnos, int fullAntes, int blinkingAntes) =>
            turnos.Full != fullAntes || turnos.Blinking != blinkingAntes;
    }
}