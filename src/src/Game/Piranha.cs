using Godot;
using System;

public class Piranha : Area2D
{
	[Signal]
	public delegate void Hit(Piranha area, AnimatedSprite animSprite);

    [Export]
    int jumpValue = 512;

	Timer timer;
	Tween tween;
	AnimatedSprite animSprite;
	CollisionShape2D collisionShape;
	VisibilityNotifier2D vis;

	Vector2 initialPos;

	bool isDead = false;

	public override void _Ready()
	{
		vis = GetNode<VisibilityNotifier2D>("VisibilityNotifier2D");
		timer = GetNode<Timer>("JumpTimer");
		tween = GetNode<Tween>("Tween");
		animSprite = GetNode<AnimatedSprite>("AnimatedSprite");
		collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

		initialPos = GlobalPosition; // Move to the starting position

		if (vis.IsOnScreen())
			timer.Start();
	}

	void JumpOut()
	{
		// If still alive then jump out of the water
		if (!isDead)
		{
			animSprite.Animation = "jump";
			tween.InterpolateProperty(this,"global_position:y",initialPos.y,initialPos.y - jumpValue,.8f,Tween.TransitionType.Linear,Tween.EaseType.InOut);
			tween.Start();

			GetTree().CreateTimer(1f).Connect("timeout",this,"JumpBack"); // Wait a sec then jump back
		}
	}

	void JumpBack()
	{
		// If still alive then jump back into the water
		if (!isDead)
		{
			animSprite.Animation = "fall";
			tween.InterpolateProperty(this,"global_position:y",(initialPos.y - jumpValue),(initialPos.y - jumpValue) + jumpValue,.8f,Tween.TransitionType.Linear,Tween.EaseType.InOut);
			tween.Start();

			GetTree().CreateTimer(1f).Connect("timeout",this,"StartTimer"); // Wait a sec then start the whole process again
		}
	}

	void StartTimer()
	{
		// Wait before starting another jump
		if (!isDead)
			timer.Start();
	}

	public void Kill()
	{
		// Kill the fish if player jumped on its head
		isDead = true;

		tween.RemoveAll();
		collisionShape.SetDeferred("disabled",true);
		timer.Stop();
		animSprite.Animation = "dead";

		tween.InterpolateProperty(this,"global_position:y",GlobalPosition.y,GlobalPosition.y+1000,2,Tween.TransitionType.Linear,Tween.EaseType.In);
		tween.Start(); // Move down then remove

        GetTree().CreateTimer(2.25f).Connect("timeout",this,"Remove");
    }

    void Remove()
    {
        QueueFree();
    }

	void PlayerEntered(Node body)
	{
		EmitSignal("Hit",this,animSprite); // Emit the hit signal if touched player (from the sides)
	}

    void ScreenEntered()
    {
		if (!isDead)
			timer.Start();
    }

    void ScreenLeft()
    {
		if (!isDead)
			timer.Stop();
    }
}
