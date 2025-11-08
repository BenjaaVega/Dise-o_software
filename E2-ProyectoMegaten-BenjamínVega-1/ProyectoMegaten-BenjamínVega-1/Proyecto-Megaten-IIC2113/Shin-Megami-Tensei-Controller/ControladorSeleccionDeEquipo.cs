using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei_Model.Entrada;
using Shin_Megami_Tensei_Model.Construccion;
using Shin_Megami_Tensei_Model.Repositorios;
using Shin_Megami_Tensei_Model.Validacion;
using Shin_Megami_Tensei_Model.ModelosEquipo;

namespace Shin_Megami_Tensei.Controllers;
public sealed class ControladorSeleccionDeEquipo
{
    private readonly IVistaJuego _view;
    private readonly string _carpetaEquipos;
    private readonly IEquipoParser _parser;
    private readonly IConstructorEquipo _builder;
    private readonly ISkillsLookup _skills;

    public ControladorSeleccionDeEquipo(
        IVistaJuego view,
        string carpetaEquipos,
        IEquipoParser parser,
        IConstructorEquipo builder,
        ISkillsLookup skills)
    {
        _view = view;
        _carpetaEquipos = carpetaEquipos;
        _parser = parser;
        _builder = builder;
        _skills = skills;
    }

    public (Equipo j1, Equipo j2)? Run()
    {
        var archivosEquipos = Directory
            .GetFiles(_carpetaEquipos, "*.txt")
            .OrderBy(Path.GetFileName)
            .ToArray();

        _view.ShowTeamFiles(archivosEquipos.Select(Path.GetFileName).ToList());

        var indiceSeleccionado = _view.ReadFileIndex();

        if (archivosEquipos.Length == 0)
        {
            _view.ShowInvalidTeamsAndExit();
            return null;
        }

        if (indiceSeleccionado < 0 || indiceSeleccionado >= archivosEquipos.Length)
            indiceSeleccionado = 0;

        var rutaArchivoSeleccionado = archivosEquipos[indiceSeleccionado];

        var equiposCrudos = _parser.Parse(rutaArchivoSeleccionado);
        var validacionJugador1 = ValidacionEquipos.ValidarCrudos(equiposCrudos.Jugador1, _skills);
        var validacionJugador2 = ValidacionEquipos.ValidarCrudos(equiposCrudos.Jugador2, _skills);

        if (!validacionJugador1.EsValido || !validacionJugador2.EsValido)
        {
            _view.ShowInvalidTeamsAndExit();
            return null;
        }

        var equipoJugador1 = _builder.Construir(equiposCrudos.Jugador1);
        var equipoJugador2 = _builder.Construir(equiposCrudos.Jugador2);

        return (equipoJugador1, equipoJugador2);
    }
}
