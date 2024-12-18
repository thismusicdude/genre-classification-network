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
        [Export] public bool PinnedDown = false;
        [Export] public float Radius = 50.0f;
        [Export] public float Mass = 1.0f;
        [Export] public float repulsion = 0.3f;
        [Export] public float MinDistance = 50.0f;
        [Export] public float MAX_SPEED = 5.0f;
        [Export] public float MIN_SPEED = 0.1f;
        [Export] public float DRAG = 0.7f;

        private bool _DrawPoint = false;

        // Visuelle Einstellungen
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

        private Color _PointColor = new Color(1, 1, 1);  // Weiß

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
        } /* {
            get => _print_color;
            set
            {
                _print_color = value;
                _SetPointColor(value);
            }
        } */

        // Wird aufgerufen, wenn die Node bereit ist

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

        // Zeichnet die Node als Punkt, wenn gewünscht
        public override void _Draw()
        {
            if (DrawPoint)
            {
                DrawCircle(Vector2.Zero, Radius, PointColor);
            }
        }

        // Beschleunigt die Node mit einer gegebenen Kraft
        public void Accelerate(Vector2 force)
        {
            // Berechnet die Beschleunigung (F = m*a)
            acceleration += force / Mass;
        }

        // Weist die Node an, sich von einem anderen Knoten abzuweisen
        public void Repulse(Node2D otherNode)
        {
            if (Position.DistanceTo(otherNode.Position) > Radius + MinDistance)
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
            if (PinnedDown)
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

        // Setzt den Wert für `DrawPoint` und löst eine Neuzeichnung aus
        private void _SetDrawPoint(bool value)
        {
            DrawPoint = value;
            OwnRedrawQueue();
        }

        // Setzt die Farbe des Punkts und löst eine Neuzeichnung aus
        private void _SetPointColor(Color value)
        {
            PointColor = value;
            OwnRedrawQueue();
        }
    }
}
