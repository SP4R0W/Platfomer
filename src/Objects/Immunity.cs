using Godot;
using System;

public class Immunity : Area2D
{

    private Timer moveTimer;
	private CollisionShape2D collisionShape;

    private int ySpeed = 50;

    private VisibilityNotifier2D vis;

    private Vector2 velocity = Vector2.Zero;

    private Tween posTween;
    private Tween visTween;

    public override void _Ready()
    {
        posTween = GetNode<Tween>("PosTween");
        visTween = GetNode<Tween>("VisTween");
    
        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

        moveTimer = GetNode<Timer>("MoveTimer");
        vis = GetNode<VisibilityNotifier2D>("VisibilityNotifier2D");


        if (vis.IsOnScreen())
        {
            moveTimer.Start();
        }
        else
        {
            moveTimer.Stop();
        }
    }

    private void Touched(Node body)
    {
        if (body.Name == "Player")
        {
            
            var immunityTimer = GetTree().Root.GetNode<Timer>("Area/ImmunityDuration");
            immunityTimer.Start();

            moveTimer.Stop();

            collisionShape.SetDeferred("disabled",true);

            var sound = GetNode<AudioStreamPlayer>("ImmuneSound");
            if (Global.isSfxOn)
            {
                sound.Play();
            }

            Global.isImmune = true;

            posTween.InterpolateProperty(this,"position:y",Position.y,Position.y-100,1);
            visTween.InterpolateProperty(this,"modulate:a",1.0,0,1);

            posTween.Start();
            visTween.Start();

            GetTree().CreateTimer(1.25f).Connect("timeout",this,"Kill");
        }
    }

    private void Kill()
    {
        QueueFree();
    }

    private void ChangeY()
    {
        ySpeed *= -1;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (vis.IsOnScreen() && Global.isAnimOn)
        {
            velocity.y = ySpeed;

		    GlobalPosition += velocity * delta;
        }
    }

    private void ScreenEntered()
    {
        moveTimer.Start();
    }

    private void ScreenLeft()
    {
        moveTimer.Stop();
    }
}
