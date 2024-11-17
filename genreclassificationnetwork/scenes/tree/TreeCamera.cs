using Godot;
using System;

public partial class TreeCamera : Marker3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public override void _Input(InputEvent @event)
	{
		// Überprüft, ob die rechte Pfeiltaste gedrückt wird
		if (@event.IsActionPressed("ui_right"))
		{
			RotateY(-RotationSpeed * GetProcessDeltaTime());
		}
		
		// Überprüft, ob die linke Pfeiltaste gedrückt wird
		if (@event.IsActionPressed("ui_left"))
		{
			RotateY(RotationSpeed * GetProcessDeltaTime());
		}
		
		// Beispiel für Mausbewegungen
		if (@event is InputEventMouseMotion mouseEvent)
		{
			RotateY(-mouseEvent.Relative.x * RotationSpeed * 0.01f);
			RotateX(-mouseEvent.Relative.y * RotationSpeed * 0.01f);
		}
	}
}
