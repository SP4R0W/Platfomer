using Godot;
using System;

public class Game : Node
{
	[Signal]
	public delegate void Timeout();

	[Signal]
	public delegate void EndGame();

	bool isCountDone = false;

	int totalScore = 0;

	public override void _Ready()
	{
    	var music = GetTree().Root.GetNode<AudioStreamPlayer>("Area/MenuTheme");
        if (music.Playing)
            music.Stop();

		if (Global.isSpeedrunOn)
			UpdateSpeedrun(); // Set up the speedrun UI

		Global.isReset = false;
		Global.isEndGame = false;

		// Reset all the values
		if (!Global.isNewGame)
		{
			Global.prevTotalKills = 0;
			Global.prevTotalScore = 0;
			Global.prevTotalTime = 0;

			Global.totalDeaths = 0;
			Global.totalKills = 0;
			Global.totalScore = 0;
			Global.totalTime = 0;
		}

		Global.coins = 0;
        Global.time = 100;
        Global.isImmune = false;

		LoadLevel(Convert.ToString(Global.level));

		var levelEnd = GetNode<LevelEnd>("Level" + Convert.ToString(Global.level) + "/LevelEnd");
		levelEnd.Connect("EndGameBG",this,"LevelEnd");

		var player = GetNode<Player>("Level" + Convert.ToString(Global.level) + "/Player");
		player.Connect("EnemyKilled",this,"UpdateCount");

		var timer = GetNode<Timer>("Timer");
		timer.Start();

        if (Global.isMusicOn)
			Global.PlayMusic(GetTree().Root.GetNode("Area"),Global.level);
	}

	void UpdateSpeedrun()
	{
		// Get the time at start in miliseconds
		Global.startTime = OS.GetSystemTimeMsecs();

		var ui = GetNode<CanvasLayer>("SpeedrunUI");
		ui.Visible = true;

		var vFlow = ui.GetNode("VFlowContainer");
		for (int i = 0;i<vFlow.GetChildCount();i++)
		{
			var child = (Label)vFlow.GetChild(i);
			child.Visible = false;
		}

		if (Global.isNewGame)
		{
			for (int i = 0;i<vFlow.GetChildCount();i++)
			{
				var child = (Label)vFlow.GetChild(i);
				child.Visible = true;
			}
		}
		else
		{
			var level = GetNode<Label>("SpeedrunUI/VFlowContainer/Level" + Global.level + "Text");
			var total = GetNode<Label>("SpeedrunUI/VFlowContainer/TotalText");

			level.Visible = true;
			total.Visible = true;
		}
	}

	void LoadLevel(string level)
	{
		string levelName = "res://src/Levels/Level" + level + ".tscn";
		PackedScene levelScenePath = (PackedScene)ResourceLoader.Load(levelName); // Load the scene with level
		var levelScene = levelScenePath.Instance();
		AddChild(levelScene);
	}

	void UpdateCount()
	{
		var sound = GetNode<AudioStreamPlayer>("KillSound");

        if (Global.isSfxOn)
            sound.Play();

		Global.totalScore += 100;
		Global.totalKills++;
	}

	void LevelEnd()
	{
		Global.StopMusic(GetTree().Root.GetNode("Area"),Global.level);

        var timer = GetNode<Timer>("Timer");
        timer.Stop();

		if (Global.isSfxOn)
			GetTree().CreateTimer(3).Connect("timeout",this,"ContinueLevelEnd");
		else
			ContinueLevelEnd(); // Skip the waiting for sound if we have them disabled
	}

	void ContinueLevelEnd()
	{
		// This is triggered when all the counters at the end of the level are finished
		isCountDone = false;

		var sound = GetNode<AudioStreamPlayer>("Counter");
		if (Global.isSfxOn)
			sound.Play();

		totalScore = 0;

		totalScore += 500 * Global.plrHealth;
        totalScore += 1000 * Global.level;
		if (Global.level == 5 && Global.isNewGame)
			totalScore += 10000; // Bonus for completing the game

		GetTree().CreateTimer(0.01f).Connect("timeout",this,"IncreaseTotalScore");

		GetTree().CreateTimer(0.01f).Connect("timeout",this,"IncreaseCoinScore");

		GetTree().CreateTimer(0.01f).Connect("timeout",this,"IncreaseTimeScore");
	}

	void IncreaseTotalScore()
	{
		if (totalScore > 0)
		{
			totalScore -= 100;
			Global.totalScore += 100;
			GetTree().CreateTimer(0.01f).Connect("timeout",this,"IncreaseTotalScore");
		}
		else
		{
			if (Global.coins < 1 && Global.time < 1)
			{
				if (Global.isSfxOn)
				{
					var sound = GetNode<AudioStreamPlayer>("Counter");
					sound.Stop();
				}

				isCountDone = true;
				EmitSignal("EndGame");
			}
		}
	}

	void IncreaseCoinScore()
	{
		// Bonus points for each coin collected
		if (Global.coins > 0)
		{
			Global.coins--;
			Global.totalScore += 100;
			GetTree().CreateTimer(0.01f).Connect("timeout",this,"IncreaseCoinScore");
		}
		else
		{
			if (totalScore < 1 && Global.time < 1)
			{
				if (Global.isSfxOn)
				{
					var sound = GetNode<AudioStreamPlayer>("Counter");
					sound.Stop();
				}

				isCountDone = true;
				EmitSignal("EndGame");
			}
		}
	}

	void IncreaseTimeScore()
	{
		// Give extra score for the remaining time.
		if (Global.time > 0)
		{
			Global.time--;
			Global.totalScore += 10;
			GetTree().CreateTimer(0.01f).Connect("timeout",this,"IncreaseTimeScore");
		}
		else
		{
			if (Global.coins < 1 && totalScore < 1)
			{
				if (Global.isSfxOn)
				{
					var sound = GetNode<AudioStreamPlayer>("Counter");
					sound.Stop();
				}

				isCountDone = true;
				EmitSignal("EndGame");
			}
		}
	}

	void IncreaseTime()
	{
		Global.time--;
		Global.totalTime++;
		if (Global.time < 1)
		{
			EmitSignal("Timeout"); // End the game if the timer runs out
			var timer = GetNode<Timer>("Timer");
			timer.Stop();
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		// Quit the game action
		if (@event.IsActionPressed("quit") && !Global.isReset && !Global.isChangingLevel && !Global.isEndGame && !Global.isHurt)
		{
			Global.StopMusic(GetTree().Root.GetNode("Area"),Global.level);

			Global.composer.GotoScene(Global.scenes["mainmenu"],true,"fade");
		}

		// Reset the level
		if (@event.IsActionPressed("restart") && !Global.isEndGame && !Global.isReset && !Global.isChangingLevel && !Global.isHurt)
		{
			Global.isReset = true;
            Global.coins = 0;
            Global.time = 100;
            Global.plrHealth = 3;
            Global.isImmune = false;

			Global.totalScore = Global.prevTotalScore;
			Global.totalKills = Global.prevTotalKills;
			Global.totalTime = Global.prevTotalTime;

			Global.totalDeaths++;
			Global.composer.GotoScene(Global.scenes["game"],true,"fade");

			Global.isReset = false;
		}


		if (@event.IsActionPressed("switchlevel") && isCountDone && Global.isEndGame && !Global.isReset && !Global.isChangingLevel && !Global.isHurt)
		{
			Global.isChangingLevel = true;

			// If we didn't start from the beggining then go to end screen
			if (!Global.isNewGame)
				Global.composer.GotoScene(Global.scenes["endgame"],true,"fade");
			else
			{
				Global.level++;
				Global.prevTotalScore = Global.totalScore;
				Global.prevTotalKills = Global.totalKills;
				Global.prevTotalTime = Global.totalTime;

				// Switch to a new level or to end screen if its the last level
				if (Global.level == 6)
					Global.composer.GotoScene(Global.scenes["endgame"],true,"fade");
				else
					Global.composer.GotoScene(Global.scenes["game"],true,"fade");
			}
		}
	}

    public override void _Process(float delta)
    {
		// Update the speedrun UI if speedrun mode is on
		if (!Global.isEndGame && !Global.isDead && Global.isSpeedrunOn)
		{
			double time = OS.GetSystemTimeMsecs(); // Get the current time

			double diff = (time - Global.startTime) / 1000; // Calculate the difference

			// Set the time value to the right variable (the current level)
			switch (Global.level)
			{
				case 1:
					Global.level1SpeedTime = diff;
					break;
				case 2:
					Global.level2SpeedTime = diff;
					break;
				case 3:
					Global.level3SpeedTime = diff;
					break;
				case 4:
					Global.level4SpeedTime = diff;
					break;
				case 5:
					Global.level5SpeedTime = diff;
					break;
			}

			// Update the text
			Global.totalSpeedTime = Global.level1SpeedTime + Global.level2SpeedTime + Global.level3SpeedTime + Global.level4SpeedTime + Global.level5SpeedTime;

			GetNode<Label>("SpeedrunUI/VFlowContainer/Level1Text").Text = "Level 1: " + Global.level1SpeedTime;
			GetNode<Label>("SpeedrunUI/VFlowContainer/Level2Text").Text = "Level 2: " + Global.level2SpeedTime;
			GetNode<Label>("SpeedrunUI/VFlowContainer/Level3Text").Text = "Level 3: " + Global.level3SpeedTime;
			GetNode<Label>("SpeedrunUI/VFlowContainer/Level4Text").Text = "Level 4: " + Global.level4SpeedTime;
			GetNode<Label>("SpeedrunUI/VFlowContainer/Level5Text").Text = "Level 5: " + Global.level5SpeedTime;
			GetNode<Label>("SpeedrunUI/VFlowContainer/TotalText").Text = "Total: " + Global.totalSpeedTime;
		}
    }
}
