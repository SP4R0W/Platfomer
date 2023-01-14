using Godot;
using System;
using Godot.Collections;

public class Options : Node
{

    private Label musicLabel;
    private Label sfxLabel;
    private Label ostLabel;
    private Label animLabel;
    private Label speedLabel;
    private TextureButton musicTextureButton;
    private TextureButton sfxTextureButton;
    private TextureButton ostTextureButton;
    private TextureButton animTextureButton;
    private TextureButton speedTextureButton;

    private Tween tween;

    private bool isAnimationFinished = false;

    private Dictionary<bool,string> textValues = new Dictionary<bool,string>()
    {
        {true,"ON"},
        {false,"OFF"}
    };

    public override async void _Ready()
    {
        tween = GetNode<Tween>("Tween");

        musicLabel = GetNode<Label>("CanvasLayer/VBoxContainer/Music/MusicLabel");
        sfxLabel = GetNode<Label>("CanvasLayer/VBoxContainer/SFX/SFXLabel");
        ostLabel = GetNode<Label>("CanvasLayer/VBoxContainer/Calm/CalmLabel");
        animLabel = GetNode<Label>("CanvasLayer/VBoxContainer/Anims/AnimLabel");
        speedLabel = GetNode<Label>("CanvasLayer/VBoxContainer/Speed/SpeedLabel");

        musicTextureButton = GetNode<TextureButton>("CanvasLayer/VBoxContainer/Music/MusicButton");
        sfxTextureButton  = GetNode<TextureButton>("CanvasLayer/VBoxContainer/SFX/SFXButton");
        ostTextureButton = GetNode<TextureButton>("CanvasLayer/VBoxContainer/Calm/CalmButton");
        animTextureButton = GetNode<TextureButton>("CanvasLayer/VBoxContainer/Anims/AnimButton");
        speedTextureButton = GetNode<TextureButton>("CanvasLayer/VBoxContainer/Speed/SpeedButton");

        var title = GetNode<Label>("CanvasLayer/Label");
        title.RectGlobalPosition = new Vector2(title.RectGlobalPosition.x,-500);
        tween.InterpolateProperty(title,"rect_global_position:y",title.RectGlobalPosition.y,110,1,Tween.TransitionType.Bounce,Tween.EaseType.InOut);

        musicLabel.Text = "MUSIC: " + textValues[Global.isMusicOn];

        musicLabel.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(musicLabel,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,1);

        sfxLabel.Text = "SFX: " + textValues[Global.isSfxOn];

        sfxLabel.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(sfxLabel,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,1f);

        ostLabel.Text = "Alt OST: " + textValues[!Global.isMusicCalm];

        ostLabel.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(ostLabel,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,1f);

        animLabel.Text = "ANIMATIONS: " + textValues[Global.isAnimOn];

        animLabel.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(animLabel,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,1f);

        speedLabel.Text = "SPEEDRUN: " + textValues[Global.isSpeedrunOn];

        speedLabel.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(speedLabel,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,1f);

        musicTextureButton.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(musicTextureButton,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,2f);

        sfxTextureButton.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(sfxTextureButton,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,2f);

        ostTextureButton.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(ostTextureButton,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,2f);

        speedTextureButton.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(speedTextureButton,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,2f);

        animTextureButton.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(animTextureButton,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,2f);

        tween.Start();

        await ToSignal(tween,"tween_all_completed");

        isAnimationFinished = true;
    }

    public async override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && !isAnimationFinished)
        {
            if (mouseButton.ButtonIndex == 1)
            {
                tween.RemoveAll();

                var title = GetNode<Label>("CanvasLayer/Label");
                title.RectGlobalPosition = new Vector2(title.RectGlobalPosition.x,110);

                musicLabel.Modulate = new Color(1,1,1,1);
                sfxLabel.Modulate = new Color(1,1,1,1);
                ostLabel.Modulate = new Color(1,1,1,1);
                animLabel.Modulate = new Color(1,1,1,1);
                speedLabel.Modulate = new Color(1,1,1,1);

                musicTextureButton.Modulate = new Color(1,1,1,1);

                sfxTextureButton.Modulate = new Color(1,1,1,1);

                ostTextureButton.Modulate = new Color(1,1,1,1);

                animTextureButton.Modulate = new Color(1,1,1,1);

                speedTextureButton.Modulate = new Color(1,1,1,1);

                await ToSignal(GetTree().CreateTimer(0.2f),"timeout");
                
                isAnimationFinished = true;
            }
        }
    }

    private void ChangeMusic()
    {
        if (!isAnimationFinished)
        {
            return;
        }

        if (Global.isSfxOn && !Global.shortClick.Playing)
        {
            Global.shortClick.Play();
        }

        Global.isMusicOn = !Global.isMusicOn;

        var music = GetTree().Root.GetNode<AudioStreamPlayer>("Area/MenuTheme");
        if (Global.isMusicOn)
        {
            music.Play();
        }
        else
        {
            music.Stop();
        }

        musicLabel.Text = "MUSIC: " + textValues[Global.isMusicOn];
    }

    private void ChangeSfx()
    {
        if (!isAnimationFinished)
        {
            return;
        }

        if (Global.isSfxOn)
        {
            Global.shortClick.Play();
        }

        Global.isSfxOn = !Global.isSfxOn;

        sfxLabel.Text = "SFX: " + textValues[Global.isSfxOn];
    }

    private void ChangeOST()
    {
        if (!isAnimationFinished)
        {
            return;
        }

        if (Global.isSfxOn)
        {
            Global.shortClick.Play();
        }

        Global.isMusicCalm = !Global.isMusicCalm;

        ostLabel.Text = "Alt OST: " + textValues[!Global.isMusicCalm];
    }

    private void ChangeAnim()
    {
        if (!isAnimationFinished)
        {
            return;
        }

        if (Global.isSfxOn)
        {
            Global.shortClick.Play();
        }

        Global.isAnimOn = !Global.isAnimOn;

        animLabel.Text = "ANIMATIONS: " + textValues[Global.isAnimOn];
    }

    private void SwitchSpeedrun()
    {
        if (!isAnimationFinished)
        {
            return;
        }

        if (Global.isSfxOn)
        {
            Global.shortClick.Play();
        }

        Global.isSpeedrunOn = !Global.isSpeedrunOn;

        speedLabel.Text = "Speedrun: " + textValues[Global.isSpeedrunOn];
    }

    private void GotoMenu()
    {
        if (!isAnimationFinished)
        {
            return;
        }

        Global.composer.GotoScene(Global.scenes["mainmenu"]);
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
