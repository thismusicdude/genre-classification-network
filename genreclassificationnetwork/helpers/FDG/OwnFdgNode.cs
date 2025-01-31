using Godot;
using System;
using System.Threading.Tasks;


namespace GenreClassificationNetwork
{
	[GlobalClass, Icon("res://resources/FDGNode.svg")]
	[Tool]
	public partial class OwnFdgNode : Node2D
	{
		// Properties of the node
		public Vector2 velocity = new Vector2(0, 0);
		public Vector2 acceleration = new Vector2(0, 0);

		// Variables for physics and node behavior
		[Export] public bool PinnedDown = false;
		[Export] public float Radius = 50.0f;
		[Export] public float Mass = 1.0f;
		[Export] public float repulsion = 0.3f;
		[Export] public float MinDistance = 50.0f;
		[Export] public float MAX_SPEED = 5.0f;
		[Export] public float MIN_SPEED = 0.1f;
		[Export] public float DRAG = 0.7f;

		private bool _DrawPoint = false;

		// Visual settings
		[ExportCategory("Visuals")]
		[Export]
		public bool DrawPoint
		{
			get => _DrawPoint;
			set
			{
				_DrawPoint = value;
				if (IsInsideTree())
				{
					try
					{
						_SetDrawPoint(value);
					}
					catch (Exception ex)
					{
						GD.PrintErr($"Error in _SetDrawPoint: {ex.Message}");
					}
				}
				else
				{
					GD.Print("Skipping _SetDrawPoint, not ready or null value");
				}
			}
		}

		private Color _PointColor = new(1, 1, 1);  // White

		[Export]
		public Color PointColor
		{
			get => _PointColor;
			set
			{
				_PointColor = value;
				if (IsInsideTree())
				{
					try
					{
						_SetPointColor(value);
					}
					catch (Exception ex)
					{
						GD.PrintErr($"Error in _SetPointColor: {ex.Message}");
					}
				}
				else
				{
					GD.Print("Skipping _SetPointColor, not ready");
				}
			}
		}

		// Called when the node is ready

		private void OwnRedrawQueue()
		{
			if (IsInsideTree())
			{
				QueueRedraw();
			}
		}

		public override void _Ready()
		{
			base._Ready();
			// Connect signals
			this.ChildExitingTree += OnChildExitingTree;
			this.ChildEnteredTree += OnChildEnteredTree;
		}

		// Draws the node as a point, if desired
		public override void _Draw()
		{
			if (DrawPoint)
			{
				DrawCircle(Vector2.Zero, Radius, PointColor);
			}
		}

		// Accelerates the node with a given force
		public void Accelerate(Vector2 force)
		{
			// Calculates the acceleration (F = m*a)
			acceleration += force / Mass;
		}

		// Instructs the node to reject itself from another node
		public void Repulse(Node2D otherNode)
		{
			if (Position.DistanceTo(otherNode.Position) > Radius + MinDistance)
				return;

			// Calculates the repulsive force
			Vector2 force = Position.DirectionTo(otherNode.Position) * repulsion;

			// Applies the repulsive force
			Accelerate(-force);
		}

		// Updates the position of the node based on its speed and acceleration
		public void UpdatePosition()
		{
			// Stops when the node is fixed
			if (PinnedDown)
				return;

			// Updates the speed
			velocity += acceleration;

			// Reduces speed through air resistance
			velocity *= DRAG;

			// Limiting the speed to the maximum permitted speed
			if (velocity.Length() > MAX_SPEED)
			{
				velocity = velocity.Normalized() * MAX_SPEED;
			}

			// Stops if the speed is too low
			if (velocity.Length() < MIN_SPEED)
			{
				velocity = Vector2.Zero;
			}

			// Updates the position
			Position += velocity;

			// Resets the acceleration
			acceleration = Vector2.Zero;
		}

		// Called when a child leaves the tree structure
		public void OnChildExitingTree(Node child)
		{
			var parent = GetParent();
			if (parent is OwnForceDirectedGraph fGraph && fGraph.IsNodeReady())
			{
				fGraph.UpdateGraphSimulation();
			}
		}

		// Called when a child enters the tree structure
		public void OnChildEnteredTree(Node child)
		{
			var parent = GetParent();
			if (parent is OwnForceDirectedGraph fGraph && fGraph.IsNodeReady())
			{
				fGraph.UpdateGraphSimulation();
			}
		}

		// Checks whether there are warnings about the configuration
		public string[] GetConfigurationWarnings()
		{
			var warnings = new System.Collections.Generic.List<string>();

			if (!(GetParent() is OwnForceDirectedGraph))
			{
				warnings.Add("Node is not a child of a ForceDirectedGraph.");
			}

			return warnings.ToArray();
		}

		// Sets the value for 'DrawPoint' and triggers a redraw
		private void _SetDrawPoint(bool value)
		{
			DrawPoint = value;
			OwnRedrawQueue();
		}

		// Sets the color of the dot and triggers a redraw
		private void _SetPointColor(Color value)
		{
			PointColor = value;
			OwnRedrawQueue();
		}
	}
}
