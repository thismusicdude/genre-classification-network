using Godot;
using System;

namespace GenreClassificationNetwork
{
	[GlobalClass, Icon("res://addons/force_directed_graph/icons/FDGSpring.svg")]
	[Tool]
	public partial class OwnFdgSpring : Line2D
	{
		// Signal-Äquivalent in C#
		[Signal]
		public delegate void ConnectionChangedEventHandler();

		// Referenzen für die Knoten, die mit der Feder verbunden sind
		[Export] public OwnFdgNode node_start;
		[Export] public OwnFdgNode node_end;

		// Federlänge und Federkonstante
		[Export] public float length = 500.0f;
		[Export] public float K = 0.005f;

		// Sichtbarkeit der Linie
		[Export] public bool draw_line = true;

		// Linie zur Darstellung der Feder
		public Line2D connection;

		public override void _Ready()
		{
			base._Ready();
			Width = 2.0f;
			connection = new Line2D();  // Initialisiere Line2D für die Darstellung
			AddChild(connection);        // Füge es als Kind hinzu
		}

		// Verbindet die zwei Knoten mit der Feder
		public void ConnectNodes(OwnFdgNode start, OwnFdgNode end)
		{
			node_start = start;
			node_end = end;
			EmitSignal(nameof(ConnectionChanged));
		}

		// Fügt den verbundenen Knoten eine Kraft hinzu
		public void MoveNodes()
		{
			if (node_start == null || node_end == null)
				return;

			Vector2 force = node_end.Position - node_start.Position;
			float magnitude = K * (force.Length() - length);

			force = force.Normalized() * magnitude;

			node_start.Accelerate(force);
			node_end.Accelerate(-force);
		}

		// Aktualisiert die Position der Federlinie
		public void UpdateLine()
		{
			connection.ClearPoints();

			if (!draw_line)
				return;

			if (node_start == null || node_end == null)
				return;

			connection.AddPoint(node_start.Position);
			connection.AddPoint(node_end.Position);
		}

		// Warnungen zur Konfiguration der Feder
		public string[] GetConfigurationWarnings()
		{
			var warnings = new System.Collections.Generic.List<string>();

			if (!(GetParent() is OwnFdgNode))
				warnings.Add("The FDGSpring should be a child of a OwnFdgNode");

			if (node_start == null || node_end == null)
				warnings.Add("The FDGSpring should have two nodes connected to it");

			return warnings.ToArray();
		}

		// Getter und Setter für node_start und node_end
		public void _ConnectNodeStart(OwnFdgNode node)
		{
			node_start = node;
			EmitSignal(nameof(ConnectionChanged));
		}

		public void _ConnectNodeEnd(OwnFdgNode node)
		{
			node_end = node;
			EmitSignal(nameof(ConnectionChanged));
		}
	}
}
