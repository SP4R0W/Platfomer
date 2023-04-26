using Godot;
using System;
using Godot.Collections;

public class Composer : Node
{

    public Node CurrentScene { get; set; }

    bool isEnteringNewScene = false;

    public Dictionary<string,int> screen = new Godot.Collections.Dictionary<string,int>();

    PackedScene zigzag = (PackedScene)ResourceLoader.Load("res://src/Globals/ZigZag.tscn");

    public override void _Ready()
    {
        screen["width"] = Convert.ToInt32(OS.GetRealWindowSize().x);
        screen["height"] = Convert.ToInt32(OS.GetRealWindowSize().y);
    }

    public void GotoScene(string newScene,bool animate = true, string animation = "zigzag",bool beQuiet = false)
    {
        if (!isEnteringNewScene)
        {
            if (Global.isSfxOn && !beQuiet)
                Global.longClick.Play(); // Play the sound

            isEnteringNewScene = true;
            CallDeferred(nameof(DefferedGotoScene),newScene,animate,animation);
        }
    }

    async void DefferedGotoScene(string newScene,bool animate = true,string animation="zigzag")
    {
        var root = GetTree().Root.GetNode<Node>("Area");

        if (CurrentScene != null && animate)
        {
            if (animation == "zigzag")
            {
                var canvas = root.GetNode<CanvasLayer>("Control/CanvasLayer");
                var tween = new Tween();

                Node2D overlay = (Node2D)zigzag.Instance();

                canvas.AddChild(overlay);
                overlay.GlobalPosition = new Vector2(0,0);

                canvas.AddChild(tween); // Start zigzag animation
                tween.InterpolateProperty(overlay,"global_position",new Vector2(-(1 * screen["width"]),0),new Vector2(0,0),.75f,Tween.TransitionType.Linear,Tween.EaseType.InOut);
                tween.Start();

                await ToSignal(tween,"tween_completed");

                var scene = root.GetNode<Node>(CurrentScene.Name);
                scene.QueueFree(); // Remove the old scene

                var nextScene = (PackedScene)ResourceLoader.Load(newScene);
                Node nextLevel = (Node)nextScene.Instance();
                root.AddChild(nextLevel); // Load the new scene and add it to root

                // Start the second zigzag animation
                tween.InterpolateProperty(overlay,"global_position",new Vector2(0,0),new Vector2(1 * (screen["width"] * 2),0),1.5f,Tween.TransitionType.Linear,Tween.EaseType.InOut);
                tween.Start();

                await ToSignal(tween,"tween_completed");

                isEnteringNewScene = false;

                overlay.QueueFree();
                tween.QueueFree();

                CurrentScene = root.GetChild(root.GetChildCount() - 1); // set current scene

                Global.isChangingLevel = false;
            }
            else if (animation == "fade")
            {
                var canvas = root.GetNode<CanvasLayer>("Control/CanvasLayer");
                var transitionRect = root.GetNode<ColorRect>("Control/CanvasLayer/TransitionRect");
                var tween = new Tween();

                transitionRect.Visible = true;
                canvas.AddChild(tween);

                tween.InterpolateProperty(transitionRect,"modulate:a",0, 1,.5f,Tween.TransitionType.Sine,Tween.EaseType.InOut); // Start fading in
                tween.Start();

                await ToSignal(tween,"tween_completed");

                var scene = root.GetNode<Node>(CurrentScene.Name);
                scene.QueueFree(); // Remove the old scene

                var nextScene = (PackedScene)ResourceLoader.Load(newScene);
                Node nextLevel = (Node)nextScene.Instance();
                root.AddChild(nextLevel); // Load the new scene and add it to root

                tween.InterpolateProperty(transitionRect,"modulate:a",1, 0,.5f,Tween.TransitionType.Sine,Tween.EaseType.InOut); // Start fading out
                tween.Start();

                await ToSignal(tween,"tween_completed");

                isEnteringNewScene = false;

                tween.QueueFree();
                transitionRect.Visible = false;

                CurrentScene = root.GetChild(root.GetChildCount() - 1); // set current scene

                Global.isChangingLevel = false;
            }
        }
        else
        {
            // No animation, simply remove the old scene and load in the new one.

            var nextLevel = (PackedScene)ResourceLoader.Load(newScene);
            root.AddChild(nextLevel.Instance());

            CurrentScene = root.GetChild(root.GetChildCount() - 1);

            isEnteringNewScene = false;

            Global.isChangingLevel = false; // set current scene
        }
    }
}
