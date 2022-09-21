using Godot;
using System;

public class SpeedLines : CSGCylinder, IPlayerGlidingEvent
{
    [Export] public float RotationSlerpT = 0.3f;
    
    [Export] public float MinAlpha = 0.1f;
    [Export] public float MaxAlpha = 0.25f;
    [Export] public float AlphaLerpT = 0.3f;

    [Export] public float MinLineThreshold = 0.65f;
    [Export] public float MaxLineThreshold = 0.75f;

    [Export] public float MinInverseSpeed = 1f;
    [Export] public float MaxInverseSpeed = 5f;
    
    [Export] public Vector3 RotationOffset = new Vector3(-90f, 0f, 0f);

    private ShaderMaterial _material;

    public override void _Ready()
    {
        _material = (ShaderMaterial)Material;
    }

    public void Gliding(PlayerLeaf player, float power, Quat glideRotation, Vector3 velocity, bool decelerating, bool decaying)
    {
        var rotationOffset = new Basis(RotationOffset * Mathf.Pi / 180f);
        
        var transform = GlobalTransform;
        transform.basis = transform.basis.Slerp(new Basis(glideRotation) * rotationOffset, RotationSlerpT);
        GlobalTransform = transform;

        var glidePowerRatio = (power - player.MinGlidePower) / (player.MaxGlidePower - player.MinGlidePower);
        
        var color = (Color)_material.GetShaderParam("line_color_b");
        var targetAlpha = Mathf.Lerp(MinAlpha, MaxAlpha, glidePowerRatio);
        color.a = Mathf.Lerp(color.a, targetAlpha, AlphaLerpT);
        _material.SetShaderParam("line_color_b", color);

        _material.SetShaderParam("line_threshold", Mathf.Lerp(MinLineThreshold, MaxLineThreshold, 1 - glidePowerRatio));
        _material.SetShaderParam("inverse_speed", Mathf.Lerp(MinInverseSpeed, MaxInverseSpeed, 1 - glidePowerRatio));
    }

    public void GlideOver()
    {
        var color = (Color)_material.GetShaderParam("line_color_b");
        color.a = 0f;
        _material.SetShaderParam("line_color_b", color);
    }
}
