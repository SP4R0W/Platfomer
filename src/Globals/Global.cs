using Godot;
using System;

public class Global : Node
{

    public static int level = 1;

    public static Godot.Collections.Dictionary<string,string> scenes = new Godot.Collections.Dictionary<string,string>()
    {
        {"mainmenu","res://src/Menu/MainMenu.tscn"},
        {"options","res://src/Menu/Options.tscn"},
        {"help","res://src/Menu/Help.tscn"},
        {"levelselect","res://src/Menu/LevelSelect.tscn"},
        {"game","res://src/Game/Game.tscn"},
        {"level1","res://src/Levels/Level1.tscn"},
        {"level2","res://src/Levels/Level2.tscn"},
        {"level3","res://src/Levels/Level3.tscn"},
        {"level4","res://src/Levels/Level4.tscn"},
        {"level5","res://src/Levels/Level5.tscn"},
        {"endgame","res://src/Menu/End.tscn"}
    };

    public static Vector2 playerPos;

    public static Composer composer;

    public static int coins = 0;
    public static int time = 120;
    public static int plrHealth = 3;
    public static bool isImmune = false;
    public static bool isNewGame = false;
    public static bool isChangingLevel = false;
    public static bool isReset =false;

    public static int prevTotalTime = 0;
    public static int prevTotalScore = 0;
    public static int prevTotalKills = 0;
    
    public static int totalTime = 0;
    public static int totalScore = 0;
    public static int totalDeaths = 0;
    public static int totalKills = 0;

    public static bool isMusicOn = true;
    public static bool isMusicCalm = true;
    public static bool isSfxOn = true;
    public static bool isAnimOn = true;
    public static bool isSpeedrunOn = false;

    public static bool isEndGame = false;
    public static bool isHurt = false;
    public static bool isDead = false;

    public static double startTime = 0;
    public static double level1SpeedTime = 0.000;
    public static double level2SpeedTime = 0.000;
    public static double level3SpeedTime = 0.000;
    public static double level4SpeedTime = 0.000;
    public static double level5SpeedTime = 0.000;
    public static double totalSpeedTime = 0.000;

    public static AudioStreamPlayer longClick;
    public static AudioStreamPlayer shortClick;

    public override void _Ready()
    {
        level = 1;
        composer = (Composer)GetNode("/root/Composer");

        shortClick = GetTree().Root.GetNode<AudioStreamPlayer>("Area/ShortClick");
        longClick = GetTree().Root.GetNode<AudioStreamPlayer>("Area/LongClick");
    }

    public static void PlayMusic(Node area, int id)
    {
        var musicName = "LevelTheme" + Convert.ToString(id);
        if (isMusicCalm)
            musicName += "Calm";

        var track = area.GetNode<AudioStreamPlayer>(musicName);
        if (!track.Playing)
            track.Play();
    }

    public static void StopMusic(Node area, int id)
    {
        var musicName = "LevelTheme" + Convert.ToString(id);
        if (isMusicCalm)
            musicName += "Calm";

        var track = area.GetNode<AudioStreamPlayer>(musicName);
        track.Stop();
    }
}
