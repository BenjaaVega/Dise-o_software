namespace Shin_Megami_Tensei_GUI;

public interface IUnit
{
    string Name { get; }
    int HP { get; }
    int MP { get; }
    int MaxHP { get; }
    int MaxMP { get; }
}
