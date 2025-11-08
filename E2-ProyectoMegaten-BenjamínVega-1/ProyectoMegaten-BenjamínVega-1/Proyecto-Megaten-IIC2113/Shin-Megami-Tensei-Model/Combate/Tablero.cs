using Shin_Megami_Tensei_Model.ModelosEquipo;

namespace Shin_Megami_Tensei_Model.Combate
{
    public sealed class Tablero
    {
        public Equipo J1 { get; }
        public Equipo J2 { get; }

        public Tablero(Equipo j1, Equipo j2) { J1 = j1; J2 = j2; }

        public Equipo Propio(bool esJ1) => esJ1 ? J1 : J2;
        public Equipo Rival(bool esJ1)  => esJ1 ? J2 : J1;

        public IReadOnlyList<IUnidad> ActivasAD(Equipo e) =>
            e.Unidades.Where(u => u is not null).Take(Equipo.ActiveSlots).ToList();
    }
}