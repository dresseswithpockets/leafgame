
using Godot;

public class LeafSway : Spatial
{    
    [Export]
    public float TimeScale
    {
        get => (float)_material.GetShaderParam("TimeScale");
        set => _material.SetShaderParam("TimeScale", value);
    }

    [Export]
    private ShaderMaterial _material;
    
    public override void _Ready()
    {
        
    }
}