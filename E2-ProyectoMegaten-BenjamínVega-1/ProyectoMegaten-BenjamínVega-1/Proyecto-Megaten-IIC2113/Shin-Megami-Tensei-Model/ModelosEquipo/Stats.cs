namespace Shin_Megami_Tensei_Model.ModelosEquipo;
public sealed record Stats(int Hp, int Mp, int Str, int Skl, int Mag, int Spd, int Lck)
{
    public int HP        => Hp;
    public int MP        => Mp;
    public int Strength  => Str;
    public int Skill     => Skl;
    public int Magic     => Mag;
    public int Speed     => Spd;
    public int Luck      => Lck;
}