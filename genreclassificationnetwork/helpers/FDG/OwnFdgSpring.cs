using Godot;
using System;

namespace GenreClassificationNetwork
{
	[GlobalClass, Icon("res://resources/FDGSpring.svg")]
	[Tool]
	public partial class OwnFdgSpring : Line2D
	{
		[Signal]
		public delegate void ConnectionChangedEventHandler();

		private OwnFdgNode _NodeStart = null;
		// References for the nodes that are connected to the spring
		[Export]
		public OwnFdgNode NodeStart
		{
			get => _NodeStart;
			set
			{
				_NodeStart = value;
				ConnectNodeStart(value);
			}
		}

		private OwnFdgNode _NodeEnd = null;
		[Export]
		public OwnFdgNode NodeEnd
		{
			get => _NodeEnd;
			set
			{
				_NodeEnd = value;
				ConnectNodeEnd(value);

			}
		}



		// Spring length and spring constant
		[Export]
		public float length = 500.0f;
		[Export] public float K = 0.005f;

		// Visibility of the line
		[Export] public bool draw_line = true;

		// Line to represent the spring
		public Line2D connection;

		public override void _Ready()
		{
			base._Ready();
			Width = 2.0f;
			connection = new Line2D();  // Initialize Line2D for the display
			AddChild(connection);        // Add it as a child
		}

		// Connects the two nodes with the spring
		public void ConnectNodes(OwnFdgNode start, OwnFdgNode end)
		{
			NodeStart = start;
			NodeEnd = end;
			EmitSignal(nameof(ConnectionChanged));
		}

		// Adds a force to the connected nodes
		public void MoveNodes()
		{
			if (NodeStart == null || NodeEnd == null)
				return;

			Vector2 force = NodeEnd.Position - NodeStart.Position;
			float magnitude = K * (force.Length() - length);

			force = force.Normalized() * magnitude;

			NodeStart.Accelerate(force);
			NodeEnd.Accelerate(-force);
		}

		// Updates the position of the spring line
		public void UpdateLine()
		{
			connection.ClearPoints();

			if (!draw_line)
				return;

			if (NodeStart == null || NodeEnd == null)
				return;

			connection.AddPoint(NodeStart.Position);
			connection.AddPoint(NodeEnd.Position);
		}

		// Warnings on the configuration of the spring
		public string[] GetConfigurationWarnings()
		{
			var warnings = new System.Collections.Generic.List<string>();

			if (!(GetParent() is OwnFdgNode))
				warnings.Add("The FDGSpring should be a child of a OwnFdgNode");

			if (NodeStart == null || NodeEnd == null)
				warnings.Add("The FDGSpring should have two nodes connected to it");

			return warnings.ToArray();
		}

		// Getter and setter for NodeStart and NodeEnd
		public void ConnectNodeStart(OwnFdgNode node)
		{
			//  NodeStart = node;
			EmitSignal(nameof(ConnectionChanged));
		}

		public void ConnectNodeEnd(OwnFdgNode node)
		{
			//  NodeEnd = node;
			EmitSignal(nameof(ConnectionChanged));
		}
	}
}
