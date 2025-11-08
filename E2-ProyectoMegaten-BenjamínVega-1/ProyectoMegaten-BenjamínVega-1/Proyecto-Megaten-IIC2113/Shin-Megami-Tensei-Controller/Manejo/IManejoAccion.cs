using Shin_Megami_Tensei_Model.Combate;
using Shin_Megami_Tensei_Model.ModelosEquipo;

namespace Shin_Megami_Tensei.Manejo;

public interface IManejoAccion
{
    ActionResult Execute(Tablero t, IUnidad actual, bool esJ1, Turnos turnos);

    public sealed record ActionResult(bool TerminoPartida, (string Nombre, string Tag)? Ganador)
    {
        public static ActionResult Continuar() => new(false, null);
        public static ActionResult Fin(string nombre, string tag) => new(true, (nombre, tag));
    }
}