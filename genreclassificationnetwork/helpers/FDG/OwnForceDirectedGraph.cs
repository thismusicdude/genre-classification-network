using Godot;
using System;
using System.Collections.Generic;

namespace GenreClassificationNetwork
{
    [GlobalClass, Icon("res://resources/ForceDirectedGraph.svg")]
    [Tool]
    public partial class OwnForceDirectedGraph : Node2D
    {
        // Die OwnFdgNodes im Graph
        public List<OwnFdgNode> nodes = new List<OwnFdgNode>();
        // Die OwnFdgSprings im Graph
        public List<OwnFdgSpring> springs = new List<OwnFdgSpring>();

        // Wenn aktiviert, wird der Graph aktualisiert (nach Hinzufügen oder Entfernen von Knoten oder Federn)
        private bool _updateGraph;

        [Export]
        public bool UpdateGraph
        {
            get => _updateGraph;
            set
            {
                _updateGraph = value;
                SetUpdateGraph(value);
            }
        }
        // Ob der Graph aktiv ist
        [Export] public bool is_active = true;
        // Ob der Graph im Editor simuliert werden soll
        [Export] public bool simulate_in_editor = true;

        // Der Node, der die Verbindungen der Federn hält
        private Node2D connections;

        // Wird beim Start aufgerufen
        public override void _Ready()
        {
            base._Ready();
            // Verbindet die Signale für das Hinzufügen und Entfernen von Kindern
            // Button.SignalName.ButtonDown, Callable.From(OnButtonDown))
            Connect(OwnForceDirectedGraph.SignalName.ChildEnteredTree, new Callable(this, MethodName.OnChildEnteredTree));
            Connect(OwnForceDirectedGraph.SignalName.ChildExitingTree, new Callable(this, MethodName.OnChildExitingTree));

            // Erstelle einen neuen Node für die Verbindungen
            connections = new Node2D { Name = "Connections" };
            AddChild(connections);

            // Bewege den Connections-Node nach hinten
            MoveChild(connections, 0);

            UpdateGraphSimulation();
        }

        // Wird jedes Frame aufgerufen
        public override void _Process(double delta)
        {
            base._Process(delta);

            if (!is_active)
                return;

            if (Engine.IsEditorHint() && !simulate_in_editor)
                return;

            // Berechne die Beschleunigung basierend auf den Federverbindungen
            foreach (var spring in springs)
            {
                spring.MoveNodes();
            }

            // Berechne die Abstoßung zwischen den Knoten
            foreach (var node in nodes)
            {
                foreach (var otherNode in nodes)
                {
                    if (node != otherNode)
                    {
                        node.Repulse(otherNode);
                    }
                }
            }

            // Aktualisiere die Federlinien
            foreach (var spring in springs)
            {
                spring.UpdateLine();
            }

            // Aktualisiere die Knotenpositionen
            foreach (var node in nodes)
            {
                node.UpdatePosition();
            }
        }

        // Aktualisiert die Knoten- und Feder-Arrays
        public void UpdateGraphSimulation()
        {
            UpdateGraphElements();
            UpdateConnections();
        }

        private Callable? connectionChangedCallable = null;
        // Füllt die Knoten- und Feder-Arrays mit den entsprechenden Knoten und Federn
        private void UpdateGraphElements()
        {
            // Leert die Knoten- und Feder-Arrays
            nodes.Clear();
            springs.Clear();

            foreach (Node child in GetChildren())
            {
                if (child is OwnFdgNode node)
                {
                    nodes.Add(node);
                    foreach (Node otherChild in node.GetChildren())
                    {
                        if (otherChild is OwnFdgSpring spring)
                        {
                            // Verbindet das Signal, falls es noch nicht verbunden ist
                            // spring.IsConnected(OwnFdgSpring.SignalName.ConnectionChanged,);
                            if (this.connectionChangedCallable == null)
                            {
                                this.connectionChangedCallable = new Callable(this, MethodName.OnConnectionChanged);
                            }

                            if (!spring.IsConnected("ConnectionChanged", connectionChangedCallable.Value))
                            {
                                Connect(OwnFdgSpring.SignalName.ConnectionChanged, connectionChangedCallable.Value);
                            }
                            springs.Add(spring);
                        }
                    }
                }
            }
        }

        // Aktualisiert den Connections-Node mit den Federn
        private void UpdateConnections()
        {
            // Löscht alle aktuellen Verbindungen
            foreach (Node child in connections.GetChildren())
            {
                ((Line2D)child).ClearPoints();
                connections.RemoveChild(child);
                child.QueueFree();
            }

            // Fügt neue Federlinien hinzu
            foreach (var spring in springs)
            {
                Line2D line = new Line2D
                {
                    Name = spring.Name,
                    Width = spring.Width,
                    WidthCurve = spring.WidthCurve,
                    DefaultColor = spring.DefaultColor,
                    Gradient = spring.Gradient,
                    Texture = spring.Texture,
                    TextureMode = spring.TextureMode,
                    JointMode = spring.JointMode,
                    BeginCapMode = spring.BeginCapMode,
                    EndCapMode = spring.EndCapMode,
                    SharpLimit = spring.SharpLimit,
                    RoundPrecision = spring.RoundPrecision,
                    Antialiased = spring.Antialiased
                };

                // Fügt die Linie zum Connections-Node hinzu
                connections.AddChild(line);

                // Setzt die Feder-Line2D-Verbindung zur neuen Linie
                spring.connection = line;

                // Aktualisiert die Linie
                spring.UpdateLine();
            }
        }

        // Wird aufgerufen, wenn sich eine Verbindung geändert hat
        private void OnConnectionChanged()
        {
            UpdateGraphSimulation();
        }

        // Wird aufgerufen, wenn ein Kind in den Baum eintritt
        private void OnChildEnteredTree(Node child)
        {
            UpdateGraphSimulation();
        }

        // Wird aufgerufen, wenn ein Kind den Baum verlässt
        private void OnChildExitingTree(Node child)
        {
            UpdateGraphSimulation();
        }

        // Gibt Warnungen zur Konfiguration zurück
        public string[] GetConfigurationWarnings()
        {
            var warnings = new List<string>();

            if (GetParent() is OwnFdgNode || GetParent() is OwnFdgSpring)
            {
                warnings.Add("The ForceDirectedGraph should not be a child of a OwnFdgNode or OwnFdgSpring.");
            }

            return warnings.ToArray();
        }

        // Setzt, ob der Graph aktualisiert werden soll
        private void SetUpdateGraph(bool value)
        {
            UpdateGraphSimulation();
        }
    }

}
