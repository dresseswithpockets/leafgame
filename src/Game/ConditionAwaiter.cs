using Godot;

namespace leafgame.Game
{
    public class ConditionAwaiter : Object
    {
        [Signal]
        public delegate void ConditionMet();
    }
}