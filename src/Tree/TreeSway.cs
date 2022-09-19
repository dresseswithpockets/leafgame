using Godot;
using Godot.Collections;

public class TreeSway : Spatial
{
    [Export]
    private ShaderMaterial[] _materials;

    [Export(PropertyHint.Range, "-10,10,0.01")] private float _swayFinalValue = 0.1f;
    [Export(PropertyHint.Range, "0,100,0.01")] private float _swayEaseOutTime = 2f;
    [Export] private Tween.TransitionType _swayTransitionType = Tween.TransitionType.Cubic;
    [Export] private Tween.EaseType _swayEaseType = Tween.EaseType.Out;
    
    public override void _Ready()
    {
    }

    public SignalAwaiter EaseStopSway()
    {
        var tween = GetTree().CreateTween();
        foreach (var mat in _materials)
            tween.Parallel().TweenMethod(this,
                    nameof(SetShaderParam),
                    mat.GetShaderParam("sway"),
                    0.1f,
                    _swayEaseOutTime,
                    new Array { mat, "sway" })
                .SetTrans(_swayTransitionType)
                .SetEase(_swayEaseType);

        return ToSignal(tween, "finished");
    }

    public void SetShaderParam(float value, ShaderMaterial mat, string param)
        => mat.SetShaderParam(param, value);
}
