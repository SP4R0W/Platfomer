using Godot;
using System;

public class HealthPotion : Area2D
{

    int ySpeed = 10;

    Timer moveTimer;
    VisibilityNotifier2D vis;

    CollisionShape2D collisionShape;

    Tween posTween;
    Tween visTween;

    Vector2 velocity = Vector2.Zero;

    public override void _Ready()
    {
        moveTimer = GetNode<Timer>("MoveTimer");
        vis = GetNode<VisibilityNotifier2D>("VisibilityNotifier2D");

        posTween = GetNode<Tween>("PosTween");
        visTween = GetNode<Tween>("VisTween");

        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

        if (vis.IsOnScreen())
            moveTimer.Start(); // Move timer is responsible for changing direction of y movement
        else
            moveTimer.Stop();
    }

    void ChangeY()
    {
        ySpeed *= -1;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (vis.IsOnScreen() && Global.isAnimOn)
        {
            velocity.y = ySpeed;

		    GlobalPosition += velocity * delta;
        }
    }

    void Touched(Node body)
    {
        if (body.Name == "Player")
        {
            collisionShape.SetDeferred("disabled",true);

            var sound = GetNode<AudioStreamPlayer>("HealthSound");
            if (Global.isSfxOn)
                sound.Play(); // Play the sound

            Global.plrHealth++; // Increase the player's health

            posTween.InterpolateProperty(this,"position:y",Position.y,Position.y-100,.5f);
            visTween.InterpolateProperty(this,"modulate:a",1.0,0,.5f); // Move up and hide animation

            posTween.Start();
            visTween.Start();

            GetTree().CreateTimer(.75f).Connect("timeout",this,"Kill"); // Destroy the object
        }
    }

    void Kill()
    {
        QueueFree();
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
