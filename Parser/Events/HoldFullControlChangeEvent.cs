namespace Parser.Events;

public class HoldFullControlChangeEvent : EventArgs
{
    public HoldFullControlChangeEvent(bool holdFullControl)
    {
        HoldFullControl = holdFullControl;
    }

    public bool HoldFullControl {  get; init; }
}
