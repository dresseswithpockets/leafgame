using Godot;
using System;

public class GuyInteractable : Spatial
{
    [Export] public bool StartsDialogue;
    [Export] public string DialogueSequence;
    [Export] public bool OneOff;
    
    private Area _interactableArea;
    private bool _playing;
    private bool _playedOnce;

    [Signal]
    public delegate void BeginSequence();

    public override void _Ready()
    {
        _interactableArea = GetNode<Area>("Area");
        Connect(nameof(BeginSequence), this, nameof(OnStartSequence));
    }

    public override void _Process(float delta)
    {
        if (!StartsDialogue || _playing) return;
        if (!Input.IsActionJustPressed("SkipDialogue")) return;
        if (!_interactableArea.OverlapsBody(Game.Instance.PlayerSpatial.PlayerLeaf)) return;
        if (OneOff && _playedOnce) return;
        
        _playedOnce = true;
        EmitSignal(nameof(BeginSequence));
    }

    private async void OnStartSequence()
    {
        _playing = true;
        Game.Instance.PlayerSpatial.PauseControl();
        await this.AwaitTimer(0.1f);
        
        Main.Instance.SetupDialogue(DialogueSequence);
        Main.Instance.StartPage();
        var dialogueFinished = ToSignal(Main.Instance, nameof(Dialogue.FinishedBook));
        
        var page = Main.Instance.CurrentPage;
        var lastCamera = (page?.EnableCamera ?? false) ? Game.Instance.GetNode<Spatial>(page.CameraName) : null;
        lastCamera?.Set("enabled", true);
        
        while (!dialogueFinished.IsCompleted)
        {
            if (Input.IsActionJustPressed("SkipDialogue"))
            {
                lastCamera?.Set("enabled", false);
                page = Main.Instance.ContinuePage();
                if (page?.EnableCamera ?? false)
                {
                    lastCamera = Game.Instance.GetNode<Spatial>(page.CameraName);
                    lastCamera.Set("enabled", true);
                }
            }

            await this.AwaitNextProcess();
        }

        lastCamera?.Set("enabled", false);
        
        Game.Instance.PlayerSpatial.SetCameraToCurrentCameraRotation();
        Game.Instance.PlayerSpatial.GiveControl();
        _playing = false;
    }
}
