using Godot;
using System;

public class LevelSelect : Node
{

    int level = 1;

    Label levelLabel;
    AnimatedSprite levelPreview;

    Tween tween;

    bool isAnimationFinished = false;
    bool isChangingLevel = false;

    public override async void _Ready()
    {
        tween = GetNode<Tween>("Tween");

        levelLabel = GetNode<Label>("CanvasLayer/LevelText");
        levelPreview = GetNode<AnimatedSprite>("CanvasLayer/LevelPreview");

        // Level select screen animations

        var title = GetNode<Label>("CanvasLayer/Label");
        title.RectGlobalPosition = new Vector2(title.RectGlobalPosition.x,-500);
        tween.InterpolateProperty(title,"rect_global_position:y",title.RectGlobalPosition.y,110,1,Tween.TransitionType.Bounce,Tween.EaseType.InOut);

        levelLabel.RectGlobalPosition = new Vector2(levelLabel.RectGlobalPosition.x,-350);
        tween.InterpolateProperty(levelLabel,"rect_global_position:y",levelLabel.RectGlobalPosition.y,300,1,Tween.TransitionType.Elastic,Tween.EaseType.InOut,1);

        levelPreview.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(levelPreview,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,1.5f);

        var button1 = GetNode<TextureButton>("CanvasLayer/PlayButton");
        button1.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(button1,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,2f);

        var button2 = GetNode<TextureButton>("CanvasLayer/BackButton");
        button2.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(button2,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,2.5f);

        var button3 = GetNode<TextureButton>("CanvasLayer/NextButton");
        button3.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(button3,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,3f);

        var button4 = GetNode<TextureButton>("CanvasLayer/PrevButton");
        button4.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(button4,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,3f);

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

                var title = GetNode<Label>("CanvasLayer/Label");
                title.RectGlobalPosition = new Vector2(title.RectGlobalPosition.x,110);

                levelLabel.RectGlobalPosition = new Vector2(levelLabel.RectGlobalPosition.x,300);

                levelPreview.Modulate = new Color(1,1,1,1);

                var button1 = GetNode<TextureButton>("CanvasLayer/PlayButton");
                button1.Modulate = new Color(1,1,1,1);

                var button2 = GetNode<TextureButton>("CanvasLayer/BackButton");
                button2.Modulate = new Color(1,1,1,1);

                var button3 = GetNode<TextureButton>("CanvasLayer/NextButton");
                button3.Modulate = new Color(1,1,1,1);

                var button4 = GetNode<TextureButton>("CanvasLayer/PrevButton");
                button4.Modulate = new Color(1,1,1,1);

                await ToSignal(GetTree().CreateTimer(0.2f),"timeout");

                isAnimationFinished = true;
            }
        }
    }

    void GotoGame()
    {
        if (!isAnimationFinished)
            return;

        // Reset all values on start of the game

		Global.totalSpeedTime = 0;
		Global.level1SpeedTime = 0;
		Global.level2SpeedTime = 0;
		Global.level3SpeedTime = 0;
		Global.level4SpeedTime = 0;
		Global.level5SpeedTime = 0;

		Global.prevTotalKills = 0;
		Global.prevTotalScore = 0;
		Global.prevTotalTime = 0;
		Global.totalDeaths = 0;
		Global.totalKills = 0;
		Global.totalScore = 0;
		Global.totalTime = 0;

        Global.plrHealth = 3;

        Global.isNewGame = false;
        Global.level = level;
        Global.composer.GotoScene(Global.scenes["game"],true,"fade");
    }

    void GotoMenu()
    {
        if (!isAnimationFinished)
            return;

        Global.composer.GotoScene(Global.scenes["mainmenu"]);
    }

    async void NextLevel()
    {
        if (!isAnimationFinished || isChangingLevel)
            return;

        isChangingLevel = true;

        level++;
        if (level > 5)
            level = 1;

        if (Global.isSfxOn)
            Global.shortClick.Play();

        // Animations for changing the level thumbnail image and text (these can't be skipped)

        tween.InterpolateProperty(levelLabel,"modulate:a",1,0,.5f);
        tween.InterpolateProperty(levelPreview,"modulate:a",1,0,.5f);

        tween.Start();

        await ToSignal(tween,"tween_all_completed");

        levelLabel.Text = "Level: " + level;
        levelPreview.Animation = Convert.ToString(level);

        tween.InterpolateProperty(levelLabel,"modulate:a",0,1,.5f);
        tween.InterpolateProperty(levelPreview,"modulate:a",0,1,.5f);

        tween.Start();

        await ToSignal(tween,"tween_all_completed");

        isChangingLevel = false;
    }

    async void PrevLevel()
    {
        if (!isAnimationFinished || isChangingLevel)
            return;

        isChangingLevel = true;

        level--;
        if (level < 1)
            level = 5;

        if (Global.isSfxOn)
            Global.shortClick.Play();

        // Animations for changing the level thumbnail image and text (these can't be skipped)

        tween.InterpolateProperty(levelLabel,"modulate:a",1,0,.5f);
        tween.InterpolateProperty(levelPreview,"modulate:a",1,0,.5f);

        tween.Start();

        await ToSignal(tween,"tween_all_completed");

        levelLabel.Text = "Level: " + level;
        levelPreview.Animation = Convert.ToString(level);

        tween.InterpolateProperty(levelLabel,"modulate:a",0,1,.5f);
        tween.InterpolateProperty(levelPreview,"modulate:a",0,1,.5f);

        tween.Start();

        await ToSignal(tween,"tween_all_completed");

        isChangingLevel = false;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//
//  }
}
