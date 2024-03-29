using Godot;
using System;

public class Level3 : Node
{

    private int cameraLeftLimit = 0;
    private int cameraRightLimit = 1920;
    private int cameraUpLimit = 0;
    private int cameraDownLimit = 1080;

    public override void _Ready()
    {
        var tilemap = GetNode<TileMap>("TileMap");
        var rect = tilemap.GetUsedRect();
        // Right limit needs a different value (calculated from the size of the map)
        cameraRightLimit = Convert.ToInt32(rect.Size.x * 64) - 128;

        var player = GetNode<Player>("Player");
        var camera = player.GetNode<Camera2D>("Camera2D");
        // Set all the camera borders
        camera.LimitLeft = cameraLeftLimit;
        camera.LimitRight = cameraRightLimit;
        camera.LimitTop = cameraUpLimit;
        camera.LimitBottom = cameraDownLimit;

        // Show the background and set the correct texture
        var bg = GetNode<Sprite>("Player/Camera2D/Background/ParallaxLayer/Sprite");
        var texture = (Texture)GD.Load("res://assets/Levels/Level3/blue_shroom.png");
        bg.Texture = texture;
        bg.Visible = true;
        bg.RegionRect = new Rect2(0,-135,cameraRightLimit,1300);

        var enemies = GetNode<Node2D>("Enemies");

        for (int i = 0; i < enemies.GetChildCount();i++)
        {
            var enemiesGroup = (Node2D)enemies.GetChild(i);
            string enemyHitMethod = GetHitMethod(enemiesGroup); // Get the correct method name

            for (int j = 0; j < enemiesGroup.GetChildCount();j++)
            {
                var enemy = enemiesGroup.GetChild(j);
                enemy.Connect("Hit",player,enemyHitMethod); // Connect all the enemies hit signals to Player
            }
        }

    }

    private string GetHitMethod(Node2D enemyGroup)
    {
        switch (enemyGroup.Name)
        {
            case "Barnacles":
                return "BarnacleHit";
            case "Bees":
                return "BeeHit";
            case "Bats":
                return "BatHit";
            case "Ghosts":
                return "GhostHit";
            case "Slimes":
                return "SlimeHit";
            case "Spiders":
                return "SpiderHit";
            case "Spinners":
                return "SpinnerHit";
            case "Piranhas":
                return "PiranhaHit";
        }

        return String.Empty;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//
//  }
}
