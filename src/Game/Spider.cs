using Godot;
using System;

public class Spider : Area2D
{
	[Signal]
	public delegate void Hit(Spider area);

	private Tween tween;
	private AnimatedSprite animSprite;
	private CollisionShape2D collisionShape;
	private RayCast2D ray1;
	private RayCast2D ray2;
    private RayCast2D playerRay;

	private VisibilityNotifier2D vis;

	private int direction = -1;

	private int speed;

    private int normalSpeed = 75;
    private int chaseSpeed = 300;

	private Vector2 velocity = Vector2.Zero;

    public override void _Ready()
    {
		vis = GetNode<VisibilityNotifier2D>("VisibilityNotifier2D");

		ray1 = GetNode<RayCast2D>("RayCast1");
		ray2 = GetNode<RayCast2D>("RayCast2");
        playerRay = GetNode<RayCast2D>("RayCast3");

        animSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        tween = GetNode<Tween>("Tween");   

        speed = normalSpeed;

        animSprite.Animation = "walk";
		if (vis.IsOnScreen() && Global.isAnimOn)
		{
			animSprite.Play();
		}
    }

	private void Collision(Node body)
	{
		if (body.Name == "Player")
		{
            EmitSignal("Hit",this);
		}
		else
		{
			if (direction == 1)
			{
				direction = -1;
                playerRay.Rotation = Mathf.Deg2Rad(0);
                animSprite.FlipH = false;
			}
			else
			{
				direction = 1;
                playerRay.Rotation = Mathf.Deg2Rad(180);
                animSprite.FlipH = true;
			}
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

	public override void _PhysicsProcess(float delta)
	{
		if (!ray1.IsColliding())
		{
			direction = 1;
            playerRay.Rotation = Mathf.Deg2Rad(180);
            animSprite.FlipH = true;
		}

		if (!ray2.IsColliding())
		{
			direction = -1;
            playerRay.Rotation = Mathf.Deg2Rad(0);
            animSprite.FlipH = false;
		}

        if (playerRay.IsColliding())
        {
            var collider = playerRay.GetCollider();
            if (((Node)collider).Name == "Player")
            {
                speed = chaseSpeed;
            }
            else
            {
                speed = normalSpeed;
            }
        }
        else
        {
            speed = normalSpeed;
        }

		velocity.x = speed * direction;

		GlobalPosition += velocity * delta;
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
