using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei_Model.Combate;
using Shin_Megami_Tensei_Model.ModelosEquipo;
using Shin_Megami_Tensei.Manejo.Helpers;
using Shin_Megami_Tensei.Manejo.Services;

namespace Shin_Megami_Tensei.Manejo
{
    public sealed class ManejoInvocar : IManejoAccion
    {
        private readonly IVistaJuego _vista;
        public ManejoInvocar(IVistaJuego vista) => _vista = vista;

        public IManejoAccion.ActionResult Execute(Tablero tablero, IUnidad unidadActual, bool esJ1, Turnos turnos)
        {
            var equipo = esJ1 ? tablero.J1 : tablero.J2;
            NormalizarMuertosActivosAReserva(equipo);

            var elegido = SelectorObjetivo.ElegirDeReservas(_vista, equipo, onlyAlive: true);
            if (elegido is null) return IManejoAccion.ActionResult.Continuar();

            if (unidadActual.EsSamurai)
            {
                var slot = SelectorPosicion.SlotActivoParaInvocar(_vista, equipo);
                if (slot is null) return IManejoAccion.ActionResult.Continuar();

                var ocupante = equipo.Unidades[slot.Value];
                if (ocupante is null || !ocupante.EstaViva)
                    equipo.MoverDesdeReserva(elegido, slot.Value);
                else
                    equipo.IntercambiarConReserva(elegido, slot.Value);

                int usarFull = 0, usarBlink = 0, ganarBlink = 0;
                if (turnos.Blinking > 0) { turnos.ConsumirBlinking(1); usarBlink = 1; }
                else { turnos.ConsumirFull(1); turnos.AgregarBlinking(1); usarFull = 1; ganarBlink = 1; }

                _vista.ShowMessage($"{elegido.Nombre} ha sido invocado");
                _vista.ShowTurnsConsumption(usarFull, usarBlink, ganarBlink);
            }
            else
            {
                equipo.Intercambiar(elegido, unidadActual);

                int usarFull = 0, usarBlink = 0, ganarBlink = 0;
                if (turnos.Blinking > 0) { turnos.ConsumirBlinking(1); usarBlink = 1; }
                else { turnos.ConsumirFull(1); turnos.AgregarBlinking(1); usarFull = 1; ganarBlink = 1; }

                _vista.ShowMessage($"{elegido.Nombre} ha sido invocado");
                _vista.ShowTurnsConsumption(usarFull, usarBlink, ganarBlink);
            }

            return IManejoAccion.ActionResult.Continuar();
        }

        public IManejoAccion.ActionResult ExecuteDesdeSabbatma(
            Tablero tablero, IUnidad unidadActual, bool esJ1, Turnos turnos, int costoMp, out bool ok)
        {
            ok = false;
            var equipo = esJ1 ? tablero.J1 : tablero.J2;
            NormalizarMuertosActivosAReserva(equipo);

            var invocado = SelectorObjetivo.ElegirDeReservas(_vista, equipo, onlyAlive: true);
            if (invocado is null) return IManejoAccion.ActionResult.Continuar();

            var slot = SelectorPosicion.SlotActivoParaInvocar(_vista, equipo);
            if (slot is null) return IManejoAccion.ActionResult.Continuar();

            var ocupante = equipo.Unidades[slot.Value];
            if (ocupante is null || !ocupante.EstaViva)
                equipo.MoverDesdeReserva(invocado, slot.Value);
            else
                equipo.IntercambiarConReserva(invocado, slot.Value);
            
            int usarFull = 0, usarBlink = 0;
            if (turnos.Blinking > 0) { turnos.ConsumirBlinking(1); usarBlink = 1; }
            else { turnos.ConsumirFull(1); usarFull = 1; }

            unidadActual.GastarMp(costoMp);

            _vista.ShowMessage($"{invocado.Nombre} ha sido invocado");
            _vista.ShowTurnsConsumption(usarFull, usarBlink, 0);

            ok = true;
            return IManejoAccion.ActionResult.Continuar();
        }

        public IManejoAccion.ActionResult ExecuteDesdeInvitation(
            Tablero tablero, IUnidad unidadActual, bool esJ1, Turnos turnos, int costoMp, out bool ok)
        {
            ok = false;
            var equipo = esJ1 ? tablero.J1 : tablero.J2;
            NormalizarMuertosActivosAReserva(equipo);

            var elegido = SelectorObjetivo.ElegirDeReservas(_vista, equipo, onlyAlive: false);
            if (elegido is null) return IManejoAccion.ActionResult.Continuar();

            var slot = SelectorPosicion.SlotActivoParaInvocar(_vista, equipo);
            if (slot is null) return IManejoAccion.ActionResult.Continuar();

            bool estabaMuerto = !elegido.EstaViva;

            var ocupante = equipo.Unidades[slot.Value];
            if (ocupante is null || !ocupante.EstaViva)
                equipo.MoverDesdeReserva(elegido, slot.Value);
            else
                equipo.IntercambiarConReserva(elegido, slot.Value);

            int usarFull = 0, usarBlink = 0;
            if (turnos.Blinking > 0) { turnos.ConsumirBlinking(1); usarBlink = 1; }
            else { turnos.ConsumirFull(1); usarFull = 1; }

            unidadActual.GastarMp(costoMp);

            if (estabaMuerto)
            {
                UnidadHelper.RevivirFull(elegido);
                var lineas = new List<string>
                {
                    $"{elegido.Nombre} ha sido invocado",
                    $"{unidadActual.Nombre} revive a {elegido.Nombre}",
                    $"{elegido.Nombre} recibe {elegido.HpMax} de HP",
                    $"{elegido.Nombre} termina con HP:{elegido.Hp}/{elegido.HpMax}"
                };
                _vista.ShowMessage(string.Join("\n", lineas));
            }
            else
            {
                _vista.ShowMessage($"{elegido.Nombre} ha sido invocado");
            }

            _vista.ShowTurnsConsumption(usarFull, usarBlink, 0);

            ok = true;
            return IManejoAccion.ActionResult.Continuar();
        }

        private static void NormalizarMuertosActivosAReserva(Equipo equipo)
        {
            int maxActivos = System.Math.Min(Equipo.ActiveSlots, equipo.Unidades.Count);
            for (int indice = 0; indice < maxActivos; indice++)
            {
                var unidad = equipo.Unidades[indice];
                if (unidad is null) continue;
                if (!unidad.EsSamurai && !unidad.EstaViva)
                    equipo.BajarMuertoDeActivosAReserva(unidad);
            }
        }
    }
}
