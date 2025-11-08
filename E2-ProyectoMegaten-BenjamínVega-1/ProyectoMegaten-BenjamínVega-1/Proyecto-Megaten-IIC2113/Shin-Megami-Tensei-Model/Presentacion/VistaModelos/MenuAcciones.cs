using Shin_Megami_Tensei_Model.ModelosEquipo;

namespace Shin_Megami_Tensei_Model.Presentacion.ViewModels;

public sealed class MenuAcciones
{
    private static readonly string[] OpcionesSamurai =
        { "1: Atacar", "2: Disparar", "3: Usar Habilidad", "4: Invocar", "5: Pasar Turno", "6: Rendirse" };

    private static readonly string[] OpcionesNoSamurai =
        { "1: Atacar", "2: Usar Habilidad", "3: Invocar", "4: Pasar Turno" };

    public IReadOnlyList<string> Opciones { get; }

    private MenuAcciones(IEnumerable<string> opciones) =>
        Opciones = new List<string>(opciones);

    public static MenuAcciones For(IUnidad unidad) =>
        unidad.EsSamurai
            ? new MenuAcciones(OpcionesSamurai)
            : new MenuAcciones(OpcionesNoSamurai);
}