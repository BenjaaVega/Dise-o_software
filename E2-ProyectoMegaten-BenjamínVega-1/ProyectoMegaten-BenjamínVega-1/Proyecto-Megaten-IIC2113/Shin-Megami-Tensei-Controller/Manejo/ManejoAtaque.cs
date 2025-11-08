using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei_Model.Combate;
using Shin_Megami_Tensei_Model.ModelosEquipo;
using Shin_Megami_Tensei.Manejo.Helpers;
using Shin_Megami_Tensei.Manejo.Services;

namespace Shin_Megami_Tensei.Manejo
{
    public sealed class ManejoAtaque : IManejoAccion
    {
        private readonly IVistaJuego _view;
        private readonly ICalcularDano _calc;

        public ManejoAtaque(IVistaJuego vistaJuego) : this(vistaJuego, new CalcularDano()) { }
        public ManejoAtaque(IVistaJuego vistaJuego, ICalcularDano calc)
        {
            _view = vistaJuego;
            _calc = calc;
        }

        public IManejoAccion.ActionResult Execute(Tablero tablero, IUnidad actual, bool esJ1, Turnos turnos)
        {
            var enemigo = esJ1 ? tablero.J2 : tablero.J1;
            var objetivo = SelectorObjetivo.ElegirEnemigoActivo(_view, enemigo, actual.Nombre); 
            if (objetivo is null) return IManejoAccion.ActionResult.Continuar();

            double raw = _calc.DanoAtaqueBase(actual, objetivo);
            var outcome = RealizarDano.HacerAtaqueBasico(_view, actual, objetivo, Elemento.Phys, raw);

            if (!objetivo.EstaViva && !objetivo.EsSamurai)
                enemigo.BajarMuertoDeActivosAReserva(objetivo);

            var (usedFull, usedBlink, gainedBlink) = TurnosHelper.DecidirConsumo(turnos, outcome);
            TurnosHelper.AplicarConsumo(turnos, usedFull, usedBlink, gainedBlink);
            _view.ShowTurnsConsumption(usedFull, usedBlink, gainedBlink);

            if (CombateHelper.EquipoRivalDerrotado(tablero, esJ1))
            {
                var (nombre, tag) = CombateHelper.NombreEquipo(tablero, esJ1);
                return IManejoAccion.ActionResult.Fin(nombre, tag);
            }
            return IManejoAccion.ActionResult.Continuar();
        }
    }
}