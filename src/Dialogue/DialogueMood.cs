using Godot;
using Godot.Collections;

public class DialogueMood : Resource
{
    [Export]
    public Array<AudioStream> Sounds;
    
    [Export]
    public float PitchLow;
    
    [Export]
    public float PitchHigh;
}