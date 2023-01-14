using Godot;
using System;

public class Barnacle : Area2D
{

	[Signal]
	public delegate void Hit(Barnacle area, AnimatedSprite animSprite);

	private Timer showTimer;
	private Timer hideTimer;
	private Tween tween;
	private AnimatedSprite animSprite;
	private CollisionShape2D collisionShape;

	private VisibilityNotifier2D vis;

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
			{
				animSprite.Play();
			}
		}

		showTimer.Start();
	}

	private void ShowEnemy()
	{

		animSprite.Animation = "closed";

		tween.InterpolateProperty(this,"position:y",Position.y,Position.y - 63,2f);
		tween.Start();

		GetTree().CreateTimer(2f).Connect("timeout",this,"EndShow");

	}

	private void EndShow()
	{
		animSprite.Animation = "open";
		hideTimer.Start();
	}

	private void HideEnemy()
	{
			animSprite.Animation = "closed";

			tween.InterpolateProperty(this,"position:y",Position.y,Position.y + 63,2f);
			tween.Start();

			GetTree().CreateTimer(2f).Connect("timeout",this,"EndHide");
	}

	private void EndHide()
	{
		animSprite.Animation = "closed";
		showTimer.Start();
	}

	public void Attack()
	{
		hideTimer.Paused = true;
		animSprite.Animation = "closed";
		
		GetTree().CreateTimer(0.2f).Connect("timeout",this,"EndAttack");
	}

	private void EndAttack()
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
		tween.Start();

        GetTree().CreateTimer(2.25f).Connect("timeout",this,"Remove");
    }

    private void Remove()
    {
        QueueFree();
    }

	private void PlayerEntered(Node body)
	{
		EmitSignal("Hit",this,animSprite);
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
