using Godot;
using System;

public class Spider : Area2D
{
	[Signal]
	public delegate void Hit(Spider area);

	Tween tween;
	AnimatedSprite animSprite;
	CollisionShape2D collisionShape;
	RayCast2D ray1;
	RayCast2D ray2;
    RayCast2D playerRay;

	VisibilityNotifier2D vis;

	int direction = -1;

	int speed;

    int normalSpeed = 75;
    int chaseSpeed = 300;

	Vector2 velocity = Vector2.Zero;

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
			animSprite.Play();
    }

	void Collision(Node body)
	{
		if (body.Name == "Player")
            EmitSignal("Hit",this); // Emit the hit signal if touched player (from the sides)
		else
		{
			if (direction == 1) // If touched a wall then change direction
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
        // Kill the spider if player jumped on its head
        collisionShape.SetDeferred("disabled",true);
        playerRay.Enabled = false;
        speed = 0;
		animSprite.Animation = "dead";

		tween.InterpolateProperty(this,"position:y",Position.y,Position.y+1000,2,Tween.TransitionType.Linear,Tween.EaseType.In);
		tween.Start();

        GetTree().CreateTimer(2.25f).Connect("timeout",this,"Remove");
    }

    void Remove()
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
            if (((Node)collider).Name == "Player") // If spotted a player then speed up
                speed = chaseSpeed;
            else
                speed = normalSpeed;
        }
        else
            speed = normalSpeed;

        // Move the spider
		velocity.x = speed * direction;

		GlobalPosition += velocity * delta;
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
