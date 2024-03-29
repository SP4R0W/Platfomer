using Godot;
using System;

public class Immunity : Area2D
{

    Timer moveTimer;
	CollisionShape2D collisionShape;

    int ySpeed = 50;

    VisibilityNotifier2D vis;

    Vector2 velocity = Vector2.Zero;

    Tween posTween;
    Tween visTween;

    public override void _Ready()
    {
        posTween = GetNode<Tween>("PosTween");
        visTween = GetNode<Tween>("VisTween");

        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

        moveTimer = GetNode<Timer>("MoveTimer");
        vis = GetNode<VisibilityNotifier2D>("VisibilityNotifier2D");


        if (vis.IsOnScreen())
            moveTimer.Start(); // Move timer is responsible for changing direction of y movement
        else
            moveTimer.Stop();
    }

    void Touched(Node body)
    {
        if (body.Name == "Player")
        {
            var immunityTimer = GetTree().Root.GetNode<Timer>("Area/ImmunityDuration");
            immunityTimer.Start(); // Start the immunity

            moveTimer.Stop();

            collisionShape.SetDeferred("disabled",true);

            var sound = GetNode<AudioStreamPlayer>("ImmuneSound");
            if (Global.isSfxOn)
                sound.Play(); // Play the sound

            Global.isImmune = true;

            posTween.InterpolateProperty(this,"position:y",Position.y,Position.y-100,1);
            visTween.InterpolateProperty(this,"modulate:a",1.0,0,1); // Move up and hide

            posTween.Start();
            visTween.Start();

            GetTree().CreateTimer(1.25f).Connect("timeout",this,"Kill"); // Remove the object
        }
    }

    void Kill()
    {
        QueueFree();
    }

    void ChangeY()
    {
        ySpeed *= -1;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta)
    {
        if (vis.IsOnScreen() && Global.isAnimOn)
        {
            velocity.y = ySpeed;

		    GlobalPosition += velocity * delta;
        }
    }

    void ScreenEntered()
    {
        SetProcess(true); // Enable when entering the screen
        moveTimer.Start();
    }

    void ScreenLeft()
    {
        SetProcess(false); // Disable when leaving the screen
        moveTimer.Stop();
    }
}
