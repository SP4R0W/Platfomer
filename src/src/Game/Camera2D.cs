using Godot;
using System;

public class Camera2D : Godot.Camera2D
{
    float _lookAheadFactor = 0.05f;

    Tween tween;

    int facing = 0;
    Vector2 prevCameraPos;

    public override void _Ready()
    {
        prevCameraPos = GetCameraPosition();
        tween = GetNode<Tween>("Tween");
    }

    public override void _Process(float delta)
    {
        CheckFacing();
        prevCameraPos = GetCameraPosition();
    }

    void CheckFacing()
    {
        // This script is responsible for camera movement when changing direction of player's movement.

        // Check if the direction has changed
        var newFacing = Mathf.Sign(GetCameraPosition().x - prevCameraPos.x);

        if (newFacing != 0 && facing != newFacing)
        {
            facing = newFacing;
            var targetOffset = GetViewportRect().Size.x * _lookAheadFactor * facing; // Calculate the offset

            // Move camera's position a bit
            tween.InterpolateProperty(this,"position:x",Position.x,targetOffset,1,Tween.TransitionType.Linear,Tween.EaseType.InOut);
            tween.Start();
        }
    }

    void OnPlayerGrounded(bool isGrounded)
    {
        DragMarginVEnabled = !isGrounded;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//
//  }
}
