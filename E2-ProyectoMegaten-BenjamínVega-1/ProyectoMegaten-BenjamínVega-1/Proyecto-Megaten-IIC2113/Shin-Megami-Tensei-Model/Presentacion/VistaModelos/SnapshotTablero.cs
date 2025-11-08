namespace Shin_Megami_Tensei_Model.Presentacion.VistaModelos;

public sealed record SnapshotTablero(
    string J1Nombre, IReadOnlyList<Slot> J1Slots,
    string J2Nombre, IReadOnlyList<Slot> J2Slots);