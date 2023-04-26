using Godot;
using System;
using Godot.Collections;

public class End : Node
{
    Tween tween;

    bool isAnimationFinished = false;
    Label titleLabel;
    Label helpLabel;

    public override async void _Ready()
    {
        var music = GetNode<AudioStreamPlayer>("EndTheme");
        if (Global.isMusicOn && !music.Playing)
            music.Play();

        tween = GetNode<Tween>("Tween");
        titleLabel = GetNode<Label>("CanvasLayer/Label");
        helpLabel = GetNode<Label>("CanvasLayer/Label2");

        // End screen animations

        titleLabel.RectGlobalPosition = new Vector2(titleLabel.RectGlobalPosition.x,-500);
        tween.InterpolateProperty(titleLabel,"rect_global_position:y",titleLabel.RectGlobalPosition.y,50,1,Tween.TransitionType.Bounce,Tween.EaseType.InOut);

        helpLabel.Modulate = new Color(1,1,1,0);
        helpLabel.Text = "Your stats:\n\nTotal time: " + Global.totalTime +"\nTotal score: " + Global.totalScore + "\nTotal deaths: " + Global.totalDeaths + "\nEnemies killed: " + Global.totalKills;
        tween.InterpolateProperty(helpLabel,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,1);

        var button2 = GetNode<TextureButton>("CanvasLayer/BackButton");
        button2.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(button2,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,2f);

        tween.Start();

        await ToSignal(tween,"tween_all_completed");

        isAnimationFinished = true;

    }

    public async override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && !isAnimationFinished)
        {
            if (mouseButton.ButtonIndex == 1) // Skip the animations on mouse click
            {
                tween.RemoveAll();

                titleLabel.RectGlobalPosition = new Vector2(titleLabel.RectGlobalPosition.x,110);

                helpLabel.Modulate = new Color(1,1,1,1);

                var button2 = GetNode<TextureButton>("CanvasLayer/BackButton");
                button2.Modulate = new Color(1,1,1,1);

                await ToSignal(GetTree().CreateTimer(0.2f),"timeout");

                isAnimationFinished = true;
            }
        }
    }

    void GotoMenu()
    {
        if (!isAnimationFinished)
            return;

        Global.composer.GotoScene(Global.scenes["mainmenu"]);
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//
//  }
}
