
using Godot;

public interface IPlayerEvents
{
    void GroundSpeed(float speed, float maxSpeed);
}

public interface IPlayerGlidingEvent
{
    void Gliding(PlayerLeaf player, float power, Quat glideRotation, Vector3 velocity, bool decelerating, bool decaying);

    void GlideOver();
}