using Shin_Megami_Tensei_GUI;

namespace Shin_Megami_Tensei_View.GUI.Adapters;

public sealed class GuiUnit : IUnit
{
    public GuiUnit(string name, int hp, int maxHp, int mp, int maxMp)
    {
        Name = name ?? string.Empty;
        HP = hp;
        MaxHP = maxHp;
        MP = mp;
        MaxMP = maxMp;
    }

    public string Name { get; }
    public int HP { get; internal set; }
    public int MP { get; internal set; }
    public int MaxHP { get; }
    public int MaxMP { get; }
}
