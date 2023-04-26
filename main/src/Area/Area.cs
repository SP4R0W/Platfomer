using Godot;
using System;

public class Area : Node
{
    public override void _Ready()
    {
        // This is the root node
        Global.composer.GotoScene(Global.scenes["mainmenu"],true,"fade",true);
    }

    void ImmunityEnd()
	{
        // Stop the immunity
		Global.isImmune = false;
	}
}
