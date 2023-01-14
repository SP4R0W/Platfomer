using Godot;
using System;

public class LevelEnd : Area2D
{
    [Signal]
    public delegate void EndGameBG();
    
    private CollisionShape2D collisionShape;

    private Tween posTween;
    private Tween visTween;

    public override void _Ready()
    {
        posTween = GetNode<Tween>("PosTween");
        visTween = GetNode<Tween>("VisTween");

        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
    }

    private void Touched(Node body)
    {
        if (body.Name == "Player")
        {
            collisionShape.SetDeferred("disabled",true);
            Global.isEndGame = true;

            var sound = GetNode<AudioStreamPlayer>("WinSound");
            if (Global.isSfxOn)
            {
                sound.Play();
            }

            posTween.InterpolateProperty(this,"position:y",Position.y,Position.y-200,.5f);
            visTween.InterpolateProperty(this,"modulate:a",1.0,0,.5f);

            posTween.Start();
            visTween.Start();

            EmitSignal("EndGameBG");
            if (Global.isSfxOn)
            {
                GetTree().CreateTimer(3f).Connect("timeout",this,"Kill");
            }
        }
    }

    private void Kill()
    {
        QueueFree();
    }
}
