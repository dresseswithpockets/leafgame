using Godot;
using System;

public class UIOverlayCamera : Camera
{
    private Camera _mainCamera;
    private Game _game;

    public override void _Ready()
    {
        _game = GetNode<Game>("%Game");
        _mainCamera = _game?.GetNode<Camera>("VCameraBrain");
    }

    public override void _Process(float delta)
    {
        if (_mainCamera == null) return;
        if (!_game.ShouldMoveOverlayCamera) return;
        GlobalTransform = _mainCamera.GlobalTransform;
        Fov = _mainCamera.Fov;
    }
}
