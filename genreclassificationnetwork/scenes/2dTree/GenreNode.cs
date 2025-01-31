using Godot;
using System;

namespace GenreClassificationNetwork
{
	public struct NodeColorStyle
	{
		public Color ColorStart { get; set; }
		public Color ColorEnd { get; set; }

		public NodeColorStyle(Color start, Color end)
		{
			ColorStart = start;
			ColorEnd = end;
		}
	}

	public partial class GenreNode : OwnFdgNode
	{
		private bool _isDragging = false;
		private Vector2 _offset = Vector2.Zero;
		private Marker2D _pinchPan;
		public NodeColorStyle ColorStyle;

		public override void _Ready()
		{
			base._Ready();

			ShaderMaterial shaderMaterial = (ShaderMaterial)GetNode<Sprite2D>("Sprite2D").Material;
			ShaderMaterial uniqueMaterial = (ShaderMaterial)shaderMaterial.Duplicate();
			GetNode<Sprite2D>("Sprite2D").Material = uniqueMaterial;

		}


		public override void _Input(InputEvent @event)
		{
			_pinchPan = GetNode<Marker2D>("/root/Main/PinchPanCamera");

			if (@event is InputEventMouseButton mouseButtonEvent)
			{
				if (mouseButtonEvent.Pressed)
				{
					if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
					{
						// Check whether the mouse is pointing at the node
						if (GetGlobalRect().HasPoint(GetGlobalMousePosition()))
						{
							_isDragging = true;
							_offset = GetGlobalMousePosition() - GlobalPosition;
							_pinchPan.Set("enable_drag", !_isDragging);
						}
					}
				}
				else
				{
					// Release mouse button ends drag
					if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
					{
						_isDragging = false;
						_pinchPan.Set("enable_drag", !_isDragging);

					}
				}
			}
		}

		public override void _Process(double delta)
		{
			base._Process(delta);
			if (_isDragging)
			{
				GlobalPosition = GetGlobalMousePosition() - _offset;
			}
		}

		// Method to set the title of the genre
		public void setGenreTitle(String title)
		{
			Label genreTitle = GetNode<Label>("Sprite2D/Label");
			genreTitle.Text = title;

			// Center text
			genreTitle.HorizontalAlignment = HorizontalAlignment.Center;
			genreTitle.VerticalAlignment = VerticalAlignment.Center;
		}

		// Method to change the size of the node
		public void SetNodeSize(float scale)
		{
			Sprite2D sprite = GetNode<Sprite2D>("Sprite2D");
			sprite.Scale = new Vector2(scale, scale);

			Label genreTitle = GetNode<Label>("Sprite2D/Label");
			genreTitle.Scale = Vector2.One; // Scaling of the text remains constant
		}


		private Rect2 GetGlobalRect()
		{
			if (GetNodeOrNull<Sprite2D>("Sprite2D") is Sprite2D sprite)
			{
				Vector2 size = sprite.Texture.GetSize() * sprite.Scale;
				return new Rect2(GlobalPosition - size / 2, size);
			}
			return new Rect2();
		}

		public void SetNodeStyle(Color color_start, Color color_end)
		{
			ColorStyle = new(color_start, color_end);
			Random random = new(42); // Use a fixed number as the seed here
			int randomInt = random.Next(1, 1000);

			ShaderMaterial shaderMaterial = (ShaderMaterial)GetNode<Sprite2D>("Sprite2D").Material;
			shaderMaterial.Set("shader_parameter/color_start", color_start);
			shaderMaterial.Set("shader_parameter/color_end", color_end);
			shaderMaterial.Set("shader_parameter/time_step", randomInt);
		}

		public void SetNodeStyle(NodeColorStyle style)
		{
			ColorStyle = style;

			Random random = new Random(42); // Use a fixed number as the seed here
			int randomInt = random.Next(1, 666);

			ShaderMaterial shaderMaterial = (ShaderMaterial)GetNode<Sprite2D>("Sprite2D").Material;
			shaderMaterial.Set("shader_parameter/color_start", style.ColorStart);
			shaderMaterial.Set("shader_parameter/color_end", style.ColorEnd);
			shaderMaterial.Set("shader_parameter/time_step", randomInt);
		}
	}
}
