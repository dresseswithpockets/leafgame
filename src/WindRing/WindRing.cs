using Godot;
using System;

public class WindRing : Spatial
{
    [Export] public float Cooldown = 5;
    [Export] public bool Enabled = true;
    [Export] public bool TriggerWindPath;
    [Export] public NodePath PathFollower;
    [Export] public bool SetsStartingId;
    [Export] public int StartingId;
    
    private Area _area;
    private float _cooldownTimer;
    private WindFollow _pathFollow;

    public override void _Ready()
    {
        _area = GetNode<Area>("Area");
        _area.Connect("body_entered", this, nameof(OnBodyEntered));
        if (!(PathFollower?.IsEmpty() ?? true))
            _pathFollow = GetNode<WindFollow>(PathFollower);
    }

    public override void _Process(float delta)
    {
        _cooldownTimer = Mathf.Clamp(_cooldownTimer - delta, 0, Cooldown);
    }

    private void OnBodyEntered(Node body)
    {
        GD.Print($"{body.Name} entered");
        if (!Enabled) return;
        if (!(body is PlayerLeaf player)) return;
        if (_cooldownTimer > 0) return;

        if (SetsStartingId)
            Game.Instance.SetCurrentStartingPosition(StartingId);
        
        if (TriggerWindPath && _pathFollow != null)
        {
            player.FollowWind(_pathFollow);
            return;
        }
        
        player.AddGlideBoost();
        _cooldownTimer = Cooldown;
    }
}
