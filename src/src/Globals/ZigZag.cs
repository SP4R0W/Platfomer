using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

public class ZigZag : Node2D
{
    // This is a C# rewrite of the ZigZag animation made by Nemesis-AS in his repo godot-transitions

    public int stripes = 3;
    public Color color = new Color(0,0,0);

    int width = Global.composer.screen["width"];
    List<Vector2> points = new List<Vector2>();

    public override void _Ready()
    {
        List<Vector2> tempArray = new List<Vector2>();

        var x = width;
        var y = 0;

        points.Add(new Vector2(x,y));
        tempArray.Add(new Vector2(x - (width + 64), y));
        while (y <= Global.composer.screen["height"] * 2)
        {
            y += 32;
            if (x <= width)
                x += 32;
            else
                x -= 32;

            points.Add(new Vector2(x,y));
            tempArray.Add(new Vector2(x - (width + 64), y));
        }

        tempArray.Reverse();
        points.AddRange(tempArray);
    }

    public override void _Draw()
    {
        Color[] colors = {color};
        DrawPolygon(points.ToArray(),colors);
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//
//  }
}
