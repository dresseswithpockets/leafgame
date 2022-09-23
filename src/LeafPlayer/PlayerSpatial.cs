using Godot;
using System;

public class PlayerSpatial : Spatial
{
    public Spatial PickupFollow;
    public PlayerLeaf PlayerLeaf;
    private Spatial _groundCamera;
    private Spatial _orbiter;
    
    public override void _Ready()
    {
        PickupFollow = GetNode<Spatial>("PickupFollow");
        PlayerLeaf = GetNode<PlayerLeaf>("PlayerLeaf");
        PlayerLeaf.PlayerSpatial = this;
        _groundCamera = GetNode<Spatial>("%GroundCamera");
        _orbiter = GetNode<Spatial>("PlayerLeaf/Orbiter");
    }

    public void GiveControl()
    {
        InputController.Instance.CanLock = true;
        InputController.Instance.AllowInput = true;
        _groundCamera.Set("enabled", true);
    }

    public void PauseControl()
    {
        InputController.Instance.AllowInput = false;
        _groundCamera.Set("enabled", false);
    }

    public void SetCameraToCurrentCameraRotation()
    {
        var rot = Game.Instance.CameraBrain.GlobalTransform.basis.GetEuler();
        //_orbiter.Set("input_rotation", new Vector2(rot.y, rot.x));
    }
}
