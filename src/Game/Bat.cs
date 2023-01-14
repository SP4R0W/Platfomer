using Godot;
using System;

public class Bat : Area2D
{
	[Signal]
	public delegate void Hit(Bat area);

    private Timer timer;
	private Tween tween;
	private AnimatedSprite animSprite;
	private CollisionShape2D collisionShape;
    private RayCast2D playerRay1;
    private RayCast2D playerRay2;

    private VisibilityNotifier2D vis;

	private int flyDuration = 1;

	private Vector2 initialPos;

    private bool isChasing = false;

    private bool isDead = false;

    public override void _Ready()
    {
        initialPos = GlobalPosition;

        vis = GetNode<VisibilityNotifier2D>("VisibilityNotifier2D");

        playerRay1 = GetNode<RayCast2D>("RayCast2D");
        playerRay2 = GetNode<RayCast2D>("RayCast2D2");

        animSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

        tween = GetNode<Tween>("Tween");
        timer = GetNode<Timer>("MoveTimer");    

        animSprite.Animation = "stand";

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
	}

    public void Kill()
    {
        isDead = true;
        tween.StopAll();
        tween.RemoveAll();
        collisionShape.SetDeferred("disabled",true);
        playerRay1.Enabled = false;
        playerRay2.Enabled = false;
        timer.Stop();
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

    private void ChasePlayer()
    {   
        if (!isDead)
        {
            var playerPos = Global.playerPos;

            tween.InterpolateProperty(this,"global_position",GlobalPosition,new Vector2(playerPos.x,playerPos.y - 64),flyDuration,Tween.TransitionType.Linear,Tween.EaseType.InOut);
            tween.Start();

            GetTree().CreateTimer(2f).Connect("timeout",this,"FlyBack");
        }
    }

    private void FlyBack()
    {
        if (!isDead)
        {
            if (!animSprite.FlipH)
                animSprite.FlipH = true;
            else
                animSprite.FlipH = false;

            tween.InterpolateProperty(this,"global_position",GlobalPosition,initialPos,flyDuration,Tween.TransitionType.Linear,Tween.EaseType.InOut);
            tween.Start();

            GetTree().CreateTimer(1.1f).Connect("timeout",this,"EndChase");
        }
    }

    private void EndChase()
    {
        if (!isDead)
        {
            animSprite.Animation = "stand";

            isChasing = false;
        }
    }

	public override void _PhysicsProcess(float delta)
	{
        if (playerRay1.IsColliding() && !isDead)
        {
            var collider = playerRay1.GetCollider();
            if (((Node)collider).Name == "Player")
            {
                if (!isChasing)
                {
                    animSprite.FlipH = false;
                    isChasing = true;
                    animSprite.Animation = "fly";
                    if (Global.isAnimOn)
                    {
                        animSprite.Play();
                    }
                    ChasePlayer();
                }
            }
        }
        else if (playerRay2.IsColliding() && !isDead)
        {
            var collider = playerRay2.GetCollider();
            if (((Node)collider).Name == "Player")
            {
                if (!isChasing)
                {
                    animSprite.FlipH = true;
                    isChasing = true;
                    animSprite.Animation = "fly";
                    if (Global.isAnimOn)
                    {
                        animSprite.Play();
                    }           
                    ChasePlayer();
                }
            }
        }
        else
        {
            if (!isChasing && !isDead)
            {
                animSprite.Animation = "stand";
            }
        }
	}

    private void ScreenEntered()
    {
        if (!isDead)
        {
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
            if (Global.isAnimOn)
            {
                animSprite.Stop();
            }
        }
    }

}
