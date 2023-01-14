using Godot;
using System;

public class Piranha : Area2D
{
	[Signal]
	public delegate void Hit(Piranha area, AnimatedSprite animSprite);

    [Export]
    private int jumpValue = 512;

	private Timer timer;
	private Tween tween;
	private AnimatedSprite animSprite;
	private CollisionShape2D collisionShape;
	private VisibilityNotifier2D vis;

	private Vector2 initialPos;

	private bool isDead = false;

	public override void _Ready()
	{
		vis = GetNode<VisibilityNotifier2D>("VisibilityNotifier2D");
		timer = GetNode<Timer>("JumpTimer");
		tween = GetNode<Tween>("Tween");
		animSprite = GetNode<AnimatedSprite>("AnimatedSprite");
		collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

		initialPos = GlobalPosition;

		if (vis.IsOnScreen())
		{
			timer.Start();
		}
	}

	private void JumpOut()
	{
		if (!isDead)
		{
			animSprite.Animation = "jump";
			tween.InterpolateProperty(this,"global_position:y",initialPos.y,initialPos.y - jumpValue,.8f,Tween.TransitionType.Linear,Tween.EaseType.InOut);
			tween.Start();

			GetTree().CreateTimer(1f).Connect("timeout",this,"JumpBack");
		}
	}

	private void JumpBack()
	{
		if (!isDead)
		{
			animSprite.Animation = "fall";
			tween.InterpolateProperty(this,"global_position:y",(initialPos.y - jumpValue),(initialPos.y - jumpValue) + jumpValue,.8f,Tween.TransitionType.Linear,Tween.EaseType.InOut);
			tween.Start();

			GetTree().CreateTimer(1f).Connect("timeout",this,"StartTimer");
		}
	}

	private void StartTimer()
	{
		if (!isDead)
		{
			timer.Start();
		}
	}

	public void Kill()
	{
		isDead = true;

		tween.RemoveAll();
		collisionShape.SetDeferred("disabled",true);
		timer.Stop();
		animSprite.Animation = "dead";

		tween.InterpolateProperty(this,"global_position:y",GlobalPosition.y,GlobalPosition.y+1000,2,Tween.TransitionType.Linear,Tween.EaseType.In);
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
		if (!isDead)
		{
			timer.Start();
		}
    }

    private void ScreenLeft()
    {
		if (!isDead)
		{
			timer.Stop();
		}
    }
}
