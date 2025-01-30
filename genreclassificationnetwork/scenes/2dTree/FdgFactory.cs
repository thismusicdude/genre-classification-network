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

			Vector2 viewportCenter = GetViewportRect().Size / 2;
			
			genreNode = GD.Load<PackedScene>("res://scenes/2dTree/genreNode.tscn");
			rootNode = CreateGenreNode(viewportCenter, 1f);
			rootNode.setGenreTitle("Root");
			AddChild(rootNode);
			
			pinchPanCamera = GetNodeOrNull<Marker2D>("/root/Main/PinchPanCamera");
			TouchZoomCamera = GetNodeOrNull<Camera2D>("/root/Main/PinchPanCamera/TouchZoomCamera2D");
			
			if (pinchPanCamera != null && TouchZoomCamera != null) 
			{
				pinchPanCamera.GlobalPosition = rootNode.GlobalPosition;	
				TouchZoomCamera.Zoom = new Vector2(0.8f, 0.8f);
				//AdjustCameraZoom();
			}
			else 
			{
				GD.PrintErr("Keine PinchPanCamera gefunden!");
			}
		}
		
		public override void _Process(double delta){
			base._Process(delta); // Andere Nodes können sich weiterhin bewegen

			if (pinchPanCamera != null)
			{
				pinchPanCamera.GlobalPosition = rootNode.GlobalPosition;
			}
			else 
			{
				GD.PrintErr("Keine PinchPanCamera gefunden!");
			}
			
			UpdateGraphSimulation(delta);
			
			ApplyRepulsionForces(delta);

			
			// Kamera-Zoom dynamisch anpassen
			//AdjustCameraZoom();
		}
		
		private void ApplyRepulsionForces(double delta)
		{
			const double repulsionStrength = 20000f;
			const double attractionStrength = 0.1f;
			
			//var genreNodes = GetChildren().OfType<Node>().Where(n => n is GenreNode).Cast<GenreNode>().ToList();

			foreach (var nodeA in genreMap.Values)
			{
				Vector2 totalForce = Vector2.Zero;

				foreach (var nodeB in genreMap.Values)
				{
					if (nodeA == nodeB) continue;

					Vector2 direction = nodeA.Position - nodeB.Position;
					double distance = direction.Length();
					if (distance > 0)
					{
						Vector2 repulsionForce = direction.Normalized() * (float)(repulsionStrength / (distance * distance));
						totalForce += repulsionForce;
					}
				}

				foreach (var child in nodeA.GetChildren())
				{
					if (child is OwnFdgSpring spring)
					{
						if (spring.NodeEnd is GenreNode connectedNode)
						{
							Vector2 direction = connectedNode.Position - nodeA.Position;
							double distance = direction.Length();
							Vector2 attractionForce = direction.Normalized() * (float)(distance - spring.length) * (float)attractionStrength;
							totalForce += attractionForce;
						}
					}
				}

				nodeA.Position += totalForce * (float)delta;
			}
		}
		
		//private void CreateConnection(GenreNode startNode, GenreNode endNode)
		//{
			//OwnFdgSpring connection = new()
			//{
				//NodeStart = startNode,
				//NodeEnd = endNode,
				//length = 300f // Standardlänge, kann dynamisch angepasst werden
			//};
//
			//startNode.AddChild(connection);
		//}

		
		private Rect2 GetNodesBoundingBox()
		{
			if (genreMap.Count == 0)
				return new Rect2(rootNode.Position, Vector2.Zero);

			Vector2 min = rootNode.Position;
			Vector2 max = rootNode.Position;

			foreach (var node in genreMap.Values)
			{
				Vector2 pos = node.Position;
				min = new Vector2(Mathf.Min(min.X, pos.X), Mathf.Min(min.Y, pos.Y));
				max = new Vector2(Mathf.Max(max.X, pos.X), Mathf.Max(max.Y, pos.Y));
			}

			return new Rect2(min, max - min); // Bounding Box als Rechteck
		}
		
		private void AdjustCameraZoom()
		{
			if (TouchZoomCamera == null)
				return;

			// Bounding Box der Nodes berechnen
			Rect2 boundingBox = GetNodesBoundingBox();

			// Viewport-Größe ermitteln
			Vector2 viewportSize = GetViewportRect().Size;

			// Berechne den erforderlichen Zoom
			float zoomX = boundingBox.Size.X / viewportSize.X;
			float zoomY = boundingBox.Size.Y / viewportSize.Y;

			// Wähle den größeren Zoom, um sicherzustellen, dass alles sichtbar ist
			float zoomFactor = Mathf.Max(zoomX, zoomY);

			// Begrenze den Zoom auf einen vernünftigen Bereich
			zoomFactor = Mathf.Clamp(zoomFactor, 0.5f, 2f); // Beispielbereich

			// Setze den Zoom der Kamera
			TouchZoomCamera.Zoom = new Vector2(zoomFactor, zoomFactor);
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
				NodeEnd = endNode,
				//length = 300f // Standardlänge, kann dynamisch angepasst werden

			};

			//startNode.AddChild(connection);

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
		
		private void UpdateGraphSimulation(double delta)
		{
			// TODO: Verbindung zwischen Genres für Abstoßung
			const float repulsionStrength = 20000f; // Stärke der abstoßenden Kraft
			const float attractionStrength = 0.1f; // Stärke der anziehenden Kraft

			foreach (var nodeA in genreMap.Values)
			{
				Vector2 totalForce = Vector2.Zero;

				// Abstoßende Kräfte zwischen allen Nodes berechnen
				foreach (var nodeB in genreMap.Values)
				{
					if (nodeA == nodeB) continue; // Keine Kraft auf sich selbst

					Vector2 direction = nodeA.Position - nodeB.Position;
					float distance = direction.Length();
					if (distance > 0)
					{
						// Abstoßungskraft proportional zur Entfernung
						Vector2 repulsionForce = direction.Normalized() * (repulsionStrength / (distance * distance));
						totalForce += repulsionForce;
					}
				}

				// Anziehende Kräfte entlang der Verbindungen berechnen
				foreach (Node connection in nodeA.GetChildren())
				{
					if (connection is OwnFdgSpring spring && spring.NodeEnd is GenreNode connectedNode)
					{
						Vector2 direction = connectedNode.Position - nodeA.Position;
						float distance = direction.Length();
						Vector2 attractionForce = direction.Normalized() * (distance - spring.length) * attractionStrength;
						totalForce += attractionForce;
					}
				}

				// Position des Nodes aktualisieren
				Vector2 newPosition = nodeA.Position + totalForce * (float)delta;
				nodeA.Position = newPosition;
			}
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
