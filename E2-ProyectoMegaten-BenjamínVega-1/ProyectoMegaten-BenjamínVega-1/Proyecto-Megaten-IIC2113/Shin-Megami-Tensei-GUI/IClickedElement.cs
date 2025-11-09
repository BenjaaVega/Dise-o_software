namespace Shin_Megami_Tensei_GUI;

public interface IClickedElement
{
    ClickedElementType Type { get; }
    string Text { get; }
    int? PlayerId { get; }
}
