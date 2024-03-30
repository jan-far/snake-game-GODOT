using Godot;
using System;

public partial class game_over_layer : CanvasLayer
{
	[Signal]
	public delegate void RestartEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnRestartButtonPressed()
	{
		// Emit the signal in C#. In C#, emitting a signal is done by invoking the event.
		EmitSignal(nameof(Restart));
	}
}
