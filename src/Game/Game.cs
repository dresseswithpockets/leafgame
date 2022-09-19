using Godot;
using System;

public class Game : Spatial
{
    private Spatial _gameStartCamera;
    private Camera _cameraBrain;
    private TreeSway _tree;
    private AnimationPlayer _branchAnimator;
    private AnimationPlayer _introPlayer;
    private AnimationPlayer _house1Player;
    private Dialogue _house1Dialogue;
    [Export]
    private string _branchSwayAnimationName = "BranchSway";

    [Signal]
    public delegate void BeginPlay();

    public override void _Ready()
    {
        _gameStartCamera = GetNode<Spatial>("%VCamera GameStart");
        _cameraBrain = GetNode<Camera>("%VCameraBrain");
        _tree = GetNode<TreeSway>("%Tree");
        _branchAnimator = GetNode<AnimationPlayer>("%BranchAnimator");
        _introPlayer = GetNode<AnimationPlayer>("%IntroPlayer");
        _house1Player = GetNode<AnimationPlayer>("%House1Player");
        _house1Dialogue = GetNode<Dialogue>("%House1Dialogue");

        _introPlayer.Connect("animation_finished", this, nameof(StartHouseSequence));
    }

    public void StartGame()
    {
        EmitSignal(nameof(BeginPlay));
    }

    public async void StartHouseSequence(string _)
    {
        // begins the house dialogue sequence and other character animations
        // _house1Player.Play("Greeting");
        await this.AwaitTimer(1f);
        _house1Dialogue.StartPage();
        var dialogueFinished = ToSignal(_house1Dialogue, nameof(Dialogue.FinishedBook));
        while (!dialogueFinished.IsCompleted)
        {
            if (Input.IsActionJustPressed("SkipDialogue"))
                _house1Dialogue.ContinuePage();
            await this.AwaitNextProcess();
        }
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("Debug_StartGame") && !OS.HasFeature("EXPORT"))
        {
            var introPlayer = GetNode<AnimationPlayer>("SequenceIntro/IntroPlayer");
            if (introPlayer.PlaybackActive && introPlayer.CurrentAnimation == "Intro")
            {
                introPlayer.PlaybackSpeed = 4;
            }
            else
            {
                StartGame();
            }
        }
    }

    private SignalAwaiter EaseToStartCamera()
    {
        var transitionTime = (float)_gameStartCamera.Get("transition_time");
        var currentTarget = (Spatial)_cameraBrain.Call("get_highest_priority_vcamera");
        var targetPriority = (int)currentTarget.Get("priority");
        _gameStartCamera.Set("priority", targetPriority + 1);
        return this.AwaitTimer(transitionTime);
    }
}
