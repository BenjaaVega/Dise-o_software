using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei_Model.Combate;
using Shin_Megami_Tensei_Model.ModelosEquipo;
using Shin_Megami_Tensei_Model.Presentacion.ViewModels;
using Shin_Megami_Tensei.Manejo;

namespace Shin_Megami_Tensei.Controllers;

public sealed class ControladorAccion
{
    private readonly IVistaJuego _view;
    private readonly IManejoAccion _attack, _shoot, _skill, _invoke, _pass, _surrender;
    
    private readonly Dictionary<int, IManejoAccion> _mapSamurai;
    private readonly Dictionary<int, IManejoAccion> _mapNoSamurai;

    public ControladorAccion(
        IVistaJuego view,
        IManejoAccion attack,
        IManejoAccion shoot,
        IManejoAccion skill,
        IManejoAccion invoke,
        IManejoAccion pass,
        IManejoAccion surrender)
    {
        _view = view;
        _attack = attack;
        _shoot = shoot;
        _skill = skill;
        _invoke = invoke;
        _pass = pass;
        _surrender = surrender;

        _mapSamurai = new Dictionary<int, IManejoAccion>
        {
            [1] = _attack,
            [2] = _shoot,
            [3] = _skill,
            [4] = _invoke,
            [5] = _pass,
            [6] = _surrender
        };

        _mapNoSamurai = new Dictionary<int, IManejoAccion>
        {
            [1] = _attack,
            [2] = _skill,
            [3] = _invoke,
            [4] = _pass
        };
    }

    public IManejoAccion.ActionResult Run(Tablero tablero, IUnidad unidadActual, bool esJugador1, Turnos turnos)
    {
        var menu = MenuAcciones.For(unidadActual);
        var opcionSeleccionada = _view.ReadActionFor(unidadActual.Nombre, menu);

        var mapa = unidadActual.EsSamurai ? _mapSamurai : _mapNoSamurai;

        if (mapa.TryGetValue(opcionSeleccionada, out var manejo))
            return manejo.Execute(tablero, unidadActual, esJugador1, turnos);

        return IManejoAccion.ActionResult.Continuar();
    }
}