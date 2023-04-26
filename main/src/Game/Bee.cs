using Godot;
using System;

public class Bee : Area2D
{
	[Signal]
	public delegate void Hit(Bee area);

    Timer timer;
	Tween tween;
	AnimatedSprite animSprite;
	CollisionShape2D collisionShape;
    RayCast2D playerRay;

    VisibilityNotifier2D vis;

	int direction = -1;

	int speed;

    int normalSpeed = 100;

    bool isChasing = false;

    int ySpeed = 10;

	Vector2 velocity = Vector2.Zero;
    Vector2 initialPos;

    bool isDead = false;

    public override void _Ready()
    {
        vis = GetNode<VisibilityNotifier2D>("VisibilityNotifier2D");
        playerRay = GetNode<RayCast2D>("RayCast2D");

        animSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        tween = GetNode<Tween>("Tween");
        timer = GetNode<Timer>("MoveTimer");

        animSprite.Animation = "fly";

        if (vis.IsOnScreen())
        {
            if (Global.isAnimOn)
                animSprite.Play();

            timer.Start(); // This timer is responsible for y axis movement
        }

        speed = normalSpeed;
    }

	void Collision(Node body)
	{
		if (body.Name == "Player")
            EmitSignal("Hit",this); // Emit the hit signal if touched player (from the sides or when teeth are open)
		else
		{
			if (direction == 1) // Change direction if touched a wall
			{
				direction = -1;
                playerRay.Rotation = Mathf.Deg2Rad(225);
                animSprite.FlipH = false;
			}
			else
			{
				direction = 1;
                playerRay.Rotation = Mathf.Deg2Rad(-225);
                animSprite.FlipH = true;
			}
		}
	}

    public void Kill()
    {
        isDead = true;

        tween.StopAll();
        tween.RemoveAll();
        collisionShape.SetDeferred("disabled",true);
        playerRay.Enabled = false;
        timer.Stop();
        speed = 0;
		animSprite.Animation = "dead";
        Rotation = Mathf.Deg2Rad(180);

		tween.InterpolateProperty(this,"position:y",Position.y,Position.y+2000,2,Tween.TransitionType.Linear,Tween.EaseType.In);
		tween.Start(); // Move way down and remove

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

    void ChasePlayer()
    {
        if (!isDead)
        {
            timer.Paused = true;
            initialPos = GlobalPosition;

            var playerPos = Global.playerPos;
            var desired = new Vector2(playerPos.x,playerPos.y - 64);
            var time = Mathf.Clamp((desired.y - initialPos.y)/2000,0.5f,1);

            tween.InterpolateProperty(this,"global_position",GlobalPosition,desired,time,Tween.TransitionType.Linear,Tween.EaseType.InOut);
            tween.Start(); // Chase the player to the position it has been spotted at

            GetTree().CreateTimer(1.2f).Connect("timeout",this,"GoBack");
        }
    }

    void GoBack()
    {
        if (!isDead)
        {
            var time = Mathf.Clamp((GlobalPosition.y - initialPos.y)/2000,0.5f,1);
            tween.InterpolateProperty(this,"global_position",GlobalPosition,initialPos,time,Tween.TransitionType.Linear,Tween.EaseType.InOut);
            tween.Start(); // Move bee back to previous position

            GetTree().CreateTimer(1.2f).Connect("timeout",this,"StopChase");
        }
        else
            isChasing = false;
    }

    void StopChase()
    {
        isChasing = false;
        timer.Paused = false;
    }

	public override void _PhysicsProcess(float delta)
	{
        if (playerRay.IsColliding() && !isDead) // If spotted player and is not chasing already then chase them
        {
            var collider = playerRay.GetCollider();
            if (((Node)collider).Name == "Player")
            {
                if (!isChasing)
                {
                    isChasing = true;
                    ChasePlayer();
                }
            }
        }
        else
            animSprite.Animation = "fly";

        if (!isChasing && !isDead)
        {
            // Move the bee if its not chasing
            velocity.x = speed * direction;

            if (!timer.IsStopped())
                velocity.y = ySpeed;

		    GlobalPosition += velocity * delta;
        }
	}

    void ScreenEntered()
    {
        if (!isDead)
        {
            timer.Start();
            if (Global.isAnimOn)
                animSprite.Play();
        }
    }

    void ScreenLeft()
    {
        if (!isDead)
        {
            timer.Stop();
            if (Global.isAnimOn)
                animSprite.Stop();
        }
    }

}

