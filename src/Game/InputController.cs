using Godot;
using System;

public class InputController : Node
{
    public static InputController Instance;
    
    public override void _EnterTree()
    {
        if (Instance != null)
            GD.PrintErr($"Replacing InputController {Instance.GetPath()} with another: {GetPath()}");
        Instance = this;
    }

    [Export]
    public bool AllowInput = false;

    private bool _canLock = false;

    public bool CanLock
    {
        get => _canLock;
        set
        {
            _canLock = value;
            if (!value)
                Input.MouseMode = Input.MouseModeEnum.Visible;
        }
    }

    public bool CanInput => AllowInput && Input.MouseMode == Input.MouseModeEnum.Captured;

    public void IntroStart()
    {
        AllowInput = false;
    }

    public void IntroOver()
    {
        CanLock = true;
        AllowInput = false;
    }

    public void HouseSequenceOver()
    {
        AllowInput = true;
    }

    public override void _Process(float delta)
    {
        if (Input.IsKeyPressed((int)KeyList.Escape))
            Input.MouseMode = Input.MouseModeEnum.Visible;
        else if (Input.IsMouseButtonPressed((int)ButtonList.Left) && CanLock)
            Input.MouseMode = Input.MouseModeEnum.Captured;
    }
}
