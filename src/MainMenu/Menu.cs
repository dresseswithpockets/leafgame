using Godot;

public class Menu : Control
{
    [Signal]
    public delegate void PlayPressed();

    [Signal]
    public delegate void VolumeSliderChanged(float value);

    public void OnPlayPressed() => EmitSignal(nameof(PlayPressed));

    public void OnVolumeSliderChanged(float value) => EmitSignal(nameof(VolumeSliderChanged), value);
}
