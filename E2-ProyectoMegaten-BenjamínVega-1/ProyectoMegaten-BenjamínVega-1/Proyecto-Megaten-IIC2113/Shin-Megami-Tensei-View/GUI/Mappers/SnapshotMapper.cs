using System;
using Shin_Megami_Tensei_Model.Presentacion.VistaModelos;
using Shin_Megami_Tensei_View.GUI.Adapters;

namespace Shin_Megami_Tensei_View.GUI.Mappers;

internal static class SnapshotMapper
{
    public static void ApplyBoard(GuiState state, SnapshotTablero snapshot)
    {
        if (state is null) throw new ArgumentNullException(nameof(state));
        if (snapshot is null) return;

        ApplyPlayer(state.Player1Adapter, snapshot.J1Nombre, snapshot.J1Slots);
        ApplyPlayer(state.Player2Adapter, snapshot.J2Nombre, snapshot.J2Slots);
    }

    public static void ApplyTurns(GuiState state, SnapshotTurno snapshot)
    {
        if (state is null) throw new ArgumentNullException(nameof(state));
        if (snapshot is null) return;

        state.SetTurns(snapshot.Full, snapshot.Blinking);
    }

    private static void ApplyPlayer(GuiPlayer player, string? name, IReadOnlyList<Slot> slots)
    {
        player.SetName(name);

        player.SetReserve(Array.Empty<Slot>());
        for (int i = 0; i < player.BoardSize; i++)
            player.SetBoardSlot(i, null);

        if (slots is null)
            return;

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (i < player.BoardSize)
            {
                player.SetBoardSlot(i, slot);
            }
            else
            {
                player.AppendReserve(slot);
            }
        }
    }
}
