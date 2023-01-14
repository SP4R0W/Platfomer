using Godot;
using System;

public class Bee : Area2D
{
	[Signal]
	public delegate void Hit(Bee area);

    private Timer timer;
	private Tween tween;
	private AnimatedSprite animSprite;
	private CollisionShape2D collisionShape;
    private RayCast2D playerRay;

    private VisibilityNotifier2D vis;

	private int direction = -1;

	private int speed;

    private int normalSpeed = 100;

    private bool isChasing = false;

    private int ySpeed = 10;

	private Vector2 velocity = Vector2.Zero;
    private Vector2 initialPos;

    private bool isDead = false;

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
            {
                animSprite.Play();
            }
            timer.Start();
        }

        speed = normalSpeed;
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
		tween.Start();

        GetTree().CreateTimer(2.25f).Connect("timeout",this,"Remove");
    }

    private void Remove()
    {
        QueueFree();
    }

    private void ChangeY()
    {
        ySpeed *= -1;
    }

    private void ChasePlayer()
    {   
        if (!isDead)
        {
            timer.Paused = true;
            initialPos = GlobalPosition;

            var playerPos = Global.playerPos;
            var desired = new Vector2(playerPos.x,playerPos.y - 64);
            var time = Mathf.Clamp((desired.y - initialPos.y)/2000,0.5f,1);

            tween.InterpolateProperty(this,"global_position",GlobalPosition,desired,time,Tween.TransitionType.Linear,Tween.EaseType.InOut);
            tween.Start();

            GetTree().CreateTimer(1.2f).Connect("timeout",this,"GoBack");
        }
    }

    private void GoBack()
    {
        if (!isDead)
        {
            var time = Mathf.Clamp((GlobalPosition.y - initialPos.y)/2000,0.5f,1);
            tween.InterpolateProperty(this,"global_position",GlobalPosition,initialPos,time,Tween.TransitionType.Linear,Tween.EaseType.InOut);
            tween.Start();

            GetTree().CreateTimer(1.2f).Connect("timeout",this,"StopChase");
        }
        else
        {
            isChasing = false;
        }
    }

    private void StopChase()
    {
        isChasing = false;
        timer.Paused = false;
    }

	public override void _PhysicsProcess(float delta)
	{
        if (playerRay.IsColliding() && !isDead)
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
        {
            animSprite.Animation = "fly";
        }

        if (!isChasing && !isDead)
        {
            velocity.x = speed * direction;
            if (!timer.IsStopped())
            {
                velocity.y = ySpeed;
            }

		    GlobalPosition += velocity * delta;
        }
	}

    private void ScreenEntered()
    {
        if (!isDead)
        {
            timer.Start();
            if (Global.isAnimOn)
            {
                animSprite.Play();
            }
        }
    }

    private void ScreenLeft()
    {
        if (!isDead)
        {
            timer.Stop();
            if (Global.isAnimOn)
            {
                animSprite.Stop();
            }
        }
    }

}

