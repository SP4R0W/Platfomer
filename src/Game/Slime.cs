using Godot;
using System;

public class Slime : Area2D
{

	[Signal]
	public delegate void Hit(Slime area);

	private Tween tween;
	private AnimatedSprite animSprite;
	private CollisionShape2D collisionShape;
	private RayCast2D ray1;
	private RayCast2D ray2;

	private int direction = -1;

	private int speed = 45;

	private Vector2 velocity = Vector2.Zero;

	private VisibilityNotifier2D vis;

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

    private void Remove()
    {
        QueueFree();
    }
	public override void _PhysicsProcess(float delta)
	{
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
