using Godot;
using System;

public class Area : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Global.composer.GotoScene(Global.scenes["mainmenu"],true,"fade",true);
    }

    private void ImmunityEnd()
	{
		Global.isImmune = false;
	}
}
