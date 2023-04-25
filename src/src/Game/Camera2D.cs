using Godot;
using System;

public class Camera2D : Godot.Camera2D
{
    private float _lookAheadFactor = 0.05f;
    
    private Tween tween;

    private int facing = 0;
    private Vector2 prevCameraPos;

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

    private void CheckFacing()
    {
        var newFacing = Mathf.Sign(GetCameraPosition().x - prevCameraPos.x);
        if (newFacing != 0 && facing != newFacing)
        {
            facing = newFacing;
            var targetOffset = GetViewportRect().Size.x * _lookAheadFactor * facing;

            tween.InterpolateProperty(this,"position:x",Position.x,targetOffset,1,Tween.TransitionType.Linear,Tween.EaseType.InOut);
            tween.Start();
        }
    }

    private void OnPlayerGrounded(bool isGrounded)
    {
        DragMarginVEnabled = !isGrounded;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
