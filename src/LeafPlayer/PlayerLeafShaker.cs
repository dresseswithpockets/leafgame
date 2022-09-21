
using Godot;

[Tool]
public class PlayerLeafShaker : Spatial, IPlayerEvents
{
    [Export] public NodePath ShakeTarget;
    [Export] public OpenSimplexNoise Noise;
    [Export] public float ShakeSpeed;
    [Export] public Vector3 ShakeStrengthTranslation;
    [Export] public Vector3 ShakeStrengthRotation;
    [Export(PropertyHint.Range, "0.0,1.0")] public float ShakeBlend;
    [Export(PropertyHint.Range, "0.0,1.0")] public float ShakeBlendLerpT;
    [Export] public Vector3 StartPosition;
    [Export] public Vector3 StartRotation;

    private float _shakeTime;

    public override void _PhysicsProcess(float delta)
    {
        _shakeTime += delta * ShakeSpeed * 50;
        var target = GetNodeOrNull<Spatial>(ShakeTarget);
        var targetTranslation = new Vector3(
            Noise.GetNoise4d(_shakeTime, 0, 0, 0) * ShakeStrengthTranslation.x,
            Noise.GetNoise4d(0, _shakeTime, 0, 0) * ShakeStrengthTranslation.y,
            Noise.GetNoise4d(0, 0, _shakeTime, 0) * ShakeStrengthTranslation.z);
        targetTranslation.x = Mathf.Lerp(StartPosition.x, targetTranslation.x, ShakeBlend);
        targetTranslation.y = Mathf.Lerp(StartPosition.y, targetTranslation.y, ShakeBlend);
        targetTranslation.z = Mathf.Lerp(StartPosition.z, targetTranslation.z, ShakeBlend);
        target.Translation = targetTranslation;

        var targetRotation = new Vector3(
            Mathf.Deg2Rad(Noise.GetNoise4d(_shakeTime, 0, 0, 1) * ShakeStrengthRotation.x),
            Mathf.Deg2Rad(Noise.GetNoise4d(0, _shakeTime, 0, 1) * ShakeStrengthRotation.y),
            Mathf.Deg2Rad(Noise.GetNoise4d(0, 0, _shakeTime, 1) * ShakeStrengthRotation.z));
        targetRotation.x = Mathf.Lerp(StartRotation.x, targetRotation.x, ShakeBlend);
        targetRotation.y = Mathf.Lerp(StartRotation.y, targetRotation.y, ShakeBlend);
        targetRotation.z = Mathf.Lerp(StartRotation.z, targetRotation.z, ShakeBlend);
        target.Rotation = targetRotation;
    }

    public void GroundSpeed(float speed, float maxSpeed)
    {
        ShakeBlend = Mathf.Lerp(ShakeBlend, Mathf.Clamp(speed / maxSpeed, 0f, 1f), ShakeBlendLerpT);
    }
}