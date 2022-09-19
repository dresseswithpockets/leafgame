
using Godot;
using Godot.Collections;

public class DialogueCharacter : Resource
{
    [Export]
    public string Name;

    [Export]
    public Dictionary<string, DialogueMood> Moods;
}