using Godot;

public class PlayerLeaf : KinematicBody
{
    [Export] public float Gravity = -4;
    [Export] public float MaxGroundSpeed = 4;
    [Export] public float GroundAccel = 30;
    [Export] public float GroundDecel = -10;

    [Export] public float MaxGlidePower = 5;
    [Export] public float MinGlidePower = 1;
    [Export] public float GlidePowerDecelRate = 3;
    /// <summary>
    /// amount of time after getting a power boost before deceleration kicks in
    /// </summary>
    [Export] public float GlidePowerDecelCooldown = 1;

    /// <summary>
    /// the amount of time the player can be at the min glide power before they lose their glide status
    /// </summary>
    [Export] public float GlideAtMinBeforeDecayTime = 3;

    [Export] public float MinCameraDistance = 1;
    [Export] public float MaxCameraDistance = 2;

    [Export] public float GlideMinPitch = -0.5f;
    [Export] public float GlideMaxPitch = 0.5f;
    [Export] public float GlideYawSpeed = 1f;
    [Export] public float GlidePitchSpeed = 0.25f;

    [Export] public NodePath GroundCamera;
    [Export] public NodePath GroundCameraOrbiter;
    [Export] public NodePath GlideCameraFollow;
    [Export] public NodePath GlideCameraFollowRot;
    [Export] public NodePath GlideCamera;

    private Spatial _groundCameraOrbiter;
    private Spatial _groundCamera;
    private Spatial _glideCameraFollow;
    private Spatial _glideCameraFollowRot;
    private Spatial _glideCamera;
    private bool _gliding;
    private Vector3 _wishDir;
    
    private float _verticalSpeed;
    private Vector3 _horizontalVelocity;

    private float _glidePitch;
    private float _glideYaw;
    private Vector3 _glideVelocity;
    private float _glidePower;
    private float _glideCooldownTimer;
    private float _glideMinTimer;

    public Vector3 Velocity => _gliding ? _glideVelocity : _horizontalVelocity;
    
    public override void _Ready()
    {
        _groundCamera = GetNode<Spatial>(GroundCamera);
        _glideCameraFollow = GetNode<Spatial>(GlideCameraFollow);
        _glideCameraFollowRot = GetNode<Spatial>(GlideCameraFollowRot);
        _glideCamera = GetNode<Spatial>(GlideCamera);
        _glidePower = MinGlidePower;
        _groundCameraOrbiter = GetNode<Spatial>(GroundCameraOrbiter);
    }

    public void AddGlideBoost()
    {
        if (_followingWind) return;
        
        _glideCooldownTimer = 0;
        _glideMinTimer = 0;
        _glidePower = MaxGlidePower;
        
        if (!_gliding)
        {
            var moveDir = _horizontalVelocity.Normalized();
            GD.Print($"moveDir: {moveDir}");
            _glideYaw = Mathf.Wrap(Mathf.Atan2(moveDir.x, moveDir.z) + Mathf.Pi, -Mathf.Tau, Mathf.Tau);
            GD.Print($"new yaw: {Mathf.Rad2Deg(_glideYaw)}");
            _glidePitch = 0f;
            _glideCameraFollow.Translation = new Vector3(0f, 0f, _glidePower);
            _glideCameraFollowRot.Rotation = new Vector3(_glidePitch, _glideYaw, 0f);
            _glideCamera.Set("enabled", true);
            _glideVelocity = new Basis(new Vector3(_glidePitch, _glideYaw, 0f)).Xform(Vector3.Forward) * _glidePower;

            /*if (moveDir.z == 1f) angle = 0f;
            else if (moveDir.x == -1f) angle = Mathf.Pi;
            else if (moveDir.z == -1f) angle = Mathf.Pi * 2;
            else if (moveDir.x == 1f) angle = Mathf.Pi * 3;
            else angle = */

            /*_glideCameraFollow.GlobalTransform = _groundCamera.GlobalTransform;
            _glideVelocity = _horizontalVelocity;
            _glideCamera.Set("enabled", true);
            var glideEuler = _groundCamera.GlobalTransform.basis.GetEuler();
            _glidePitch = glideEuler.x;
            _glideYaw = glideEuler.y;*/
        }
        
        _gliding = true;
    }

    private void MoveGround(float delta)
    {
        // move horizontally relative to our camera
        _wishDir = GetViewport().GetCamera().GlobalTransform.basis.Xform(_wishDir).Normalized();
        
        var offset = (_wishDir * GroundAccel * MaxGroundSpeed * delta) + (_horizontalVelocity.Normalized() * GroundDecel * MaxGroundSpeed * delta);

        if (_wishDir == Vector3.Zero && offset.LengthSquared() > _horizontalVelocity.LengthSquared())
            _horizontalVelocity = Vector3.Zero;
        else
        {
            _horizontalVelocity.x = Mathf.Clamp(_horizontalVelocity.x + offset.x, -MaxGroundSpeed, MaxGroundSpeed);
            _horizontalVelocity.z = Mathf.Clamp(_horizontalVelocity.z + offset.z, -MaxGroundSpeed, MaxGroundSpeed);

            if (IsOnFloor())
                _verticalSpeed = 0f;
            else
                _verticalSpeed += Gravity * delta;
            
            HandleGroundMoveAndSlide();
        }

        foreach (var child in this.GetChildren<IPlayerEvents>())
            child.GroundSpeed(_horizontalVelocity.Length(), MaxGroundSpeed);
    }

    private void HandleGroundMoveAndSlide()
    {
        var vel = _horizontalVelocity;
        vel.y = _verticalSpeed;
        vel = MoveAndSlideWithSnap(vel, new Vector3(0, -0.2f, 0f), Vector3.Up, true, floorMaxAngle: 1.396263f);

        _horizontalVelocity = vel;
        _verticalSpeed = _horizontalVelocity.y;
        _horizontalVelocity.y = 0f;
    }

    private void MoveGlide(float delta)
    {
        if (_wishDir != Vector3.Zero)
        {
            _glidePitch += _wishDir.z * GlidePitchSpeed * Mathf.Pi * delta;
            _glidePitch = Mathf.Clamp(_glidePitch, GlideMinPitch, GlideMaxPitch);
            _glideYaw += -_wishDir.x * GlideYawSpeed * Mathf.Pi * delta;
            _glideYaw = Mathf.Wrap(_glideYaw, -Mathf.Tau, Mathf.Tau);
            _glideVelocity = new Basis(new Vector3(_glidePitch, _glideYaw, 0f)).Xform(Vector3.Forward) * _glidePower;
        }

        // make sure our velocity magnitude is always capped to our current power
        _glideVelocity = _glideVelocity.Normalized() * _glidePower;

        if (_glideCooldownTimer < GlidePowerDecelCooldown)
            _glideCooldownTimer += delta;
        
        if (_glideCooldownTimer >= GlidePowerDecelCooldown)
            _glidePower -= GlidePowerDecelRate * delta;

        if (_glidePower <= MinGlidePower)
        {
            _glidePower = MinGlidePower;
            _glideMinTimer += delta;
        }
        else
        {
            _glideMinTimer = 0;
        }

        // if we've decayed our gliding state, then immediately process a ground move 
        if (_glideMinTimer >= GlideAtMinBeforeDecayTime)
        {
            foreach (var child in this.GetChildren<IPlayerGlidingEvent>())
                child.GlideOver();
            
            _glideCamera.Set("enabled", false);
            _groundCameraOrbiter.Set("input_rotation",
                new Vector2(Mathf.Rad2Deg(_glideYaw), Mathf.Rad2Deg(_glidePitch)));
            _gliding = false;
            _horizontalVelocity = _glideVelocity;
            _verticalSpeed = _horizontalVelocity.y;
            
            // todo: maybe this isnt necessary
            HandleGroundMoveAndSlide();
            return;
        }

        _glideVelocity = MoveAndSlide(_glideVelocity, Vector3.Up);
        
        // adjust our camera to always be behind our player after moving, based on the direction they're moving
        // var inverseCameraDir = -_glideVelocity.Normalized();
        
        var glidePowerRatio = (_glidePower - MinGlidePower) / (MaxGlidePower - MinGlidePower);
        _glideCameraFollow.Translation =
            new Vector3(-Mathf.Lerp(MinCameraDistance, MaxCameraDistance, glidePowerRatio), 0, Mathf.Lerp(MinCameraDistance, MaxCameraDistance, glidePowerRatio));

        _glideCameraFollowRot.Rotation = new Vector3(_glidePitch, _glideYaw, 0f);
        
        /*var glideCameraTransform = _glideCameraFollow.Transform;
        glideCameraTransform.basis = Basis.Identity;
        glideCameraTransform.origin =
            inverseCameraDir * ;
        _glideCameraFollow.Transform = glideCameraTransform;*/
        
        // grab our corrected pitch and yaw after moving
        {
            var lookAt = new Transform().LookingAt(_glideVelocity, Vector3.Up);
            var lookAtEuler = lookAt.basis.GetEuler();
            _glidePitch = lookAtEuler.x;
            _glideYaw = lookAtEuler.y;
            /*var yawVel = _glideVelocity;
            yawVel.y = 0f;
            _glideYaw = Vector3.Forward.AngleTo(yawVel.Normalized());

            var pitchVel = new Vector3(0f, _glideVelocity.y, 0f);
            _glidePitch = Mathf.Clamp(Vector3.Forward.AngleTo(pitchVel.Normalized()), GlideMinPitch, GlideMaxPitch);*/
        }

        foreach (var child in this.GetChildren<IPlayerGlidingEvent>())
            child.Gliding(this,
                _glidePower, 
                new Quat(new Vector3(_glidePitch, _glideYaw, 0f)),
                _glideVelocity,
                _glideCooldownTimer >= GlidePowerDecelCooldown,
                _glideMinTimer >= GlideAtMinBeforeDecayTime);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (!InputController.Instance.CanInput) return;
        
        _wishDir = Vector3.Zero;
        if (Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            _wishDir = new Vector3(Input.GetActionStrength("MoveRight") - Input.GetActionStrength("MoveLeft"),
                0f,
                Input.GetActionStrength("MoveBackward") - Input.GetActionStrength("MoveForward"));
        }
        
        if (_gliding)
        {
            MoveGlide(delta);
        }
        else
        {
            MoveGround(delta);
        }
    }

    public void HitWindWall(WindWall windWall)
    {
        GD.Print(windWall);
        // todo: move towards windWall.IslandCenter
    }
}
