using Godot;
using System;

public class UIOverlayCamera : Camera
{
    private Camera _mainCamera;

    public override void _Ready()
    {
        var game = GetNode("%Game");
        _mainCamera = game?.GetNode<Camera>("VCameraBrain");
    }

    public override void _Process(float delta)
    {
        if (_mainCamera == null) return;
        GlobalTransform = _mainCamera.GlobalTransform;
        Fov = _mainCamera.Fov;
    }
}
