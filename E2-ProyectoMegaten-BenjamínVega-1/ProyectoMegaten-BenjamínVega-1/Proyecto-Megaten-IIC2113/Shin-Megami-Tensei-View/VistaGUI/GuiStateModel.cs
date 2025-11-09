using System;
using System.Collections.Generic;
using Shin_Megami_Tensei_Model.Presentacion.VistaModelos;

namespace Shin_Megami_Tensei_View.VistaGUI;

internal sealed class GuiStateModel
{
    private readonly Dictionary<string, HashSet<int>> _unitOwners = new(StringComparer.OrdinalIgnoreCase);

    public GuiStateModel()
    {
        Player1 = new GuiPlayerState();
        Player2 = new GuiPlayerState();
        Options = new List<string>();
        Order = new List<string>();
    }

    public GuiPlayerState Player1 { get; }
    public GuiPlayerState Player2 { get; }
    public List<string> Options { get; }
    public List<string> Order { get; }
    public int Turns { get; set; }
    public int BlinkingTurns { get; set; }

    public IEnumerable<string> GetKnownUnitNames() => _unitOwners.Keys;

    public IReadOnlyCollection<int> GetOwnersOf(string? unitName)
    {
        if (string.IsNullOrWhiteSpace(unitName))
        {
            return Array.Empty<int>();
        }

        return _unitOwners.TryGetValue(unitName.Trim(), out var owners) && owners.Count > 0
            ? owners
            : Array.Empty<int>();
    }

    public void UpdateKnownUnits(SnapshotTablero? snapshot)
    {
        _unitOwners.Clear();
        RegisterUnits(snapshot?.J1Slots, playerId: 1);
        RegisterUnits(snapshot?.J2Slots, playerId: 2);
    }

    public void ClearOptions() => Options.Clear();
    public void ClearOrder() => Order.Clear();

    private void RegisterUnits(IReadOnlyList<Slot>? slots, int playerId)
    {
        if (slots is null) return;

        foreach (var slot in slots)
        {
            var name = slot?.Nombre;
            if (string.IsNullOrWhiteSpace(name)) continue;

            var key = name.Trim();
            if (!_unitOwners.TryGetValue(key, out var owners))
            {
                owners = new HashSet<int>();
                _unitOwners[key] = owners;
            }

            owners.Add(playerId);
        }
    }
}

internal sealed class GuiPlayerState
{
    private readonly GuiUnitState?[] _board = new GuiUnitState?[4];
    private readonly List<GuiUnitState> _reserve = new();

    public GuiUnitState?[] Board => _board;
    public List<GuiUnitState> Reserve => _reserve;

    public void UpdateBoard(IReadOnlyList<Slot>? slots)
    {
        for (int i = 0; i < _board.Length; i++)
        {
            _board[i] = null;
        }

        if (slots is null) return;

        int len = System.Math.Min(slots.Count, _board.Length);
        for (int i = 0; i < len; i++)
        {
            var slot = slots[i];
            if (slot is null || slot.Nombre is null)
            {
                _board[i] = null;
                continue;
            }

            int hp = slot.Hp ?? 0;
            int mp = slot.Mp ?? 0;
            int maxHp = slot.HpMax ?? System.Math.Max(hp, 0);
            int maxMp = slot.MpMax ?? System.Math.Max(mp, 0);
            _board[i] = new GuiUnitState(slot.Nombre, hp, mp, maxHp, maxMp);
        }
    }

    public void SetReserve(IEnumerable<GuiUnitState>? units)
    {
        _reserve.Clear();
        if (units is null) return;
        _reserve.AddRange(units);
    }
}

internal sealed record GuiUnitState(string Name, int Hp, int Mp, int MaxHp, int MaxMp);
