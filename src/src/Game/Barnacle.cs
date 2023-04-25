using Godot;
using System;

public class Barnacle : Area2D
{

	[Signal]
	public delegate void Hit(Barnacle area, AnimatedSprite animSprite);

	Timer showTimer;
	Timer hideTimer;
	Tween tween;
	AnimatedSprite animSprite;
	CollisionShape2D collisionShape;

	VisibilityNotifier2D vis;

	public override void _Ready()
	{
		vis = GetNode<VisibilityNotifier2D>("VisibilityNotifier2D");

		Position = new Vector2(Position.x,Position.y + 63);

		showTimer = GetNode<Timer>("ShowTimer");
		hideTimer = GetNode<Timer>("HideTimer");
		tween = GetNode<Tween>("Tween");
		animSprite = GetNode<AnimatedSprite>("AnimatedSprite");
		collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

		if (vis.IsOnScreen())
		{
			if (Global.isAnimOn)
				animSprite.Play();
		}

		showTimer.Start();
	}

	void ShowEnemy()
	{
		// Move the barnacle from the ground
		animSprite.Animation = "closed";

		tween.InterpolateProperty(this,"position:y",Position.y,Position.y - 63,2f);
		tween.Start();

		GetTree().CreateTimer(2f).Connect("timeout",this,"EndShow");

	}

	void EndShow()
	{
		// Open the barnacle's teeth
		animSprite.Animation = "open";
		hideTimer.Start();
	}

	void HideEnemy()
	{
		// Hide the barnacle
		animSprite.Animation = "closed";

		tween.InterpolateProperty(this,"position:y",Position.y,Position.y + 63,2f);
		tween.Start();

		GetTree().CreateTimer(2f).Connect("timeout",this,"EndHide");
	}

	void EndHide()
	{
		// Close barnacle's teeth
		animSprite.Animation = "closed";
		showTimer.Start();
	}

	public void Attack()
	{
		hideTimer.Paused = true;
		animSprite.Animation = "closed";

		GetTree().CreateTimer(0.2f).Connect("timeout",this,"EndAttack");
	}

	void EndAttack()
	{
		animSprite.Animation = "open";
		hideTimer.Paused = false;
	}

	public void Kill()
	{
		collisionShape.SetDeferred("disabled",true);
		showTimer.Stop();
		hideTimer.Stop();
		ZIndex = 1;
		animSprite.Animation = "dead";
		Rotation = Mathf.Deg2Rad(180);

		tween.InterpolateProperty(this,"position:y",Position.y,Position.y+1000,2,Tween.TransitionType.Linear,Tween.EaseType.In);
		tween.Start(); // Move the barnacle down and remove

        GetTree().CreateTimer(2.25f).Connect("timeout",this,"Remove");
    }

    void Remove()
    {
        QueueFree();
    }

	void PlayerEntered(Node body)
	{
		EmitSignal("Hit",this,animSprite); // Emit the hit signal if touched player (from the sides or when teeth are open)
	}

    void ScreenEntered()
    {
        if (Global.isAnimOn)
        {
            animSprite.Play();
        }
    }

    void ScreenLeft()
    {
        if (Global.isAnimOn)
        {
            animSprite.Stop();
        }
    }
}
