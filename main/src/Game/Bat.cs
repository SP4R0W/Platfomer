using Godot;
using System;

public class Bat : Area2D
{
	[Signal]
	public delegate void Hit(Bat area);

	Tween tween;
	AnimatedSprite animSprite;
	CollisionShape2D collisionShape;
    RayCast2D playerRay1;
    RayCast2D playerRay2;

    VisibilityNotifier2D vis;

	int flyDuration = 1;

	Vector2 initialPos;

    bool isChasing = false;

    bool isDead = false;

    public override void _Ready()
    {
        initialPos = GlobalPosition;

        vis = GetNode<VisibilityNotifier2D>("VisibilityNotifier2D");

        playerRay1 = GetNode<RayCast2D>("RayCast2D");
        playerRay2 = GetNode<RayCast2D>("RayCast2D2");

        animSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

        tween = GetNode<Tween>("Tween");

        animSprite.Animation = "stand";

        if (vis.IsOnScreen() && Global.isAnimOn)
            animSprite.Play();
    }

	void Collision(Node body)
	{
		if (body.Name == "Player")
            EmitSignal("Hit",this); // Emit the hit signal if touched player (from the sides)
	}

    public void Kill()
    {
        isDead = true;
        tween.StopAll();
        tween.RemoveAll();
        collisionShape.SetDeferred("disabled",true);
        playerRay1.Enabled = false;
        playerRay2.Enabled = false;

		animSprite.Animation = "dead";
        Rotation = Mathf.Deg2Rad(180);

		tween.InterpolateProperty(this,"position:y",Position.y,Position.y+2000,2,Tween.TransitionType.Linear,Tween.EaseType.In);
		tween.Start(); // Move the bat way down and remove

        GetTree().CreateTimer(2.25f).Connect("timeout",this,"Remove");
    }

    void Remove()
    {
        QueueFree();
    }

    void ChasePlayer()
    {
        if (!isDead)
        {
            var playerPos = Global.playerPos;

            tween.InterpolateProperty(this,"global_position",GlobalPosition,new Vector2(playerPos.x,playerPos.y - 64),flyDuration,Tween.TransitionType.Linear,Tween.EaseType.InOut);
            tween.Start(); // Chase the player to the position it has been spotted at

            GetTree().CreateTimer(2f).Connect("timeout",this,"FlyBack");
        }
    }

    void FlyBack()
    {
        if (!isDead)
        {
            if (!animSprite.FlipH)
                animSprite.FlipH = true;
            else
                animSprite.FlipH = false;

            tween.InterpolateProperty(this,"global_position",GlobalPosition,initialPos,flyDuration,Tween.TransitionType.Linear,Tween.EaseType.InOut);
            tween.Start();  // Move bat back to previous position

            GetTree().CreateTimer(1.1f).Connect("timeout",this,"EndChase");
        }
    }

    void EndChase()
    {
        if (!isDead)
        {
            animSprite.Animation = "stand";

            isChasing = false;
        }
    }

	public override void _PhysicsProcess(float delta)
	{
        if (playerRay1.IsColliding() && !isDead) // If spotted player and is not chasing already then chase them
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
                        animSprite.Play();

                    ChasePlayer();
                }
            }
        }
        else if (playerRay2.IsColliding() && !isDead) // If spotted player and is not chasing already then chase them
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
                        animSprite.Play();

                    ChasePlayer();
                }
            }
        }
        else
        {
            if (!isChasing && !isDead)
                animSprite.Animation = "stand";
        }
	}

    void ScreenEntered()
    {
        if (!isDead)
        {
            if (Global.isAnimOn)
                animSprite.Play();
        }
    }

    void ScreenLeft()
    {

        if (!isDead)
        {
            if (Global.isAnimOn)
                animSprite.Stop();
        }
    }

}
