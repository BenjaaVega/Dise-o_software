using System;
using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei_View.VistaGUI;
using Shin_Megami_Tensei_Model.Combate;
using Shin_Megami_Tensei_Model.Construccion;
using Shin_Megami_Tensei_Model.Entrada;
using Shin_Megami_Tensei_Model.ModelosEquipo;
using Shin_Megami_Tensei_Model.Presentacion;
using Shin_Megami_Tensei_Model.Repositorios;
using Shin_Megami_Tensei.Controllers;
using Shin_Megami_Tensei.Manejo;

namespace Shin_Megami_Tensei;

public class Game
{
    private readonly IVistaJuego _view;
    private readonly string _teamsFolder;

    public Game(View view, string teamsFolder)
        : this(new VistaJuegoGui(view), teamsFolder)
    {
    }

    public Game(IVistaJuego vistaJuego, string teamsFolder)
    {
        _view = vistaJuego ?? throw new ArgumentNullException(nameof(vistaJuego));
        _teamsFolder = teamsFolder;
    }

    public void Play()
    {
        if (_view is VistaJuegoGui gui)
        {
            gui.Run(PlayCore);
            return;
        }

        PlayCore();
    }

    private void PlayCore()
    {
        var parser                 = new ParserArchivoEquipos();
        var repositorioHabilidades = new JsonSkills();
        var repositorioUnidades    = new JsonUnidades();
        var constructorEquipo      = new ContructorEquipo(repositorioUnidades);
        var presentadorTablero     = new PresentadorTablero();
        var calculadorDano         = new CalcularDano();

        var equipos = SeleccionarEquipos(parser, constructorEquipo, repositorioHabilidades);
        if (equipos is null) return;
        var (j1, j2) = equipos.Value;

        var controladorAccion = CrearControladorAccion(calculadorDano, repositorioHabilidades);

        var controladorRonda   = new ControladorRonda(_view, presentadorTablero, controladorAccion);
        var controladorBatalla = new ControladorBatalla(_view, controladorRonda);

        controladorBatalla.Run(j1, j2);
    }

    private (Equipo j1, Equipo j2)? SeleccionarEquipos(
        IEquipoParser parser,
        ContructorEquipo constructorEquipo,
        ISkillsLookup repositorioHabilidades)
    {
        var selector = new ControladorSeleccionDeEquipo(
            view: _view,
            carpetaEquipos: _teamsFolder,
            parser: parser,
            builder: constructorEquipo,
            skills: repositorioHabilidades
        );

        return selector.Run();
    }

    private ControladorAccion CrearControladorAccion(CalcularDano calculadorDano, ISkillsLookup repositorioHabilidades)
    {
        IManejoAccion ataque  = new ManejoAtaque(_view);
        IManejoAccion disparo = new ManejoDisparo(_view);
        var invocar           = new ManejoInvocar(_view); 
        IManejoAccion habilidad = new ManejoHabilidad(_view, repositorioHabilidades, invocar);
        IManejoAccion pasar     = new ManejoPasar(_view);
        IManejoAccion rendir    = new ManejoRendir(_view);

        return new ControladorAccion(
            view: _view,
            attack: ataque,
            shoot: disparo,
            skill: habilidad,
            invoke: invocar,
            pass: pasar,
            surrender: rendir
        );
    }
}
