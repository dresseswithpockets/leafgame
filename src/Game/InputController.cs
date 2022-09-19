using Godot;
using System;

public class InputController : Node
{
    public override void _Process(float delta)
    {
        if (Input.IsKeyPressed((int)KeyList.Escape))
            Input.MouseMode = Input.MouseModeEnum.Visible;
        else if (Input.IsMouseButtonPressed((int)ButtonList.Left))
            Input.MouseMode = Input.MouseModeEnum.Captured;
    }
}
