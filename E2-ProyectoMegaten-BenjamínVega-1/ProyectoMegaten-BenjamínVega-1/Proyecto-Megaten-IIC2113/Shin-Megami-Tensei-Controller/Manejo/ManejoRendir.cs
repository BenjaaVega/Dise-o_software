using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei_Model.Combate;
using Shin_Megami_Tensei_Model.ModelosEquipo;

namespace Shin_Megami_Tensei.Manejo;

public sealed class ManejoRendir : IManejoAccion
{
    private readonly IVistaJuego _view;
    public ManejoRendir(IVistaJuego vistaJuego) => _view = vistaJuego;

    public IManejoAccion.ActionResult Execute(Tablero tablero, IUnidad actual, bool esJ1, Turnos turnos)
    {
        var tag = esJ1 ? "J1" : "J2";
        _view.ShowSurrender(actual.Nombre, tag);
        var ganador = esJ1 ? tablero.J2.Samurai.Nombre : tablero.J1.Samurai.Nombre;
        var ganadorTag = esJ1 ? "J2" : "J1";
        return IManejoAccion.ActionResult.Fin(ganador, ganadorTag);
    }
}