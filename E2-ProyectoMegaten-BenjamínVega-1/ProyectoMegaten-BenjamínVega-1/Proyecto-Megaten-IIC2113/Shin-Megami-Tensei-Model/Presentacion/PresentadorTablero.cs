using Shin_Megami_Tensei_Model.Combate;
using Shin_Megami_Tensei_Model.ModelosEquipo;
using Shin_Megami_Tensei_Model.Presentacion.VistaModelos;

namespace Shin_Megami_Tensei_Model.Presentacion;

public sealed class PresentadorTablero : IBoardPresenter
{
    public SnapshotTablero BoardOf(Tablero tablero) => new(
        tablero.J1.Samurai.Nombre, SlotsOf(tablero.J1.Unidades),
        tablero.J2.Samurai.Nombre, SlotsOf(tablero.J2.Unidades)
    );

    public SnapshotTurno TurnsOf(Turnos turnos) => new(turnos.Full, turnos.Blinking);

    private static IReadOnlyList<Slot> SlotsOf(IReadOnlyList<IUnidad> unidades) => new[]
    {
        CrearSlot("A", unidades.ElementAtOrDefault(0)), 
        CrearSlot("B", unidades.ElementAtOrDefault(1)),
        CrearSlot("C", unidades.ElementAtOrDefault(2)),
        CrearSlot("D", unidades.ElementAtOrDefault(3)),
    };

    private static Slot CrearSlot(string letra, IUnidad? unidad)
    {
        if (unidad is null) return new Slot(letra, null, null, null, null, null);
        
        if (!unidad.EsSamurai && !unidad.EstaViva)
            return new Slot(letra, null, null, null, null, null);
        
        var hp = Clamp(unidad.Hp, 0, unidad.HpMax);
        var mp = Clamp(unidad.Mp, 0, unidad.MpMax);

        var nombreSeguro = string.IsNullOrWhiteSpace(unidad.Nombre) ? "?" : unidad.Nombre;
        return new Slot(letra, nombreSeguro, hp, unidad.HpMax, mp, unidad.MpMax);
    }

    private static int Clamp(int value, int min, int max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}
