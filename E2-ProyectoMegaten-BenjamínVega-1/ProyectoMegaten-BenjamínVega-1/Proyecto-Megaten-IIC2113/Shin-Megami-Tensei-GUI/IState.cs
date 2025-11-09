namespace Shin_Megami_Tensei_GUI;

public interface IState
{
    IPlayer Player1 { get; }
    IPlayer Player2 { get; }
    IEnumerable<string> Options { get; }
    int Turns { get; }
    int BlinkingTurns { get; }
    IEnumerable<string> Order { get; }
}
