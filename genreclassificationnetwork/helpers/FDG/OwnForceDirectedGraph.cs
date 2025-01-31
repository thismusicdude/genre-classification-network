using Godot;
using System;
using System.Collections.Generic;

namespace GenreClassificationNetwork
{
	[GlobalClass, Icon("res://resources/ForceDirectedGraph.svg")]
	[Tool]
	public partial class OwnForceDirectedGraph : Node2D
	{
		// OwnFdgNodes in Graph
		public List<OwnFdgNode> nodes = new();
		// OwnFdgSprings in Graph
		public List<OwnFdgSpring> springs = new();

		// If activated, the graph is updated (after adding or removing nodes or springs)
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
		[Export] public bool is_active = true;
		[Export] public bool simulate_in_editor = true;
		private Node2D connections;

		public override void _Ready()
		{
			base._Ready();
			// Connects the signals for adding and removing children
			// Button.SignalName.ButtonDown, Callable.From(OnButtonDown))
			Connect(OwnForceDirectedGraph.SignalName.ChildEnteredTree, new Callable(this, MethodName.OnChildEnteredTree));
			Connect(OwnForceDirectedGraph.SignalName.ChildExitingTree, new Callable(this, MethodName.OnChildExitingTree));

			// Create a new node for the connections
			connections = new Node2D { Name = "Connections" };
			AddChild(connections);

			// Move the connections node backwards
			MoveChild(connections, 0);

			UpdateGraphSimulation();
		}

		// Is called every frame
		public override void _Process(double delta)
		{
			base._Process(delta);

			if (!is_active)
				return;

			if (Engine.IsEditorHint() && !simulate_in_editor)
				return;

			// Calculate the acceleration based on the spring connections
			foreach (var spring in springs)
			{
				spring.MoveNodes();
			}

			// Calculate the repulsion between the nodes
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

			// Update the spring lines
			foreach (var spring in springs)
			{
				spring.UpdateLine();
			}

			// Update the node positions
			foreach (var node in nodes)
			{
				node.UpdatePosition();
			}
		}

		// Updates the node and spring arrays
		public void UpdateGraphSimulation()
		{
			UpdateGraphElements();
			UpdateConnections();
		}

		private Callable? connectionChangedCallable = null;
		// Fills the node and spring arrays with the corresponding nodes and springs
		private void UpdateGraphElements()
		{
			// Empties the node and spring arrays
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
							// Connects the signal if it is not yet connected
							// spring.IsConnected(OwnFdgSpring.SignalName.ConnectionChanged,);
							if (connectionChangedCallable == null)
							{
								connectionChangedCallable = new Callable(this, MethodName.OnConnectionChanged);
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

		// Updates the connections node with the springs
		private void UpdateConnections()
		{
			// Deletes all current connections
			foreach (Node child in connections.GetChildren())
			{
				((Line2D)child).ClearPoints();
				connections.RemoveChild(child);
				child.QueueFree();
			}

			// Adds new spring lines
			foreach (var spring in springs)
			{
				Line2D line = new()
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

				// Adds the line to the connections node
				connections.AddChild(line);

				// Sets the spring Line2D connection to the new line
				spring.connection = line;

				// Updates the line
				spring.UpdateLine();
			}
		}

		// Is called when a connection has changed
		private void OnConnectionChanged()
		{
			UpdateGraphSimulation();
		}

		// Is called when a child enters the tree
		private void OnChildEnteredTree(Node child)
		{
			UpdateGraphSimulation();
		}

		// Is called when a child leaves the tree
		private void OnChildExitingTree(Node child)
		{
			UpdateGraphSimulation();
		}

		// Returns warnings about the configuration
		public string[] GetConfigurationWarnings()
		{
			var warnings = new List<string>();

			if (GetParent() is OwnFdgNode || GetParent() is OwnFdgSpring)
			{
				warnings.Add("The ForceDirectedGraph should not be a child of a OwnFdgNode or OwnFdgSpring.");
			}

			return warnings.ToArray();
		}

		// Sets whether the graph should be updated
		private void SetUpdateGraph(bool value)
		{
			UpdateGraphSimulation();
		}
	}

}
