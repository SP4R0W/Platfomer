using Godot;
using System;

public class Spinner : KinematicBody2D
{
	[Signal]
	public delegate void Hit(Spinner body);

	Tween tween;
	AnimatedSprite animSprite;
	CollisionShape2D collisionShape;
    RayCast2D playerRay;
    RayCast2D wallRay;

	int direction = -1;

	int speed;

    int normalSpeed = 150;
    int chaseSpeed = 250;

    float gravity;
    float jumpVel;
    float jumpHeight = 2f * 64;
    float jumpDuration = 0.5f;


	Vector2 velocity = Vector2.Zero;

    bool playerSeen = false;

    VisibilityNotifier2D vis;

    public override void _Ready()
    {
        vis = GetNode<VisibilityNotifier2D>("VisibilityNotifier2D");

        wallRay = GetNode<RayCast2D>("WallRay");
        playerRay = GetNode<RayCast2D>("PlayerRay");

        animSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        tween = GetNode<Tween>("Tween");

        speed = normalSpeed;

        gravity = 2 * jumpHeight / Mathf.Pow(jumpDuration,2);
        jumpVel = -Mathf.Sqrt(2 * gravity * jumpHeight);

        animSprite.Animation = "spin";

        if (vis.IsOnScreen() && Global.isAnimOn)
            animSprite.Play();
    }

	void Collision(KinematicCollision2D body)
	{
		if (((Node)body.Collider).Name == "Player")
            EmitSignal("Hit",this); // Signal for colliding with player
	}

    public void Kill()
    {
        // Spinner can only die if we have immunity on
        collisionShape.SetDeferred("disabled",true);
        playerRay.Enabled = false;
        speed = 0;
		animSprite.Animation = "dead";

		tween.InterpolateProperty(this,"position:y",Position.y,Position.y+1000,2,Tween.TransitionType.Linear,Tween.EaseType.In);
		tween.Start(); // Move down and remove

        GetTree().CreateTimer(2.25f).Connect("timeout",this,"Remove");
    }

    void Remove()
    {
        QueueFree();
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionPressed("jump") && IsOnFloor() && playerSeen)
            velocity.y = jumpVel; // Jump when sees player, player is jumping and the spinner is on floor.

        if (wallRay.IsColliding()) // Change direction if spinner hit a wall
        {
			if (direction == 1)
			{
				direction = -1;
                playerRay.Rotation = Mathf.Deg2Rad(0);
                wallRay.Rotation = Mathf.Deg2Rad(0);
			}
			else
			{
				direction = 1;
                playerRay.Rotation = Mathf.Deg2Rad(180);
                wallRay.Rotation = Mathf.Deg2Rad(180);
			}
        }
    }

	public override void _PhysicsProcess(float delta)
	{
        if (playerRay.IsColliding()) // Check if the ray sees player
        {
            var collider = playerRay.GetCollider();
            if (((Node)collider).Name == "Player")
            {
                playerSeen = true;
                speed = chaseSpeed;
            }
            else
            {
                playerSeen = false;
                speed = normalSpeed;
            }
        }
        else
        {
            playerSeen = false;
            speed = normalSpeed;
        }

        // Move the spinner
		velocity.x = speed * direction;
        velocity.y += gravity * delta;

        if (vis.IsOnScreen())
            velocity = MoveAndSlide(velocity,new Vector2(0,-1),true);

        for (int i = 0;i < GetSlideCount();i++)
        {
            var kinCollision = GetSlideCollision(i);
            Collision(kinCollision);
        }
	}

    void ScreenEntered()
    {
        if (Global.isAnimOn)
            animSprite.Play();
    }

    void ScreenLeft()
    {
        if (Global.isAnimOn)
            animSprite.Stop();
    }
}
