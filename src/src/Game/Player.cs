using Godot;
using System;

public class Player : KinematicBody2D
{

    [Signal]
    public delegate void GroundedUpdated(bool isGrounded);

    [Signal]
    public delegate void EnemyKilled();

    [Signal]
    public delegate void Killed();

    float _maxSpeed = 400;
    float _initialSpeed = 300;

    float gravity;
    float maxJumpVel;
    float minJumpVel;

    float maxJumpHeight = 3.25f * 64;
    float minJumpHeight = 1.2f * 64;
    float jumpDuration = 0.5f;

    float currentSpeed = 0;
    float speedIncrement = 500;

    float direction = 1;

    int hurtPhaseCount = 0;

    bool isDead = false;
    bool isGrounded = false;

    bool canMove = true;

    string state = "stand";

    Vector2 velocity = new Vector2(0,0);

    AnimatedSprite animSprite;
    RayCast2D ray;
    RayCast2D ray2;

    CollisionShape2D collisionShape;
    Tween tween;

    Panel endPanel;
    Label restartLabel;

    Timer jumpBuffer;

    public override void _Ready()
    {
        var game = GetParent().GetParent();
        game.Connect("Timeout",this,"Kill");
        game.Connect("EndGame",this,"LevelEnd");

        tween = GetNode<Tween>("Tween");
        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        animSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        ray = GetNode<RayCast2D>("RayCast2D");
        ray2 = GetNode<RayCast2D>("RayCast2D2");

        restartLabel = GetNode<Label>("UI/CanvasLayer/RetryText");
        endPanel = GetNode<Panel>("UI/CanvasLayer/EndPanel");

        jumpBuffer = GetNode<Timer>("Buffer");

        gravity = 2 * maxJumpHeight / Mathf.Pow(jumpDuration,2);
        maxJumpVel = -Mathf.Sqrt(2 * gravity * maxJumpHeight);
        minJumpVel = -Mathf.Sqrt(2 * gravity * minJumpHeight);

        restartLabel.Visible = false;
        endPanel.Visible = false;

        var timer = restartLabel.GetNode<Timer>("Timer");
        timer.Stop();

        var timer2 = endPanel.GetNode<Timer>("Timer");
        timer2.Stop();

        Global.isDead = false;
        isDead = false;
        Global.isHurt = false;
        canMove = true;
        animSprite.Animation = "stand";
        animSprite.Play();
        currentSpeed = 0;

        var levelEnd = GetParent().GetNode<LevelEnd>("LevelEnd");
        levelEnd.Connect("EndGameBG",this,"LevelEndBG");
    }

    void GetHurt()
    {
        if (!Global.isHurt)
        {
            Global.isHurt = true;
            var sound = GetNode<AudioStreamPlayer>("HurtSound");
            if (Global.isSfxOn)
                sound.Play();

            var sound1 = GetNode<AudioStreamPlayer>("JumpShort");
            if (Global.isSfxOn && !sound1.Playing)
                sound1.Play();

            velocity.y = minJumpVel; // A small jump whenever the player gets hurt

            if (Global.plrHealth > 0)
                Global.plrHealth--;

            if (Global.plrHealth < 1)
                Kill(); // Game over, we lost our health
            else
                GetTree().CreateTimer(0.1f).Connect("timeout",this,"PlayerBeep"); // Flash when player took a hit
        }
    }

    void PlayerBeep()
    {
        Visible = !Visible;
        hurtPhaseCount++;
        if (hurtPhaseCount < 7)
            GetTree().CreateTimer(0.1f).Connect("timeout",this,"PlayerBeep");
        else
            Visible = true;
            Global.isHurt = false;
    }

    void Beep()
    {
        restartLabel.Visible = !restartLabel.Visible;
    }

    void BeepEnd()
    {
        endPanel.Visible = !endPanel.Visible;
    }

    void LevelEndBG()
    {
        canMove = false;
    }

    void LevelEnd()
    {
        // Show the panel for the level end
        endPanel.Visible = true;
        var timer = endPanel.GetNode<Timer>("Timer");
        timer.Start();
    }

    void Kill()
    {
        Global.isDead = true;
        EmitSignal("Killed"); // Emit the signal

        var gameTimer = GetParent().GetParent().GetNode<Timer>("Timer");
        gameTimer.Stop();

        // Play the death sound
        var sound = GetNode<AudioStreamPlayer>("DeathSound");
        if (Global.isSfxOn)
            sound.Play();

        Global.totalDeaths++;
        canMove = false;
        Global.isHurt = false;
        isDead = true;

        currentSpeed = 0;
        velocity = Vector2.Zero;

		collisionShape.SetDeferred("disabled",true);

		tween.InterpolateProperty(this,"modulate:a",1.0,0,1,Tween.TransitionType.Sine,Tween.EaseType.InOut);
		tween.Start();

		GetTree().CreateTimer(1.25f).Connect("timeout",this,"ShowReset");
    }

    void ShowReset()
    {
        restartLabel.Visible = true;
        var timer = restartLabel.GetNode<Timer>("Timer");
        timer.Start();
    }

    public override void _Process(float delta)
    {
        // Don't allow the player
        if (Global.isChangingLevel)
            canMove = false;
        else
        {
            if (!Global.isEndGame)
                canMove = true;
        }

        velocity.x = 0;

        // Kill the player if he falls into a pit
        if (Position.y > 1080 && !isDead)
            Kill();

        // Jump buffer
        if (Input.IsActionPressed("jump"))
            jumpBuffer.Start();

        if (Input.IsActionPressed("jump") && IsOnFloor() && canMove && !jumpBuffer.IsStopped())
        {
            var sound = GetNode<AudioStreamPlayer>("JumpLong");
            if (Global.isSfxOn && !sound.Playing)
                sound.Play();

            velocity.y = maxJumpVel;
            jumpBuffer.Stop();
        }

        // Movement code
        if (Input.IsActionPressed("walk_left") && canMove)
        {
            animSprite.FlipH = true;
            direction = -1;

            if (currentSpeed < 1)
                currentSpeed = _initialSpeed;
            if (currentSpeed < _maxSpeed)
                currentSpeed += speedIncrement * delta;
        }

         if (Input.IsActionPressed("walk_right") && canMove)
         {
            animSprite.FlipH = false;
            direction = 1;

            if (currentSpeed < 1)
                currentSpeed = _initialSpeed;

            if (currentSpeed < _maxSpeed)
                currentSpeed += speedIncrement * delta;
        }

        // Emit particles if immune
        var particles = GetNode<CPUParticles2D>("ImmunityParticles");
        if (Global.isImmune)
        {
            particles.Emitting = true;
            currentSpeed = _maxSpeed * 1.25f;
        }
        else
            particles.Emitting = false;

        if (isDead)
            currentSpeed = 0;

        velocity.x = (currentSpeed * direction);

        // Stop the player if there's no input
        if (!Input.IsActionPressed("walk_right") && !Input.IsActionPressed("walk_left"))
        {
            currentSpeed = 0;
            velocity.x = 0;
        }

        if (!Input.IsActionPressed("jump") && velocity.y < minJumpVel)
        {
            var sound = GetNode<AudioStreamPlayer>("JumpLong");
            if (Global.isSfxOn && !sound.Playing)
                sound.Play();

            velocity.y = minJumpVel;
        }

        // Code responsible for "states", which are just animations for the player.
        if (IsOnFloor() && state == "fall")
        {
            var sound = GetNode<AudioStreamPlayer>("LandSound");
            if (Global.isSfxOn)
                sound.Play();
        }

        if (!IsOnFloor())
        {
            if (velocity.y < 0)
                state = "jump";
            else if (velocity.y > 0)
                state = "fall";
        }

        if (IsOnFloor() && velocity.x < 1)
            state = "stand";

        if (IsOnFloor() && (velocity.x < 0 || velocity.x > 1))
            state = "walk";

        if (isDead || Global.isHurt)
            state = "hurt";

        animSprite.Animation = state;
        animSprite.Play();
    }

    public override void _PhysicsProcess(float delta)
    {
        if (!isDead)
        {
            velocity.y += gravity * delta;

            velocity = MoveAndSlide(velocity,new Vector2(0,-1),true);

            var wasGrounded = isGrounded;
            isGrounded = IsOnFloor();

            if (isGrounded != wasGrounded)
                EmitSignal("GroundedUpdated",isGrounded);

            Vector2 point = ray2.GetCollisionPoint();
            Global.playerPos = new Vector2(GlobalPosition.x,point.y);
        }
    }

    void BarnacleHit(Barnacle enemy, AnimatedSprite animSprite)
    {
        if (!Global.isImmune)
        {
            if (velocity.y == 0)
            {
                enemy.Attack(); // Play the enemy's attack animation
                GetHurt();
            }
            else
            {
                // Barnacle can only be dead if its mouth is closed.
                if (animSprite.Animation == "closed" && !Global.isHurt)
                {
                    var sound = GetNode<AudioStreamPlayer>("JumpLong");
                    if (Global.isSfxOn && !sound.Playing)
                        sound.Play();

                    velocity.y = maxJumpVel;
                    enemy.Kill();
                    EmitSignal("EnemyKilled");
                }
                else
                {
                    enemy.Attack();
                    GetHurt();
                }
            }
        }
        else
        {
            // Kill the enemy if player has immunity
            enemy.Kill();
            EmitSignal("EnemyKilled");
        }
    }

    void PiranhaHit(Piranha enemy, AnimatedSprite animSprite)
    {
        if (!Global.isImmune)
        {
            if (velocity.y == 0)
                GetHurt();
            else
            {
                // You can only kill the piranha if you jump on its head
                if (animSprite.Animation == "jump" && enemy.GlobalPosition.y >= GlobalPosition.y && !Global.isHurt)
                {
                    var sound = GetNode<AudioStreamPlayer>("JumpLong");
                    if (Global.isSfxOn && !sound.Playing)
                        sound.Play();

                    velocity.y = maxJumpVel;
                    enemy.Kill();
                    EmitSignal("EnemyKilled");
                }
                else
                    GetHurt();
            }
        }
        else
        {
            // Kill the enemy if player has immunity
            enemy.Kill();
            EmitSignal("EnemyKilled");
        }
    }

    void SlimeHit(Slime enemy)
    {
        if (!Global.isImmune)
        {
            if (velocity.y == 0)
                GetHurt();
            else
            {
                if (!Global.isHurt)
                {
                    var sound = GetNode<AudioStreamPlayer>("JumpLong");
                    if (Global.isSfxOn && !sound.Playing)
                        sound.Play();

                    velocity.y = maxJumpVel;
                    enemy.Kill();
                    EmitSignal("EnemyKilled");
                }
            }
        }
        else
        {
            // Kill the enemy if player has immunity
            enemy.Kill();
            EmitSignal("EnemyKilled");
        }
    }

    void GhostHit(Ghost enemy)
    {
        if (!Global.isImmune)
            GetHurt(); // Ghost cannot be killed without immunity.
        else
        {
            // Kill the enemy if player has immunity
            var sound = GetNode<AudioStreamPlayer>("JumpLong");
            if (Global.isSfxOn && !sound.Playing)
                sound.Play();

            velocity.y = maxJumpVel;
            enemy.Kill();
            EmitSignal("EnemyKilled");
        }
    }

    void SpiderHit(Spider enemy)
    {
        if (!Global.isImmune)
        {
            if (velocity.y == 0)
                GetHurt();
            else
            {
                if (!Global.isHurt)
                {
                    var sound = GetNode<AudioStreamPlayer>("JumpLong");
                    if (Global.isSfxOn && !sound.Playing)
                        sound.Play();

                    velocity.y = maxJumpVel;
                    enemy.Kill();
                    EmitSignal("EnemyKilled");
                }
            }
        }
        else
        {
            // Kill the enemy if player has immunity
            enemy.Kill();
            EmitSignal("EnemyKilled");
        }
    }

    void BeeHit(Bee enemy)
    {
        if (!Global.isImmune)
        {
            if (velocity.y == 0)
                GetHurt();
            else
            {
                // Check if the player jumped on bee's head
                if (enemy.GlobalPosition.y >= GlobalPosition.y && !Global.isHurt)
                {
                    var sound = GetNode<AudioStreamPlayer>("JumpLong");
                    if (Global.isSfxOn && !sound.Playing)
                        sound.Play();

                    velocity.y = maxJumpVel;
                    enemy.Kill();
                    EmitSignal("EnemyKilled");
                }
                else
                    GetHurt();
            }
        }
        else
        {
            // Kill the enemy if player has immunity
            enemy.Kill();
            EmitSignal("EnemyKilled");
        }
    }

    void BatHit(Bat enemy)
    {
        if (!Global.isImmune)
        {
            if (velocity.y == 0)
                GetHurt();
            else
            {
                // Check if the player jumped on bat's head
                if (enemy.GlobalPosition.y >= GlobalPosition.y && !Global.isHurt)
                {
                    var sound = GetNode<AudioStreamPlayer>("JumpLong");
                    if (Global.isSfxOn && !sound.Playing)
                        sound.Play();

                    velocity.y = maxJumpVel;
                    enemy.Kill();
                    EmitSignal("EnemyKilled");
                }
                else
                    GetHurt();
            }
        }
        else
        {
            // Kill the enemy if player has immunity
            enemy.Kill();
            EmitSignal("EnemyKilled");
        }
    }

    void SpinnerHit(Spinner enemy)
    {
        if (!Global.isImmune)
            GetHurt(); // Spinner cannot be killed without immunity
        else
        {
            // Kill the enemy if player has immunity
            var sound = GetNode<AudioStreamPlayer>("JumpLong");
            if (Global.isSfxOn && !sound.Playing)
                sound.Play();

            velocity.y = maxJumpVel;
            enemy.Kill();
            EmitSignal("EnemyKilled");
        }
    }
}
