
using Godot;

public class WindFollow : PathFollow
{
    [Export] public Curve FollowCurve;
    [Export] public Curve WindVolumeCurve;
    [Export] public Curve SpeedLinesCurve;
    [Export] public float FollowTime;
    [Export] public NodePath FollowingCamera;

    [Signal]
    public delegate void Finished();

    private bool _following;
    private float _time;

    public void StartFollow()
    {
        UnitOffset = 0f;
        _following = true;
        _time = 0f;
    }

    public override void _Ready()
    {
        FollowCurve?.Bake();
        SpeedLinesCurve?.Bake();
        WindVolumeCurve?.Bake();
    }

    public override void _Process(float delta)
    {
        if (!_following) return;
        _time += delta;
        UnitOffset = FollowCurve.InterpolateBaked(Mathf.Clamp(_time, 0f, FollowTime) / FollowTime);
        if (_time >= FollowTime)
            EmitSignal(nameof(Finished));
    }

    public float GetSpeedLines()
    {
        return SpeedLinesCurve.InterpolateBaked(Mathf.Clamp(_time, 0f, FollowTime) / FollowTime);
    }

    public float GetVolume()
    {
        return WindVolumeCurve.InterpolateBaked(Mathf.Clamp(_time, 0f, FollowTime) / FollowTime);
    }
}