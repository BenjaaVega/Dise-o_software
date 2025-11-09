namespace Shin_Megami_Tensei_GUI;

public interface IPlayer
{
    IUnit?[] UnitsInBoard { get; }
    IEnumerable<IUnit> UnitsInReserve { get; }
}
