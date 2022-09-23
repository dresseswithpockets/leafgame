
using Godot;

public class AppleDropoff : Area
{
    [Export] public NodePath DropOffSpot;
    [Export] public NodePath TargetApple;
    [Export] public bool TriggersDialogue;
    [Export] public string DialogueSequence;
    [Export] public bool ActivatesNode;
    [Export] public NodePath ActivationNode;

    private Apple _targetApple;
    private Spatial _dropOffSpatial;
    private Spatial _activationNode;
    
    public override void _Ready()
    {
        if (ActivationNode != null && !ActivationNode.IsEmpty())
            _activationNode = GetNode<Spatial>(ActivationNode);

        if (_activationNode != null && ActivatesNode)
        {
            _activationNode.Visible = false;
            _activationNode.SetProcess(false);
            _activationNode.SetPhysicsProcess(false);
        }
        
        _dropOffSpatial = GetNode<Spatial>(DropOffSpot);
        _targetApple = GetNode<Apple>(TargetApple);
        Connect("body_entered", this, nameof(OnBodyEntered));
    }

    private void OnBodyEntered(Node node)
    {
        if (!(node is PlayerLeaf playerLeaf)) return;
        if (!(_targetApple.PickedUp && _targetApple.Following == playerLeaf)) return;
        _targetApple.PickedUp = false;
        _targetApple.GlobalTranslation = _dropOffSpatial.GlobalTranslation;
        //_targetApple.GlobalRotation = _dropOffSpatial.GlobalTransform.basis;
        
        if (TriggersDialogue)
            StartDialogue();
    }
    
    private async void StartDialogue()
    {
        Game.Instance.PlayerSpatial.PauseControl();
        await this.AwaitNextProcess();
        
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

        if (!ActivatesNode) return;
        _activationNode.Visible = true;
        _activationNode.SetProcess(true);
        _activationNode.SetPhysicsProcess(true);
    }
}