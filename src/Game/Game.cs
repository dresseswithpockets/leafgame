using Godot;
using System;

public class Game : Spatial
{
    public static Game Instance;

    public bool ShouldMoveOverlayCamera;
    public Camera CameraBrain;
    
    private PlayerSpatial _playerSpatial;
    private Spatial _gameStartCamera;
    private TreeSway _tree;
    private AnimationPlayer _branchAnimator;
    private AnimationPlayer _introPlayer;
    //private Dialogue _house1Dialogue;
    [Export] private string _house1DialogueSequenceName;
    [Export]
    private string _branchSwayAnimationName = "BranchSway";

    [Signal]
    public delegate void BeginPlay();

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        _playerSpatial = GetNode<PlayerSpatial>("%PlayerSpatial");
        _gameStartCamera = GetNode<Spatial>("%VCamera GameStart");
        CameraBrain = GetNode<Camera>("%VCameraBrain");
        _tree = GetNode<TreeSway>("%Tree");
        _branchAnimator = GetNode<AnimationPlayer>("%BranchAnimator");
        _introPlayer = GetNode<AnimationPlayer>("%IntroPlayer");
        //_house1Dialogue = GetNode<Dialogue>("%House1Dialogue");

        _introPlayer.Connect("animation_finished", this, nameof(StartHouseSequence));
        
        // if allow input is already set to true, then assume that we want to debug the player
        if (InputController.Instance.AllowInput)
            _playerSpatial.GiveControl();
    }

    public void StartGame()
    {
        InputController.Instance.CanLock = true;
        EmitSignal(nameof(BeginPlay));
    }

    public async void StartHouseSequence(string _)
    {
        // begins the house dialogue sequence and other character animations
        _playerSpatial.PauseControl();
        await this.AwaitTimer(1f);
        //_house1Dialogue.StartPage();
        Main.Instance.SetupDialogue(_house1DialogueSequenceName);
        Main.Instance.StartPage();
        var dialogueFinished = ToSignal(Main.Instance, nameof(Dialogue.FinishedBook));
        Spatial lastCamera = null;
        while (!dialogueFinished.IsCompleted)
        {
            if (Input.IsActionJustPressed("SkipDialogue"))
            {
                // ShouldMoveOverlayCamera = true;
                lastCamera?.Set("enabled", false);
                var page = Main.Instance.ContinuePage();
                if (page?.EnableCamera ?? false)
                {
                    // ShouldMoveOverlayCamera = false;
                    lastCamera = GetNode<Spatial>(page.CameraName);
                    lastCamera.Set("enabled", true);
                }
            }

            await this.AwaitNextProcess();
        }

        lastCamera?.Set("enabled", false);
        // ShouldMoveOverlayCamera = true;
        
        _playerSpatial.SetCameraToCurrentCameraRotation();
        _playerSpatial.GiveControl();
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
        var currentTarget = (Spatial)CameraBrain.Call("get_highest_priority_vcamera");
        var targetPriority = (int)currentTarget.Get("priority");
        _gameStartCamera.Set("priority", targetPriority + 1);
        return this.AwaitTimer(transitionTime);
    }
}
