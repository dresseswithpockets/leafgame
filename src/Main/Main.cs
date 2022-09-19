using Godot;
using System;
using System.Threading.Tasks;
using Godot.Collections;
using Array = Godot.Collections.Array;

public class Main : Control
{
    private Menu _menu;
    private Game _game;
    
    [Export]
    private string _busName;
    
    public override void _Ready()
    {   
        _menu = GetNode<Menu>("%Menu");
        _menu.Connect(nameof(Menu.PlayPressed), this, nameof(OnPlayPressed));
        _menu.Connect(nameof(Menu.VolumeSliderChanged), this, nameof(OnVolumeChanged));

        _game = GetNode<Game>("%Game");
    }

    public async void OnPlayPressed()
    {
        _menu.SetProcessInput(false);
        await this.AwaitNextProcess();

        var linearVolume = GD.Db2Linear(AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex(_busName)));
        
        var tween = GetTree().CreateTween();
        tween.TweenProperty(_menu, "modulate", Colors.Transparent, 1f).SetTrans(Tween.TransitionType.Linear);
        //tween.Parallel().TweenMethod(this, nameof(OnVolumeChanged), linearVolume, 0f, 1f).SetTrans(Tween.TransitionType.Linear);
        tween.TweenCallback(this, "remove_child", new Array {_menu});
        await ToSignal(tween, "finished");
        
        _game.StartGame();
    }

    public void OnVolumeChanged(float value)
    {
        var idx = AudioServer.GetBusIndex(_busName);
        if (idx == -1) return;
        AudioServer.SetBusVolumeDb(idx, GD.Linear2Db(value));
    }
}
