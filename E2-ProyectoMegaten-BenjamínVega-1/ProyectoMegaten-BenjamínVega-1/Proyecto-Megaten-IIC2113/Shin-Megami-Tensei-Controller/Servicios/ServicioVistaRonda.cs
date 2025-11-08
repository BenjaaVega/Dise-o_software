using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei_Model.Combate;
using Shin_Megami_Tensei_Model.Presentacion;
using Shin_Megami_Tensei_Model.ModelosEquipo;
using Shin_Megami_Tensei.Manejo.Helpers;

namespace Shin_Megami_Tensei.Manejo.Services
{
    public sealed class ServicioVistaRonda
    {
        private readonly IVistaJuego _view;
        private readonly IBoardPresenter _presenter;

        public ServicioVistaRonda(IVistaJuego vistaJuego, IBoardPresenter presenter)
        {
            _view = vistaJuego;
            _presenter = presenter;
        }

        public void MostrarEncabezado(Tablero tablero, Equipo equipo, string tag)
        {
            _view.ShowRoundHeader(equipo.Samurai.Nombre, tag);
            _view.ShowBoard(_presenter.BoardOf(tablero));
        }

        public void MostrarTurnosYOrden(Turnos turnos, Equipo equipo, List<int> colaSlots)
        {
            _view.ShowTurns(_presenter.TurnsOf(turnos));
            _view.ShowOrder(ColaRondaHelper.OrdenComoTexto(equipo, colaSlots));
        }

        public void Refrescar(Tablero tablero, Turnos turnos, Equipo equipo, List<int> colaSlots)
        {
            _view.ShowBoard(_presenter.BoardOf(tablero));
            _view.ShowTurns(_presenter.TurnsOf(turnos));
            _view.ShowOrder(ColaRondaHelper.OrdenComoTexto(equipo, colaSlots));
        }
    }
}