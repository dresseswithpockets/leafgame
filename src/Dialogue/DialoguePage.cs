using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DialoguePage : Resource
{
    [Export] public string Mood;
    [Export] public float Speed;
    [Export] public float NoisePeriod;
    [Export(PropertyHint.MultilineText)] public string Text;

    [Export] public bool EnableCamera;
    [Export] public string CameraName;

    [Export] public bool StartAnimation;
    [Export] public string AnimationPlayerName;
    [Export] public string AnimationName;

    [Export] public bool AutoNext;
    [Export] public float AutoNextDelay;
}