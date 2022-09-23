using Godot;
using System;

public class Apple : Spatial
{
    [Export] public Vector3 OffsetConstant;
    [Export] public Vector3 OffsetRate;
    [Export] public Vector3 OffsetAmplitude;
    [Export] public float YawSpeed;
    
    private Area _area;
    public bool PickedUp;
    public PlayerLeaf Following;
    private float _timer;

    public override void _Ready()
    {
        _area = GetNode<Area>("Area");
        _area.Connect("body_entered", this, nameof(OnBodyEntered));
    }

    public override void _Process(float delta)
    {
        if (!PickedUp || Following == null) return;
        _timer += delta;
        var offset = OffsetAmplitude * new Vector3(
            Mathf.Sin(_timer * OffsetRate.x), 
            Mathf.Sin(_timer * OffsetRate.y),
            Mathf.Sin(_timer * OffsetRate.z));
        GlobalTranslation = Following.PlayerSpatial.PickupFollow.GlobalTranslation + offset + OffsetConstant;
        
        RotateY(Mathf.Deg2Rad(YawSpeed * delta));
    }

    private void OnBodyEntered(Node node)
    {
        if (!(node is PlayerLeaf playerLeaf)) return;
        if (PickedUp) return;

        _timer = 0f;
        Following = playerLeaf;
        PickedUp = true;
        _area.Disconnect("body_entered", this, nameof(OnBodyEntered));
    }
}
