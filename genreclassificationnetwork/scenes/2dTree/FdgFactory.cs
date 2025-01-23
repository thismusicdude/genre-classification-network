using Godot;
using System;
using System.Collections.Generic;

namespace GenreClassificationNetwork
{
	public partial class FdgFactory : OwnForceDirectedGraph
	{
		private PackedScene genreNode;
		private Dictionary<string, GenreNode> genreMap = new();
		private GenreNode rootNode;
		private int mainGenreCount = 0;
		Marker2D pinchPanCamera;
		Camera2D TouchZoomCamera;

		public override void _Ready()
		{			
			base._Ready();

			// Root-Node erstellen
			Vector2 viewportCenter = GetViewportRect().Size / 2;
			//Vector2 viewportCenter = GetViewportRect().Size;

			//GD.Print("");
			//GD.Print($"GetViewportRect().Size: {GetViewportRect().Size}");
			//GD.Print($"viewportCenter: {viewportCenter}");
			
			
			genreNode = GD.Load<PackedScene>("res://scenes/2dTree/genreNode.tscn");
			rootNode = CreateGenreNode(viewportCenter, 1f);
			rootNode.setGenreTitle("Root");
			AddChild(rootNode);
			
			pinchPanCamera = GetNodeOrNull<Marker2D>("/root/Main/PinchPanCamera");
			TouchZoomCamera = GetNodeOrNull<Camera2D>("/root/Main/PinchPanCamera/TouchZoomCamera2D");
		
			//private Camera2D TouchZoomCamera;
			
			if (pinchPanCamera != null && TouchZoomCamera != null)
			{				
				pinchPanCamera.GlobalPosition = rootNode.GlobalPosition; // Kamera auf Root-Node zentrieren
				
				TouchZoomCamera.Zoom = new Vector2(0.8f, 0.8f);
			}
			else
			{
				GD.PrintErr("Keine PinchPanCamera gefunden!");
			}
			
			GD.Print($"Root-Node Position: {rootNode.GlobalPosition}");
			//GD.Print($"PinchPanCamera Position: {pinchPanCamera?.GlobalPosition}");
		}
		
		public override void _Process(double delta){
			base._Process(delta); // Andere Nodes können sich weiterhin bewegen

			if (pinchPanCamera != null)
			{
				pinchPanCamera.GlobalPosition = rootNode.GlobalPosition;
			}
			else 
			{
				
			}
		}

		private GenreNode CreateGenreNode(Vector2 position, float scale)
		{
			GenreNode node = genreNode.Instantiate<GenreNode>();
			node.Position = position;
			node.SetNodeSize(scale); // Größe setzen
			return node;
		}

		public void AddGenre(string name, double weight)
		{
			if (genreMap.ContainsKey(name))
				return;

			// Berechne dynamischen Abstand basierend auf der Anzahl der Hauptgenres
			float radius = 700f + mainGenreCount * 50f; // Dynamischer Abstand vom Root-Node
			Vector2 position = CalculateMainGenrePosition(radius);

			GenreNode nodeToAdd = CreateGenreNode(position, 0.75f); // Hauptgenre mit Skalierung 1.5
			nodeToAdd.setGenreTitle(name);
			AddChild(nodeToAdd);
			genreMap.Add(name, nodeToAdd);

			CreateConnection(rootNode, nodeToAdd);
		}

		public void AddSubGenre(string parent, string name, double weight)
		{
			if (!genreMap.ContainsKey(parent) || genreMap.ContainsKey(name))
				return;

			GenreNode parentNode = genreMap[parent];

			// Berechne Subgenre-Position mit gleichmäßiger Verteilung um das Hauptgenre
			int subGenreCount = CountSubGenres(parent);
			Vector2 position = CalculateSubGenrePosition(parentNode, subGenreCount);

			GenreNode nodeToAdd = CreateGenreNode(position, 0.55f); // Subgenre mit Skalierung 1.0
			nodeToAdd.setGenreTitle(name);
			AddChild(nodeToAdd);
			genreMap.Add(name, nodeToAdd);

			CreateConnection(parentNode, nodeToAdd);
		}

		private void CreateConnection(GenreNode startNode, GenreNode endNode)
		{
			OwnFdgSpring connection = new()
			{
				NodeStart = startNode,
				NodeEnd = endNode
			};

			// Verbindungseinstellungen
			if (startNode == rootNode) // Root → Hauptgenre
			{
				connection.K = 0.002f;      // Schwächere Federkraft
				connection.length = 450f; // Dynamischer Zielabstand
			}
			else // Hauptgenre → Subgenre
			{
				connection.K = 0.01f;      // Stärkere Federkraft
				connection.length = 400f; // Größerer Zielabstand
			}

			startNode.AddChild(connection);
			UpdateGraphSimulation();
		}

		private Vector2 CalculateMainGenrePosition(float radius)
		{
			const int maxMainGenres = 12; // Maximale Hauptgenres für gleichmäßige Verteilung
			float angleStep = 2 * Mathf.Pi / maxMainGenres; // Gleichmäßiger Winkel

			float angle = mainGenreCount * angleStep;
			mainGenreCount++; // Für das nächste Hauptgenre

			return rootNode.Position + new Vector2(
				Mathf.Cos(angle) * radius,
				Mathf.Sin(angle) * radius
			);
		}

		private Vector2 CalculateSubGenrePosition(GenreNode parent, int subGenreCount)
		{
			const float baseDistance = 150; // Mindestabstand zum Hauptgenre
			float radius = baseDistance + subGenreCount * 100f; // Dynamische Entfernung

			float angleStep = Mathf.Pi / 6; // Gleichmäßiger Winkel für Subgenres
			float angle = subGenreCount * angleStep; // Winkel basierend auf der Anzahl der Subgenres

			Vector2 direction = (parent.Position - rootNode.Position).Normalized();

			// Position weiter außen entlang des Vektors vom Hauptgenre
			return parent.Position + direction.Rotated(angle) * radius;
		}

		private int CountSubGenres(string parent)
		{
			int count = 0;
			foreach (var connection in genreMap)
			{
				if (connection.Value != null && connection.Key.StartsWith(parent))
				{
					count++;
				}
			}
			return count;
		}
	}
}
