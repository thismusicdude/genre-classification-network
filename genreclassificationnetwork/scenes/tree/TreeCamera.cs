using Godot;
using System;


namespace MusicGenreTreeView {
	public partial class TreeCamera : Marker3D
	{
		
		[Export]
		public float rotationSpeed = 1;
		
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			float delta_f =  Convert.ToSingle(delta);
			if (Input.IsActionPressed("ui_right"))
			{
				RotateY(-rotationSpeed * delta_f);
			}
			if (Input.IsActionPressed("ui_left"))
			{
				RotateY(rotationSpeed * delta_f);
			}
		}
		
	}
}
