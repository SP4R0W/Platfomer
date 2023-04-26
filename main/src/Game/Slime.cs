using Godot;
using System;

public class Slime : Area2D
{

	[Signal]
	public delegate void Hit(Slime area);

	Tween tween;
	AnimatedSprite animSprite;
	CollisionShape2D collisionShape;
	RayCast2D ray1;
	RayCast2D ray2;

	int direction = -1;

	int speed = 45;

	Vector2 velocity = Vector2.Zero;

	VisibilityNotifier2D vis;

	public override void _Ready()
	{
		vis = GetNode<VisibilityNotifier2D>("VisibilityNotifier2D");
        GlobalPosition = new Vector2(GlobalPosition.x,GlobalPosition.y-2);

		ray1 = GetNode<RayCast2D>("RayCast1");
		ray2 = GetNode<RayCast2D>("RayCast2");

        animSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        tween = GetNode<Tween>("Tween");

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
                animSprite.FlipH = false;
			}
			else
			{
				direction = 1;
                animSprite.FlipH = true;
			}
		}
	}

    public void Kill()
    {
        speed = 0;
		collisionShape.SetDeferred("disabled",true);
		animSprite.Animation = "dead";
		Rotation = Mathf.Deg2Rad(180);

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
		// Change direction if the rays can't find ground anymore
		if (!ray1.IsColliding())
		{
			direction = 1;
            animSprite.FlipH = true;
		}

		if (!ray2.IsColliding())
		{
			direction = -1;
            animSprite.FlipH = false;
		}

		// Move the slime
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
