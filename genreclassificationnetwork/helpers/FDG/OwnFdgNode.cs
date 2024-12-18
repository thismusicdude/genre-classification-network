using Godot;
using System;
using System.Threading.Tasks;


namespace GenreClassificationNetwork
{
	[GlobalClass, Icon("res://resources/FDGNode.svg")]
	[Tool]
	public partial class OwnFdgNode : Node2D
	{
		// Eigenschaften der Node
		public Vector2 velocity = new Vector2(0, 0);
		public Vector2 acceleration = new Vector2(0, 0);

		// Variablen für Physik und Knoten-Verhalten
		[Export] public bool pinned_down = false;
		[Export] public float radius = 50.0f;
		[Export] public float mass = 1.0f;
		[Export] public float repulsion = 0.3f;
		[Export] public float min_distance = 50.0f;
		[Export] public float MAX_SPEED = 5.0f;
		[Export] public float MIN_SPEED = 0.1f;
		[Export] public float DRAG = 0.7f;

		// Visuelle Einstellungen
		[ExportCategory("Visuals")]
		[Export] public bool draw_point = false;
		[Export] public Color point_color = new Color(1, 1, 1);  // Weiß

		// Wird aufgerufen, wenn die Node bereit ist
		public override void _Ready()
		{
			base._Ready();
			// Connect signals
			this.ChildExitingTree += OnChildExitingTree;
			this.ChildEnteredTree += OnChildEnteredTree;
		}

		// Zeichnet die Node als Punkt, wenn gewünscht
		public override void _Draw()
		{
			if (draw_point)
			{
				DrawCircle(Vector2.Zero, radius, point_color);
			}
		}

		// Beschleunigt die Node mit einer gegebenen Kraft
		public void Accelerate(Vector2 force)
		{
			// Berechnet die Beschleunigung (F = m*a)
			acceleration += force / mass;
		}

		// Weist die Node an, sich von einem anderen Knoten abzuweisen
		public void Repulse(Node2D otherNode)
		{
			if (Position.DistanceTo(otherNode.Position) > radius + min_distance)
				return;

			// Berechnet die Abstoßkraft
			Vector2 force = Position.DirectionTo(otherNode.Position) * repulsion;

			// Wendet die Abstoßkraft an
			Accelerate(-force);
		}

		// Aktualisiert die Position der Node basierend auf ihrer Geschwindigkeit und Beschleunigung
		public void UpdatePosition()
		{
			// Stoppt, wenn die Node fixiert ist
			if (pinned_down)
				return;

			// Aktualisiert die Geschwindigkeit
			velocity += acceleration;

			// Reduziert die Geschwindigkeit durch Luftwiderstand
			velocity *= DRAG;

			// Begrenzung der Geschwindigkeit auf die maximal erlaubte Geschwindigkeit
			if (velocity.Length() > MAX_SPEED)
			{
				velocity = velocity.Normalized() * MAX_SPEED;
			}

			// Stoppt, wenn die Geschwindigkeit zu niedrig ist
			if (velocity.Length() < MIN_SPEED)
			{
				velocity = Vector2.Zero;
			}

			// Aktualisiert die Position
			Position += velocity;

			// Setzt die Beschleunigung zurück
			acceleration = Vector2.Zero;
		}

		// Wird aufgerufen, wenn ein Kind die Baumstruktur verlässt
		public void OnChildExitingTree(Node child)
		{
			var parent = GetParent();
			if (parent is OwnForceDirectedGraph fGraph && fGraph.IsNodeReady())
			{
				fGraph.UpdateGraphSimulation();
			}
		}

		// Wird aufgerufen, wenn ein Kind in die Baumstruktur eintritt
		public void OnChildEnteredTree(Node child)
		{
			var parent = GetParent();
			if (parent is OwnForceDirectedGraph fGraph && fGraph.IsNodeReady())
			{
				fGraph.UpdateGraphSimulation();
			}
		}

		// Überprüft, ob es Warnungen zur Konfiguration gibt
		public string[] GetConfigurationWarnings()
		{
			var warnings = new System.Collections.Generic.List<string>();

			if (!(GetParent() is OwnForceDirectedGraph))
			{
				warnings.Add("Node is not a child of a ForceDirectedGraph.");
			}

			return warnings.ToArray();
		}

		// Setzt den Wert für `draw_point` und löst eine Neuzeichnung aus
		private void _SetDrawPoint(bool value)
		{
			draw_point = value;
			QueueRedraw();
		}

		// Setzt die Farbe des Punkts und löst eine Neuzeichnung aus
		private void _SetPointColor(Color value)
		{
			point_color = value;
			QueueRedraw();
		}
	}
}
