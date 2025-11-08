using Shin_Megami_Tensei_Model.Combate;
using Shin_Megami_Tensei_Model.ModelosEquipo;
using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Controllers;

namespace Shin_Megami_Tensei;

public sealed class ControladorBatalla
{
    private readonly IVistaJuego _view;
    private readonly ControladorRonda _controladorRonda;

    public ControladorBatalla(IVistaJuego vistaJuego, ControladorRonda controladorRonda)
    {
        _view = vistaJuego;
        _controladorRonda = controladorRonda;
    }

    public void Run(Equipo equipoJugador1, Equipo equipoJugador2)
    {
        var tablero = new Tablero(equipoJugador1, equipoJugador2);
        
        while (true)
        {
            var ganador = _controladorRonda.Play(tablero, true);  
            if (ganador is not null)
            {
                _view.ShowWinner(ganador.Value.Nombre, ganador.Value.Tag);
                return;
            }

            ganador = _controladorRonda.Play(tablero, false);     
            if (ganador is not null)
            {
                _view.ShowWinner(ganador.Value.Nombre, ganador.Value.Tag);
                return;
            }
        }
    }
}