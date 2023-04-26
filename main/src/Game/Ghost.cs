using Godot;
using System;

public class Ghost : Area2D
{
	[Signal]
	public delegate void Hit(Ghost area);

    Timer timer;
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

    int ySpeed = 5;

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
        timer = GetNode<Timer>("MoveTimer");

        speed = normalSpeed;

        if (vis.IsOnScreen())
        {
            timer.Start(); // This timer is responsible for y axis movement
            if (Global.isAnimOn)
                animSprite.Play();
        }
    }

	void Collision(Node body)
	{
		if (body.Name == "Player")
            EmitSignal("Hit",this); // Emit the hit signal if touched player (from the sides)
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
        // Ghost can only die if we have immunity on

        collisionShape.SetDeferred("disabled",true);
        playerRay.Enabled = false;
        timer.Stop();
        speed = 0;
		animSprite.Animation = "dead";
        Rotation = Mathf.Deg2Rad(180); // Turn the ghost upside down

		tween.InterpolateProperty(this,"position:y",Position.y,Position.y+1000,2,Tween.TransitionType.Linear,Tween.EaseType.In);
		tween.Start(); // Move down then remove

        GetTree().CreateTimer(2.25f).Connect("timeout",this,"Remove");
    }

    void Remove()
    {
        QueueFree();
    }

    void ChangeY()
    {
        ySpeed *= -1;
    }

	public override void _PhysicsProcess(float delta)
	{
         // If found no ground on one side then change direction
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

        if (!ray1.IsColliding() && !ray2.IsColliding()) // If found no ground then kill (prevents flying ghosts)
            Kill();

        if (playerRay.IsColliding())
        {
            var collider = playerRay.GetCollider(); // If spotted player then move faster and change appearance
            if (((Node)collider).Name == "Player")
            {
                speed = chaseSpeed;
                animSprite.Animation = "scared";
            }
            else
            {
                speed = normalSpeed;
                animSprite.Animation = "normal";
            }
        }
        else
        {
            speed = normalSpeed;
            animSprite.Animation = "normal";
        }

        // Move the ghost
		velocity.x = speed * direction;
        if (vis.IsOnScreen())
            velocity.y = ySpeed;

		GlobalPosition += velocity * delta;
	}


    void ScreenEntered()
    {
        timer.Start(); // Start the timer for y movement
        if (Global.isAnimOn)
            animSprite.Play();
    }

    void ScreenLeft()
    {
        timer.Stop(); // Stop the timer for y movement
        if (Global.isAnimOn)
            animSprite.Stop();
    }
}
