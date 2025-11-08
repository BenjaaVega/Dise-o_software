namespace Shin_Megami_Tensei_Model.Combate;

public readonly struct EfectoTruno
{
    public int ConsumedFull { get; }
    public int ConsumedBlinking { get; }
    public int GainedBlinking { get; }

    public EfectoTruno(int consumedFull, int consumedBlinking, int gainedBlinking)
    {
        ConsumedFull = consumedFull;
        ConsumedBlinking = consumedBlinking;
        GainedBlinking = gainedBlinking;
    }

    public static EfectoTruno None() => new(0,0,0);
    public static EfectoTruno ConsumeFull() => new(1,0,0);
    public static EfectoTruno ConsumeBlinking() => new(0,1,0);
    public static EfectoTruno GainBlinking() => new(0,0,1);
}