
using Godot;

public class RadialCamera : Spatial
{
    [Export] public NodePath Target;
    [Export] public Vector3 TargetOffset;
    [Export] public Shape Shape;
    [Export(PropertyHint.Layers3dPhysics)] public uint CollisionMask;

    private Spatial _camera;

    public override void _Ready()
    {
        _camera = GetChild<Spatial>(0);
    }

    public override void _PhysicsProcess(float delta)
    {
        var space = GetWorld().DirectSpaceState;
        var targetPos = GetNode<Spatial>(Target).GlobalTransform.origin + TargetOffset;
        var motion = GlobalTransform.origin - targetPos;
        var query = new PhysicsShapeQueryParameters
        {
            Margin = Shape.Margin,
            CollisionMask = CollisionMask,
            Transform = new Transform(Basis.Identity, targetPos),
        };
        query.SetShape(Shape);
        var results = space.CastMotion(query, motion);
        var safeFraction = (float)results[0];
        var newCameraPos = targetPos.Lerp(GlobalTransform.origin, safeFraction);
        _camera.GlobalTranslation = newCameraPos;
    }
}