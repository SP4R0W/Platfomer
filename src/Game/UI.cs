using Godot;
using System;

public class UI : Control
{

    private Label scoreLabel;
    private Label healthLabel;
    private Label timeLabel;
    private Label coinText;
    private Label levelText;

    public override void _Ready()
    {
        scoreLabel = GetNode<Label>("CanvasLayer/Panel/ScoreText");
        healthLabel = GetNode<Label>("CanvasLayer/Panel/HealthText");
        timeLabel = GetNode<Label>("CanvasLayer/Panel/TimeText");
        coinText = GetNode<Label>("CanvasLayer/Panel/CoinText");
        levelText = GetNode<Label>("CanvasLayer/Panel/LevelText");

        levelText.Text = "Level: " + Convert.ToString(Global.level);
    }

    public override void _Process(float delta)
    {
        coinText.Text = "Coins: " + Global.coins;
        timeLabel.Text = "Time: " + Global.time;
        if (Global.time < 31)
        {
            timeLabel.Modulate = new Color(1,0,0);
        }
        else
        {
            timeLabel.Modulate = new Color(1,1,1);
        }
        scoreLabel.Text = "Score: " + Global.totalScore;
        healthLabel.Text = "Health: " + Global.plrHealth;
    }
}
