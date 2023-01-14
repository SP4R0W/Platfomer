using Godot;
using System;

public class Coin : Area2D
{

    private int ySpeed = 10;
    
    private Timer moveTimer;
    private VisibilityNotifier2D vis;

    private CollisionShape2D collisionShape;

    private Tween posTween;
    private Tween visTween;

    private Vector2 velocity = Vector2.Zero;

    public override void _Ready()
    {
        moveTimer = GetNode<Timer>("MoveTimer");
        vis = GetNode<VisibilityNotifier2D>("VisibilityNotifier2D");

        posTween = GetNode<Tween>("PosTween");
        visTween = GetNode<Tween>("VisTween");

        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

        if (vis.IsOnScreen())
        {
            moveTimer.Start();
        }
        else
        {
            moveTimer.Stop();
        }
    }

    private void ChangeY()
    {
        ySpeed *= -1;
    }

    public override void _Process(float delta)
    {
        if (vis.IsOnScreen() && Global.isAnimOn)
        {
            velocity.y = ySpeed;

            GlobalPosition += velocity * delta;   
        }
    }

    private void Touched(Node body)
    {
        if (body.Name == "Player")
        {
            collisionShape.SetDeferred("disabled",true);
            var sound = GetNode<AudioStreamPlayer>("CoinSound");
            if (Global.isSfxOn)
            {
                sound.Play();
            }
            Global.coins++;
            if (Global.coins % 10 == 0)
            {
                Global.plrHealth++;
            }

            posTween.InterpolateProperty(this,"position:y",Position.y,Position.y-100,.5f);
            visTween.InterpolateProperty(this,"modulate:a",1.0,0,.5f);

            posTween.Start();
            visTween.Start();

            GetTree().CreateTimer(.75f).Connect("timeout",this,"Kill");
        }
    }

    private void Kill()
    {
        QueueFree();
    }

    private void ScreenEntered()
    {
        moveTimer.Start();
    }

    private void ScreenLeft()
    {
        moveTimer.Stop();
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
