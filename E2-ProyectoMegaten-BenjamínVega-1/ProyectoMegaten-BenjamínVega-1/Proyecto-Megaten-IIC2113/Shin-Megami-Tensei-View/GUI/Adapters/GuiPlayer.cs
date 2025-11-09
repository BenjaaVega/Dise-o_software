using System.Collections.Generic;
using Shin_Megami_Tensei_GUI;
using Shin_Megami_Tensei_Model.ModelosEquipo;
using Shin_Megami_Tensei_Model.Presentacion.VistaModelos;

namespace Shin_Megami_Tensei_View.GUI.Adapters;

public sealed class GuiPlayer : IPlayer
{
    private readonly GuiUnit?[] _board;
    private readonly List<GuiUnit> _reserve = new();

    public GuiPlayer()
    {
        _board = new GuiUnit?[Equipo.ActiveSlots];
    }

    public string Name { get; private set; } = string.Empty;

    public IUnit?[] UnitsInBoard => _board;

    public IEnumerable<IUnit> UnitsInReserve => _reserve;

    internal int BoardSize => _board.Length;

    public void SetName(string? name) => Name = name ?? string.Empty;

    public void SetBoardSlot(int index, Slot? slot)
    {
        if (index < 0 || index >= _board.Length) return;
        if (slot is null || slot.Nombre is null)
        {
            _board[index] = null;
            return;
        }

        var unit = new GuiUnit(slot.Nombre, slot.Hp ?? 0, slot.HpMax ?? 0, slot.Mp ?? 0, slot.MpMax ?? 0);
        _board[index] = unit;
    }

    public void SetReserve(IEnumerable<Slot> reserveSlots)
    {
        _reserve.Clear();
        if (reserveSlots is null) return;
        foreach (var slot in reserveSlots)
        {
            if (slot?.Nombre is null) continue;
            var unit = new GuiUnit(slot.Nombre, slot.Hp ?? 0, slot.HpMax ?? 0, slot.Mp ?? 0, slot.MpMax ?? 0);
            _reserve.Add(unit);
        }
    }

    public void AppendReserve(Slot? slot)
    {
        if (slot?.Nombre is null) return;
        var unit = new GuiUnit(slot.Nombre, slot.Hp ?? 0, slot.HpMax ?? 0, slot.Mp ?? 0, slot.MpMax ?? 0);
        _reserve.Add(unit);
    }
}
