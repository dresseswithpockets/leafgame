using Godot;
using System;

public class VelocityBox : CSGBox
{
    [Export] public float MinDepth;
    [Export] public float MaxDepth;

    private PlayerLeaf _playerLeaf;

    public override void _Ready()
    {
        _playerLeaf = GetParent<PlayerLeaf>();
    }

    public override void _Process(float delta)
    {
        var playerVelocity = _playerLeaf.Velocity;
        var playerSpeed = playerVelocity.Length();
        Depth = Mathf.Lerp(MinDepth, MaxDepth, playerSpeed / _playerLeaf.MaxGroundSpeed);
        var transform = Transform.LookingAt(playerVelocity, Vector3.Up);
        Transform = transform;
    }
}
