using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei_Model.Combate;
using Shin_Megami_Tensei_Model.ModelosEquipo;
using Shin_Megami_Tensei.Manejo.Helpers;
using Shin_Megami_Tensei.Manejo.Services;

namespace Shin_Megami_Tensei.Manejo
{
    public sealed class ManejoDisparo : IManejoAccion
    {
        private readonly IVistaJuego _view;
        private readonly ICalcularDano _calc;

        public ManejoDisparo(IVistaJuego vistaJuego) : this(vistaJuego, new CalcularDano()) { }
        public ManejoDisparo(IVistaJuego vistaJuego, ICalcularDano calc)
        {
            _view = vistaJuego;
            _calc = calc;
        }

        public IManejoAccion.ActionResult Execute(Tablero tableor, IUnidad unidadActual, bool esJ1, Turnos turnos)
        {
            var enemigo = esJ1 ? tableor.J2 : tableor.J1;
            var objetivo = SelectorObjetivo.ElegirEnemigoActivo(_view, enemigo, unidadActual.Nombre);
            if (objetivo is null) return IManejoAccion.ActionResult.Continuar();

            double raw = _calc.DanoDisparoBase(unidadActual, objetivo);
            var outcome = RealizarDano.HacerAtaqueBasico(_view, unidadActual, objetivo, Elemento.Gun, raw);

            if (!objetivo.EstaViva && !objetivo.EsSamurai)
                enemigo.BajarMuertoDeActivosAReserva(objetivo);

            var (usedFull, usedBlink, gainedBlink) = TurnosHelper.DecidirConsumo(turnos, outcome);
            TurnosHelper.AplicarConsumo(turnos, usedFull, usedBlink, gainedBlink);
            _view.ShowTurnsConsumption(usedFull, usedBlink, gainedBlink);

            if (CombateHelper.EquipoRivalDerrotado(tableor, esJ1))
            {
                var (nombre, tag) = CombateHelper.NombreEquipo(tableor, esJ1);
                return IManejoAccion.ActionResult.Fin(nombre, tag);
            }
            return IManejoAccion.ActionResult.Continuar();
        }
    }
}