using Godot;
using System;

public class PlayerLeafWind : AudioStreamPlayer, IPlayerEvents
{
    [Export(PropertyHint.Range, "0.0,1.0")]
    public float Low = 0f;

    [Export(PropertyHint.Range, "0.0,1.0")]
    public float High = 1f;

    [Export(PropertyHint.Range, "0.0,1.0")]
    public float LerpT = 1f;

    public void GroundSpeed(float speed, float maxSpeed)
    {
        var frac = Mathf.Clamp(speed / maxSpeed, 0f, 1f);
        var current = GD.Db2Linear(VolumeDb);
        var next = Mathf.Lerp(current, frac, LerpT);
        VolumeDb = GD.Linear2Db(next * (High - Low) + Low);
    }
}