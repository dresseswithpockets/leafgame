using Godot;
using System;

[Tool]
public class Fader : ColorRect
{
    [Export]
    public float Alpha
    {
        get => Color.a;
        set
        {
            var c = Color;
            c.a = value;
            Color = c;
        }
    }
}
