using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei_Model.Combate;
using Shin_Megami_Tensei_Model.Presentacion;
using Shin_Megami_Tensei.Manejo.Helpers;
using Shin_Megami_Tensei.Manejo.Services;

namespace Shin_Megami_Tensei.Controllers
{
    public sealed class ControladorRonda
    {
        private readonly IVistaJuego _view;
        private readonly IBoardPresenter _presenter;
        private readonly ControladorAccion _controladorAccion;

        private readonly ServicioTurnoRonda _turnSvc;
        private readonly ServicioVistaRonda _roundView;

        public ControladorRonda(IVistaJuego vistaJuego, IBoardPresenter presentador, ControladorAccion controladorAccion)
        {
            _view = vistaJuego;
            _presenter = presentador;
            _controladorAccion = controladorAccion;

            _turnSvc = new ServicioTurnoRonda();
            _roundView = new ServicioVistaRonda(_view, _presenter);
        }

        public (string Nombre, string Tag)? Play(Tablero tablero, bool esJugador1)
        {
            var equipoPropio = esJugador1 ? tablero.J1 : tablero.J2;
            var tag = esJugador1 ? "J1" : "J2";

            _roundView.MostrarEncabezado(tablero, equipoPropio, tag);

            var turnos = _turnSvc.CrearTurnosIniciales(equipoPropio);
            var colaSlots = ColaRondaHelper.ConstruirColaSlotsAlInicio(equipoPropio);

            _roundView.MostrarTurnosYOrden(turnos, equipoPropio, colaSlots);

            while (turnos.HayTurnos && colaSlots.Count > 0)
            {
                int? idxSlotActual = ColaRondaHelper.SiguienteSlotConVivo(equipoPropio, colaSlots);
                if (idxSlotActual is null) break;

                var unidadActual = ColaRondaHelper.SafeUnidad(equipoPropio, idxSlotActual.Value);
                if (unidadActual is null || !unidadActual.EstaViva)
                {
                    ColaRondaHelper.RotarCola(colaSlots);
                    colaSlots.RemoveAll(slot => !ColaRondaHelper.SlotTieneVivo(equipoPropio, slot));
                    continue;
                }

                var (fullAntes, blinkingAntes) = _turnSvc.Snapshot(turnos);

                var resultado = _controladorAccion.Run(tablero, unidadActual, esJugador1, turnos);
                if (resultado.TerminoPartida) return resultado.Ganador;

                var finDirecto = ServicioRondaGanada.Detectar(tablero);
                if (finDirecto is not null) return finDirecto;

                bool consumioTurno = _turnSvc.HuboConsumo(turnos, fullAntes, blinkingAntes);
                if (consumioTurno)
                {
                    ColaRondaHelper.RotarCola(colaSlots);
                    colaSlots.RemoveAll(slot => !ColaRondaHelper.SlotTieneVivo(equipoPropio, slot));
                    ColaRondaHelper.InsertarNuevosSlotsAntesDelActor(equipoPropio, colaSlots, idxSlotActual.Value);

                    if (turnos.HayTurnos && colaSlots.Count > 0)
                        _roundView.Refrescar(tablero, turnos, equipoPropio, colaSlots);
                }
            }

            return null;
        }
    }
}
