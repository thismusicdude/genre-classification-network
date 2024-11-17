using Godot;
using System;

public partial class VisualizationTree : Node3D
{
	
	[Export]
	public double rotationSpeed = 1.0;

	private Camera3D camera;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		camera = GetNode<Camera3D>("Camera3D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
		if (Input.IsActionPressed("ui_right"))
		{
			RotateY(-rotationSpeed * delta);
		}
		if (Input.IsActionPressed("ui_left"))
		{
			RotateY(rotationSpeed * delta);
		}
	}

    private void RotateY(double v)
    {
		camera.RotateY()
        throw new NotImplementedException();
    }

}



