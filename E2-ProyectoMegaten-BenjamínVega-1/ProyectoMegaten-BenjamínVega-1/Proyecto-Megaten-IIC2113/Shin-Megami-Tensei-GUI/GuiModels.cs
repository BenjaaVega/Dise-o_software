namespace Shin_Megami_Tensei_GUI;

public sealed class GuiUnit : IUnit
{
    public GuiUnit(string name, int hp, int mp, int maxHp, int maxMp)
    {
        Name = name;
        HP = hp;
        MP = mp;
        MaxHP = maxHp;
        MaxMP = maxMp;
    }

    public string Name { get; }
    public int HP { get; }
    public int MP { get; }
    public int MaxHP { get; }
    public int MaxMP { get; }
}

public sealed class GuiPlayer : IPlayer
{
    public GuiPlayer(IUnit?[] boardUnits, IEnumerable<IUnit> reserveUnits)
    {
        UnitsInBoard = boardUnits ?? Array.Empty<IUnit?>();
        UnitsInReserve = reserveUnits ?? Array.Empty<IUnit>();
    }

    public IUnit?[] UnitsInBoard { get; }
    public IEnumerable<IUnit> UnitsInReserve { get; }
}

public sealed class GuiState : IState
{
    public GuiState(IPlayer player1, IPlayer player2, IEnumerable<string> options, int turns, int blinkingTurns, IEnumerable<string> order)
    {
        Player1 = player1;
        Player2 = player2;
        Options = options ?? Array.Empty<string>();
        Turns = turns;
        BlinkingTurns = blinkingTurns;
        Order = order ?? Array.Empty<string>();
    }

    public IPlayer Player1 { get; }
    public IPlayer Player2 { get; }
    public IEnumerable<string> Options { get; }
    public int Turns { get; }
    public int BlinkingTurns { get; }
    public IEnumerable<string> Order { get; }
}

public sealed class TeamInfo : ITeamInfo
{
    public TeamInfo(string samuraiName, string[] skillNames, string[] demonNames)
    {
        SamuraiName = samuraiName;
        SkillNames = skillNames ?? Array.Empty<string>();
        DemonNames = demonNames ?? Array.Empty<string>();
    }

    public string SamuraiName { get; }
    public string[] SkillNames { get; }
    public string[] DemonNames { get; }
}
