
using Godot;

public static class TreeExtensions
{
    public static SignalAwaiter AwaitTimer(this Node node, float time, bool processDuringPause = true)
        => node.ToSignal(node.GetTree().CreateTimer(time, processDuringPause), "timeout");

    /// <summary>
    /// Creates a signal awaiter that awaits a <see cref="SceneTreeTimer"/> with a timeout of 0 seconds. The timeout
    /// will not occur until the next frame. 
    /// </summary>
    public static SignalAwaiter AwaitNextProcess(this Node node, bool processDuringPause = true)
        => AwaitTimer(node, 0f, processDuringPause);

    
}