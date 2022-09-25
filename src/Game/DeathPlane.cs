using Godot;

public class DeathPlane : Area
{
    public override void _Ready()
    {
        Connect("body_entered", this, nameof(OnBodyEntered));
    }

    private void OnBodyEntered(Node node)
    {
        if (!(node is PlayerLeaf playerLeaf)) return;
        Game.Instance.HandleBottomPlaneHit();
    }
}