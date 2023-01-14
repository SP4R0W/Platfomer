using Godot;
using System;

public class Spinner : KinematicBody2D
{
	[Signal]
	public delegate void Hit(Spinner body);

	private Tween tween;
	private AnimatedSprite animSprite;
	private CollisionShape2D collisionShape;
    private RayCast2D playerRay;
    private RayCast2D wallRay;

	private int direction = -1;

	private int speed;

    private int normalSpeed = 150;
    private int chaseSpeed = 250;

    private float gravity;
    private float jumpVel;
    private float jumpHeight = 2f * 64;
    private float jumpDuration = 0.5f;


	private Vector2 velocity = Vector2.Zero;

    private bool playerSeen = false;

    private VisibilityNotifier2D vis;

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
        {
            animSprite.Play();
        }
    }

	private void Collision(KinematicCollision2D body)
	{
		if (((Node)body.Collider).Name == "Player")
		{
            EmitSignal("Hit",this);
		}
	}

    public void Kill()
    {
        collisionShape.SetDeferred("disabled",true);
        playerRay.Enabled = false;
        speed = 0;
		animSprite.Animation = "dead";

		tween.InterpolateProperty(this,"position:y",Position.y,Position.y+1000,2,Tween.TransitionType.Linear,Tween.EaseType.In);
		tween.Start();

        GetTree().CreateTimer(2.25f).Connect("timeout",this,"Remove");
    }

    private void Remove()
    {
        QueueFree();
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionPressed("jump") && IsOnFloor() && playerSeen)
        {
            velocity.y = jumpVel;
        }

        if (wallRay.IsColliding())
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
        if (playerRay.IsColliding())
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

		velocity.x = speed * direction;
        velocity.y += gravity * delta;

        if (vis.IsOnScreen())
        {
            velocity = MoveAndSlide(velocity,new Vector2(0,-1),true);   
        }

        for (int i = 0;i < GetSlideCount();i++)
        {
            var kinCollision = GetSlideCollision(i);
            Collision(kinCollision);
        }
	}

    private void ScreenEntered()
    {
        if (Global.isAnimOn)
        {
            animSprite.Play();
        }
    }

    private void ScreenLeft()
    {
        if (Global.isAnimOn)
        {
            animSprite.Stop();
        }
    }
}
