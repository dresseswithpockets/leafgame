using Godot;
using System;

public class PlayerLeaf : KinematicBody
{
    [Export] public float Gravity = -4;
    [Export] public float MaxSpeed = 4;
    [Export] public float Accel = 30;
    [Export] public float Decel = -10;

    private Vector3 _wishDir;
    private float _verticalSpeed;
    private Vector3 _horizontalVelocity;
    
    public override void _Ready()
    {
        
    }

    public override void _Process(float delta)
    {
        
    }

    public override void _PhysicsProcess(float delta)
    {
        _wishDir = Vector3.Zero;
        if (Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            _wishDir = new Vector3(Input.GetActionStrength("MoveRight") - Input.GetActionStrength("MoveLeft"),
                0f,
                Input.GetActionStrength("MoveBackward") - Input.GetActionStrength("MoveForward"));

            _wishDir = GetViewport().GetCamera().GlobalTransform.basis.Xform(_wishDir).Normalized();
        }

        var offset = (_wishDir * Accel * MaxSpeed * delta) + (_horizontalVelocity.Normalized() * Decel * MaxSpeed * delta);

        if (_wishDir == Vector3.Zero && offset.LengthSquared() > _horizontalVelocity.LengthSquared())
            _horizontalVelocity = Vector3.Zero;
        else
        {
            _horizontalVelocity.x = Mathf.Clamp(_horizontalVelocity.x + offset.x, -MaxSpeed, MaxSpeed);
            _horizontalVelocity.z = Mathf.Clamp(_horizontalVelocity.z + offset.z, -MaxSpeed, MaxSpeed);

            if (IsOnFloor())
                _verticalSpeed = 0f;
            else
                _verticalSpeed += Gravity * delta;
            
            var vel = _horizontalVelocity;
            vel.y = _verticalSpeed;
            vel = MoveAndSlide(vel, Vector3.Up, true);
            
            _horizontalVelocity = vel;
            _verticalSpeed = _horizontalVelocity.y;
            _horizontalVelocity.y = 0f;
        }

        foreach (var child in this.GetChildren<IPlayerEvents>())
            child.Speed(_horizontalVelocity.Length(), MaxSpeed);
    }
}
