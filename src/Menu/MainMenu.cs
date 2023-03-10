using Godot;
using System;

public class MainMenu : Node
{

    private Tween tween;

    private bool isAnimationFinished = false;

    private bool isPanelVisible = false;

    public override async void _Ready()
    {
        tween = GetNode<Tween>("Tween");

        var button4 = GetNode<TextureButton>("CanvasLayer/QuitButton");

        if (OS.GetName() == "HTML5")
        {
            button4.Visible = false;
        }

        var title = GetNode<Label>("CanvasLayer/Label");
        title.RectGlobalPosition = new Vector2(title.RectGlobalPosition.x,-500);
        tween.InterpolateProperty(title,"rect_global_position:y",title.RectGlobalPosition.y,110,1,Tween.TransitionType.Bounce,Tween.EaseType.InOut);

        var devTitle = GetNode<Label>("CanvasLayer/Label2");
        devTitle.RectGlobalPosition = new Vector2(devTitle.RectGlobalPosition.x,-375);
        tween.InterpolateProperty(devTitle,"rect_global_position:y",devTitle.RectGlobalPosition.y,300,1,Tween.TransitionType.Elastic,Tween.EaseType.InOut,1);

        var button1 = GetNode<TextureButton>("CanvasLayer/PlayButton");
        button1.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(button1,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,2);

        var button2 = GetNode<TextureButton>("CanvasLayer/OptionsButton");
        button2.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(button2,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,2.5f);

        var button3 = GetNode<TextureButton>("CanvasLayer/HelpButton");
        button3.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(button3,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,3);
        
        button4.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(button4,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,3.5f);

        tween.Start();

        await ToSignal(tween,"tween_all_completed");

        isAnimationFinished = true;

        var music = GetTree().Root.GetNode<AudioStreamPlayer>("Area/MenuTheme");
        if (Global.isMusicOn && !music.Playing)
        {
            music.Play();
        }
    }

    public override async void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && !isAnimationFinished)
        {
            if (mouseButton.ButtonIndex == 1)
            {
                tween.RemoveAll();

                var title = GetNode<Label>("CanvasLayer/Label");
                title.RectGlobalPosition = new Vector2(title.RectGlobalPosition.x,110);

                var devTitle = GetNode<Label>("CanvasLayer/Label2");
                devTitle.RectGlobalPosition = new Vector2(devTitle.RectGlobalPosition.x,300);

                var button1 = GetNode<TextureButton>("CanvasLayer/PlayButton");
                button1.Modulate = new Color(1,1,1,1);

                var button2 = GetNode<TextureButton>("CanvasLayer/OptionsButton");
                button2.Modulate = new Color(1,1,1,1);

                var button3 = GetNode<TextureButton>("CanvasLayer/HelpButton");
                button3.Modulate = new Color(1,1,1,1);

                var button4 = GetNode<TextureButton>("CanvasLayer/QuitButton");
                button4.Modulate = new Color(1,1,1,1);

                await ToSignal(GetTree().CreateTimer(0.2f),"timeout");
                
                isAnimationFinished = true;

                var music = GetTree().Root.GetNode<AudioStreamPlayer>("Area/MenuTheme");
                if (Global.isMusicOn && !music.Playing)
                {
                    music.Play();
                }
            }
        }
    }

    private void ShowPanel()
    {
        if (!isAnimationFinished && !isPanelVisible)
        {
            return;
        }

        if (Global.isSfxOn)
        {
            Global.shortClick.Play();
        }

        isPanelVisible = true;
        
        Panel panel = GetNode<Panel>("CanvasLayer/Panel");
        panel.Visible = true;
    }
    private void GotoNewGame()
    {
        if (!isAnimationFinished && isPanelVisible)
        {
            return;
        }

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

        Global.isNewGame = true;
        Global.level = 1;
        Global.composer.GotoScene(Global.scenes["game"],true,"fade");     
    }

    private void GotoLevelSelect()
    {
        if (!isAnimationFinished && isPanelVisible)
        {
            return;
        }
        
        Global.composer.GotoScene(Global.scenes["levelselect"]);
    }

    private void ClosePanel()
    {
        if (!isAnimationFinished && isPanelVisible)
        {
            return;
        }

        if (Global.isSfxOn)
        {
            Global.shortClick.Play();
        }
        
        Panel panel = GetNode<Panel>("CanvasLayer/Panel");
        panel.Visible = false;

        isPanelVisible = false;
    }

    private void GotoOptions()
    {
        if (!isAnimationFinished && !isPanelVisible)
        {
            return;
        }

        Global.composer.GotoScene(Global.scenes["options"]);
    }

    private void GotoHelp()
    {
        if (!isAnimationFinished && !isPanelVisible)
        {
            return;
        }

        Global.composer.GotoScene(Global.scenes["help"]);
    }

    private void QuitGame()
    {
        if (!isAnimationFinished && !isPanelVisible)
        {
            return;
        }

        GetTree().Quit();
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
