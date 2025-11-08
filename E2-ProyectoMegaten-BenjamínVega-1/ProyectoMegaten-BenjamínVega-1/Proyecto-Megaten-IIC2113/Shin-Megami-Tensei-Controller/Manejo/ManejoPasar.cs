using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei_Model.Combate;
using Shin_Megami_Tensei_Model.ModelosEquipo;

namespace Shin_Megami_Tensei.Manejo;

public sealed class ManejoPasar : IManejoAccion
{
    private readonly IVistaJuego _vistaJuego;
    public ManejoPasar(IVistaJuego vistaJuego) => _vistaJuego = vistaJuego;

    public IManejoAccion.ActionResult Execute(Tablero tablero, IUnidad actual, bool esJ1, Turnos turnos)
    {
        if (turnos.Blinking > 0)
        {
            turnos.ConsumirBlinking(1);
            _vistaJuego.ShowTurnsConsumption(0, 1, 0);
        }
        else
        {
            turnos.ConsumirFull(1);
            turnos.AgregarBlinking(1);
            _vistaJuego.ShowTurnsConsumption(1, 0, 1);
        }

        return IManejoAccion.ActionResult.Continuar();
    }
}