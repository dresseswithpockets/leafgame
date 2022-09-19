
using System.Collections.Generic;
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

    public static bool TryGetChild<T>(this Node node, out T child)
    {
        foreach (var c in node.GetChildren())
        {
            if (!(c is T t)) continue;
            
            child = t;
            return true;
        }

        child = default;
        return false;
    }

    public static IEnumerable<T> GetChildren<T>(this Node node)
    {
        foreach (var c in node.GetChildren())
        {
            if (!(c is T t)) continue;
            yield return t;
        }
    }

    public static Vector3 Lerp(this Vector3 a, Vector3 b, float t) => new Vector3(
        Mathf.Lerp(a.x, b.x, t),
        Mathf.Lerp(a.y, b.y, t),
        Mathf.Lerp(a.z, b.z, t));
}