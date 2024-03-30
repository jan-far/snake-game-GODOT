using Godot;
using System;

public partial class main : Node
{
	[Export]
	public PackedScene SnakeScene = ResourceLoader.Load<PackedScene>("res://snake_segment.tscn");

	// Member variables
	private int score;
	private bool gameStarted = false;
	private bool gameOver = false;
	private bool paused = false;

	// Grid variables
	private int cells = 30;
	private int cellSize = 50;

	// Food variables
	private Vector2 foodPos;
	private bool regenFood = true;

	// Snake variables
	private Godot.Collections.Array oldData = new Godot.Collections.Array();
	private Godot.Collections.Array snakeData = new Godot.Collections.Array();
	private Godot.Collections.Array snake = new Godot.Collections.Array();

	// Movement variables
	private Vector2 startPos = new Vector2(14, 14);
	private Vector2 up = new Vector2(0, -1);
	private Vector2 down = new Vector2(0, 1);
	private Vector2 left = new Vector2(-1, 0);
	private Vector2 right = new Vector2(1, 0);
	private Vector2 moveDirection;
	private bool canMove;

	public override void _Ready()
	{
		NewGame();
	}

	private void NewGame()
	{
		GetTree().Paused = false;
		GetTree().CallGroup("segments", "queue_free");
		(GetNode<CanvasLayer>("GameOverLayer"))?.Hide();
		score = 0;
		(GetNode<Label>("Score/ScoreLabel")).Text = $"SCORE: {score}";
		moveDirection = up;
		canMove = true;
		gameOver = false;
		GenerateSnake();
		MoveFood();
	}

	private void GenerateSnake()
	{
		oldData.Clear();
		snakeData.Clear();
		snake.Clear();
		for (int i = 0; i < 3; i++)
		{
			AddSegment(startPos + new Vector2(0, i));
		}
	}

	private void AddSegment(Vector2 pos)
	{
		snakeData.Add(pos);
		// Create a new Node2D instance as the wrapper for the segment
		var snakeSegmentWrapper = new Node2D();
		AddChild(snakeSegmentWrapper);

		var snakeSegment = SnakeScene.Instantiate() as Panel;

		if (snakeSegment != null)
		{
			// Add the Panel as a child of the wrapper Node2D
			snakeSegmentWrapper.AddChild(snakeSegment);

			// Position the wrapper, which in turn positions the Panel
			snakeSegmentWrapper.Position = (pos * cellSize);

			snake.Add(snakeSegmentWrapper);
		}
		else
		{
			GD.Print("Error: Snake segment instance is not a Panel");
		}
	}

	public override void _Process(double delta)
	{
		MoveSnake();
	}

	private void MoveSnake()
	{
		if (canMove)
		{
			if (Input.IsActionJustPressed("move_down") && moveDirection != up)
			{
				moveDirection = down;
				canMove = false;
				if (!gameStarted) StartGame();
			}
			if (Input.IsActionJustPressed("move_up") && moveDirection != down)
			{
				moveDirection = up;
				canMove = false;
				if (!gameStarted) StartGame();
			}
			if (Input.IsActionJustPressed("move_left") && moveDirection != right)
			{
				moveDirection = left;
				canMove = false;
				if (!gameStarted) StartGame();
			}
			if (Input.IsActionJustPressed("move_right") && moveDirection != left)
			{
				moveDirection = right;
				canMove = false;
				if (!gameStarted) StartGame();
			}
			if (Input.IsActionJustPressed("pause_game") && !gameOver)
			{
				if (gameStarted)
				{
					Label resultLabel = GetNode<Label>("GameOverLayer/ResultLabel");
					GetNode<Timer>("MoveTimer").Stop();
					GetTree().Paused = true;
					resultLabel.Modulate = new Color(1, 1, 0, 1);
					resultLabel.Text = "Game Paused";
					GetNode<Button>("GameOverLayer/RestartButton").Text = "Continue";
					(GetNode<CanvasLayer>("GameOverLayer"))?.Show();
					paused = true;
					canMove = false;
				}
			}
		}
	}

	private void StartGame()
	{
		gameStarted = true;
		(GetNode<Timer>("MoveTimer")).Start();
	}

	private void OnMoveTimerTimeout()
	{
		canMove = true;
		oldData = new Godot.Collections.Array(snakeData);
		snakeData[0] = (Vector2)snakeData[0] + moveDirection;
		for (int i = 0; i < snakeData.Count; i++)
		{
			if (i > 0)
			{
				snakeData[i] = oldData[i - 1];
			}
				((Node2D)snake[i]).Position = ((Vector2)snakeData[i] * cellSize);
		}
		CheckOutOfBounds();
		CheckSelfEaten();
		CheckFoodEaten();
	}
	private void CheckOutOfBounds()
	{
		Vector2 snakeHeadPosition = (Vector2)snakeData[0];
		if (snakeHeadPosition.X < 0 || snakeHeadPosition.X > cells - 1 || snakeHeadPosition.Y < -1 || snakeHeadPosition.Y > cells - 1)
		{
			EndGame();
		}
	}


	private void CheckSelfEaten()
	{
		for (int i = 1; i < snakeData.Count; i++)
		{
			if ((Vector2)snakeData[0] == (Vector2)snakeData[i])
			{
				EndGame();
			}
		}
	}

	private void CheckFoodEaten()
	{
		if ((Vector2)snakeData[0] == foodPos)
		{
			score++;
			GetNode<Label>("Score/ScoreLabel").Text = $"SCORE: {score}";
			regenFood = true;
			AddSegment((Vector2)oldData[oldData.Count - 1]);
			MoveFood();
		}
	}

	private void MoveFood()
	{
		while (regenFood)
		{
			foodPos = new Vector2((int)GD.RandRange(0, cells - 2), (int)GD.RandRange(0, cells - 2));
			regenFood = false;
			for (int i = 0; i < snakeData.Count; i++)
			{
				if (foodPos == (Vector2)snakeData[i])
				{
					regenFood = true;
				}
			}
		}
		GetNode<Sprite2D>("Food").Position = (foodPos * cellSize);
		regenFood = true;
	}

	private void EndGame()
	{
		Label resultLabel = GetNode<Label>("GameOverLayer/ResultLabel");
		gameOver = true;
		GetTree().Paused = true;
		(GetNode<Timer>("MoveTimer"))?.Stop();
		resultLabel.Text = "Game Over";
		resultLabel.Modulate = Colors.Red;
		GetNode<Button>("GameOverLayer/RestartButton").Text = "Play Again";
		(GetNode<CanvasLayer>("GameOverLayer"))?.Show();
		gameStarted = false;
	}

	private void OnPlayAgainButtonPressed()
	{
		if (gameOver)
		{
			(GetNode<CanvasLayer>("GameOverLayer"))?.Hide();
			NewGame();
		}
		else if (paused)
		{
			GetNode<Timer>("MoveTimer").Start();
			GetTree().Paused = false;
			(GetNode<CanvasLayer>("GameOverLayer"))?.Hide();
			paused = false;
			canMove = true;
		}
	}
}
