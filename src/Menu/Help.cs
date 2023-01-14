using Godot;
using System;
using Godot.Collections;

public class Help : Node
{
    private Tween tween;

    private bool isAnimationFinished = false;
    private bool isChangingPage = false;

    private Label titleLabel;
    private Label helpLabel;

    private int page = 0;

    private Array<string> helpTitle = new Array<string>()
    {
        "In-Game Controls:",
        "In-Game Credits :"
    };

    private Array<string> helpText = new Array<string>()
    {
        "use wasd or arrow keys to walk\npress space to jump\npress q in game to go back to menu\npress r to quickly restart the level\npress enter to proceed to the next level",
        "coded by Sp4r0w\nart & Blocks font made by kenney\nWatermelon Days font made by Khurasan\nButton sprites made by Viktor Gogela\nMusic by joshuuu (alt OST: Clustertruck OST)"
    };

    public override async void _Ready()
    {
        tween = GetNode<Tween>("Tween");
        titleLabel = GetNode<Label>("CanvasLayer/Label");
        helpLabel = GetNode<Label>("CanvasLayer/Label2");

        titleLabel.RectGlobalPosition = new Vector2(titleLabel.RectGlobalPosition.x,-500);
        tween.InterpolateProperty(titleLabel,"rect_global_position:y",titleLabel.RectGlobalPosition.y,110,1,Tween.TransitionType.Bounce,Tween.EaseType.InOut);
        
        helpLabel.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(helpLabel,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,1);

        var button1 = GetNode<TextureButton>("CanvasLayer/PageButton");
        button1.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(button1,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,2);

        var button2 = GetNode<TextureButton>("CanvasLayer/BackButton");
        button2.Modulate = new Color(1,1,1,0);
        tween.InterpolateProperty(button2,"modulate:a",0,1,1,Tween.TransitionType.Linear,Tween.EaseType.InOut,2.5f);

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

                titleLabel.RectGlobalPosition = new Vector2(titleLabel.RectGlobalPosition.x,110);

                helpLabel.Modulate = new Color(1,1,1,1);

                var button1 = GetNode<TextureButton>("CanvasLayer/PageButton");
                button1.Modulate = new Color(1,1,1,1);

                var button2 = GetNode<TextureButton>("CanvasLayer/BackButton");
                button2.Modulate = new Color(1,1,1,1);
                
                await ToSignal(GetTree().CreateTimer(0.2f),"timeout");
                
                isAnimationFinished = true;
            }
        }
    }

    private async void SwitchPage()
    {
        if (!isAnimationFinished && isChangingPage)
        {
            return;
        }

        isChangingPage = true;

        tween.InterpolateProperty(helpLabel,"rect_global_position:x",helpLabel.RectGlobalPosition.x,2000,1,Tween.TransitionType.Elastic,Tween.EaseType.InOut);
        tween.Start();

        await ToSignal(tween,"tween_completed");

        page++;
        if (page > 1)
        {
            page = 0;
        }

        titleLabel.Text = helpTitle[page];
        helpLabel.Text = helpText[page];

        helpLabel.RectGlobalPosition = new Vector2(-2000,helpLabel.RectGlobalPosition.y);

        tween.InterpolateProperty(helpLabel,"rect_global_position:x",helpLabel.RectGlobalPosition.x,245,1,Tween.TransitionType.Elastic,Tween.EaseType.InOut);
        tween.Start();

        await ToSignal(tween,"tween_completed");
        
        isChangingPage = false;
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
