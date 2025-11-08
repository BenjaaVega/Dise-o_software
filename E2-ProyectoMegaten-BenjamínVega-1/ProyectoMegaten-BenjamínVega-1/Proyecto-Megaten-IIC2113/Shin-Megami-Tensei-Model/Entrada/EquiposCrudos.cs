namespace Shin_Megami_Tensei_Model.Entrada;

public sealed record EquiposCrudos(
    IReadOnlyList<IntentoUnidad> Jugador1,
    IReadOnlyList<IntentoUnidad> Jugador2
);