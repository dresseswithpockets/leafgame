using Godot;
using System;

public class WindWall : Spatial
{
    [Export] public NodePath IslandCenterNode { get; set; }
    [Export] public float WindSpeed { get; set; }
    [Export] public float MinTravelTime { get; set; }

    public Spatial IslandCenter;
    private CSGShape _wall;

    public override void _Ready()
    {
        _wall = GetChild<CSGShape>(0);
        IslandCenter = GetNode<Spatial>(IslandCenterNode);

        foreach (var signal in _wall.GetSignalList())
        {
            GD.Print(signal);
        }
        
        _wall.Connect("body_entered", this, nameof(OnBodyEntered));
    }

    private void OnBodyEntered(Node node)
    {
        if (!(node is PlayerLeaf player)) return;
        player.HitWindWall(this);
    }
}
