using Godot;
using System;
using System.Linq;

public class Game : Spatial
{
    public static Game Instance;

    public bool ShouldMoveOverlayCamera;
    public Camera CameraBrain;
    
    public PlayerSpatial PlayerSpatial;
    private Spatial _gameStartCamera;
    private TreeSway _tree;
    private AnimationPlayer _branchAnimator;
    private AnimationPlayer _introPlayer;
    //private Dialogue _house1Dialogue;
    [Export] private string _house1DialogueSequenceName;
    [Export]
    private string _branchSwayAnimationName = "BranchSway";

    [Export] private NodePath[] _startingNodes;

    [Signal]
    public delegate void BeginPlay();

    private Spatial[] _startingSpatials;
    private int _spatialIndex;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        PlayerSpatial = GetNode<PlayerSpatial>("%PlayerSpatial");
        _gameStartCamera = GetNode<Spatial>("%VCamera GameStart");
        CameraBrain = GetNode<Camera>("%VCameraBrain");
        _tree = GetNode<TreeSway>("%Tree");
        _branchAnimator = GetNode<AnimationPlayer>("%BranchAnimator");
        _introPlayer = GetNode<AnimationPlayer>("%IntroPlayer");
        //_house1Dialogue = GetNode<Dialogue>("%House1Dialogue");

        _introPlayer.Connect("animation_finished", this, nameof(StartHouseSequence));
        
        // if allow input is already set to true, then assume that we want to debug the player
        if (InputController.Instance.AllowInput)
        {
            PlayerSpatial.GiveControl();
            _gameStartCamera.Set("enabled", false);
        }

        _startingSpatials = _startingNodes.Select(GetNode<Spatial>).ToArray();
    }

    public void StartGame()
    {
        InputController.Instance.CanLock = true;
        EmitSignal(nameof(BeginPlay));
    }

    public async void StartHouseSequence(string _)
    {
        // begins the house dialogue sequence and other character animations
        PlayerSpatial.PauseControl();
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
        
        PlayerSpatial.SetCameraToCurrentCameraRotation();
        PlayerSpatial.GiveControl();
    }

    public override void _Process(float delta)
    {
        if (!Input.IsActionJustPressed("Debug_StartGame") || OS.HasFeature("EXPORT")) return;

        // if the player is in control, then this input will cycle the player between starting positions
        if (InputController.Instance.CanInput && _startingSpatials.Length > 0)
        {
            _spatialIndex++;
            if (_spatialIndex >= _startingSpatials.Length)
                _spatialIndex = 0;

            PlayerSpatial.PlayerLeaf.GlobalTranslation = _startingSpatials[_spatialIndex].GlobalTranslation;
            return;
        }

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

    private SignalAwaiter EaseToStartCamera()
    {
        var transitionTime = (float)_gameStartCamera.Get("transition_time");
        var currentTarget = (Spatial)CameraBrain.Call("get_highest_priority_vcamera");
        var targetPriority = (int)currentTarget.Get("priority");
        _gameStartCamera.Set("priority", targetPriority + 1);
        return this.AwaitTimer(transitionTime);
    }

    public void HandleBottomPlaneHit()
    {
        PlayerSpatial.PlayerLeaf.GlobalTranslation = _startingSpatials[_spatialIndex].GlobalTranslation;
    }

    public void SetCurrentStartingPosition(int x)
    {
        _spatialIndex = x;
    }
}
