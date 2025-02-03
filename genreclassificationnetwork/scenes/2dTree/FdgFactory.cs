using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

namespace GenreClassificationNetwork
{
	public partial class FdgFactory : OwnForceDirectedGraph
	{
		private readonly PackedScene genreNode = GD.Load<PackedScene>("res://scenes/2dTree/genreNode.tscn");
		private readonly Dictionary<string, GenreNode> genreMap = new();
		private GenreNode rootNode;
		private int mainGenreCount;
		private int MaxMainGenres = 10;		// TODO: change to dynamic
		private Marker2D pinchPanCamera;
		private Camera2D touchZoomCamera;
		private List<GenreNode> mainGenres = new(); 
		
		private const float TargetConnectionLength = 400f; // Einheitliche Länge
		private const float AdjustmentSpeed = 0.0025f; // Geschwindigkeit der Anpassung
		//private const float AdjustmentSpeed = 2.5f; // Geschwindigkeit der Anpassung
		private float elapsedTime = 0f;
		private const float TimeBeforeEqualizing = 5f; // Wartezeit in Sekunden

		public override void _Ready()
		{
			base._Ready();

			rootNode = CreateGenreNode(GetViewportRect().Size / 2, 1f);
			rootNode.setGenreTitle("Root");
			AddChild(rootNode);

			pinchPanCamera = GetNodeOrNull<Marker2D>("/root/Main/PinchPanCamera");
			touchZoomCamera = GetNodeOrNull<Camera2D>("/root/Main/PinchPanCamera/TouchZoomCamera2D");

			if (pinchPanCamera == null || touchZoomCamera == null)
			{
				GD.PrintErr("Keine PinchPanCamera gefunden!");
				return;
			}

			pinchPanCamera.GlobalPosition = rootNode.GlobalPosition;
			touchZoomCamera.Zoom = new Vector2(0.7f, 0.7f);
		}

		public override void _Process(double delta)
		{
			base._Process(delta);
			elapsedTime += (float)delta;
			
			if (elapsedTime > TimeBeforeEqualizing)
			{
				EqualizeConnectionLengths(delta);
			}

			if (pinchPanCamera != null)
				pinchPanCamera.GlobalPosition = rootNode.GlobalPosition;
				
			else
				GD.PrintErr("Keine PinchPanCamera gefunden!");

			UpdateGraphSimulation(delta);
			ApplyRepulsionForces(delta);
		}
		
private void EqualizeConnectionLengths(double delta)
{
	foreach (var node in genreMap.Values)
	{
		foreach (var connection in node.GetChildren().OfType<OwnFdgSpring>())
		{
			if (connection.NodeEnd is GenreNode connectedNode)
			{
				Vector2 direction = connectedNode.Position - node.Position;
				float currentLength = direction.Length();

				float adjustment = (TargetConnectionLength - currentLength) * (float)delta * AdjustmentSpeed;
				connectedNode.Position += direction.Normalized() * adjustment;
			}
		}
	}
}

		private void ApplyRepulsionForces(double delta)
		{
			const float RepulsionStrength = 80000f; // Höhere Abstoßung für gleichmäßige Verteilung
			const float MainGenreRepulsion = 120000f; // Hauptgenres stoßen sich noch stärker ab
			const float AttractionStrength = 0.3f; // Stärkere Anziehungskraft
			const float MaxRepulsionDistance = 1200f; // Keine extreme Abstoßung über große Distanzen hinaus
			const float MovementLimit = 70f; // Begrenze Bewegung pro Frame
			const float MinDistanceFromRoot = 800f; // Mindestabstand zur Root Node

			foreach (var nodeA in genreMap.Values)
			{
				Vector2 totalForce = Vector2.Zero;

				// Abstoßungskraft berechnen
				foreach (var nodeB in genreMap.Values.Where(nodeB => nodeA != nodeB))
				{
					Vector2 direction = nodeA.Position - nodeB.Position;
					float distanceSquared = direction.LengthSquared();
					
					if (distanceSquared > 0 && distanceSquared < MaxRepulsionDistance * MaxRepulsionDistance) // Begrenzte Abstoßung
					{
						float forceFactor = RepulsionStrength / Mathf.Pow(Mathf.Sqrt(distanceSquared), 1.5f); // Sanftere Abstoßung
						//Vector2 repulsionForce = direction.Normalized() * forceFactor;
						//totalForce += repulsionForce;
						totalForce += direction.Normalized() * forceFactor;
					}
				}

				//// Anziehungskraft berechnen
				//foreach (var child in nodeA.GetChildren().OfType<OwnFdgSpring>())
				//{
					//if (child.NodeEnd is GenreNode connectedNode)
					//{
						//Vector2 direction = connectedNode.Position - nodeA.Position;
						//float distance = direction.Length();
						//Vector2 attractionForce = direction.Normalized() * ((distance - child.length) * AttractionStrength);
						//totalForce += attractionForce;
					//}
				//}
//
				//if (totalForce.Length() > MovementLimit)
					//totalForce = totalForce.Normalized() * MovementLimit;
//
				//nodeA.Position += totalForce * (float)delta;
				
				if (mainGenres.Contains(nodeA))
				{
					foreach (var otherMainGenre in mainGenres.Where(g => g != nodeA))
					{
						Vector2 direction = nodeA.Position - otherMainGenre.Position;
						float distanceSquared = direction.LengthSquared();

						if (distanceSquared > 0 && distanceSquared < MaxRepulsionDistance * MaxRepulsionDistance)
						{
							float extraRepulsion = MainGenreRepulsion / Mathf.Pow(Mathf.Sqrt(distanceSquared), 1.5f);
							totalForce += direction.Normalized() * extraRepulsion;
						}
					}
				}

				if (mainGenres.Contains(nodeA))
				{
					Vector2 rootDirection = rootNode.Position - nodeA.Position;
					float rootDistance = rootDirection.Length();

					if (rootDistance < MinDistanceFromRoot)
					{
						Vector2 pushAway = -rootDirection.Normalized() * (MinDistanceFromRoot - rootDistance) * 0.2f;
						totalForce += pushAway;
					}
				}

				// **4️⃣ Bewegung begrenzen**
				if (totalForce.Length() > MovementLimit)
				{
					totalForce = totalForce.Normalized() * MovementLimit;
				}

				nodeA.Position += totalForce * (float)delta;
			}
		}

		private static Color GetRandomColor()
		{
			Random random = new();
			float r = (float)random.NextDouble();
			float g = (float)random.NextDouble();
			float b = (float)random.NextDouble();

			return new Color(r, g, b, 1.0f);
		}

		private Rect2 GetNodesBoundingBox()
		{
			if (!genreMap.Any()) return new Rect2(rootNode.Position, Vector2.Zero);

			Vector2 min = rootNode.Position;
			Vector2 max = rootNode.Position;

			foreach (var pos in genreMap.Values.Select(node => node.Position))
			{
				min = new Vector2(Mathf.Min(min.X, pos.X), Mathf.Min(min.Y, pos.Y));
				max = new Vector2(Mathf.Max(max.X, pos.X), Mathf.Max(max.Y, pos.Y));
			}

			return new Rect2(min, max - min);
		}

		private void AdjustCameraZoom()
		{
			if (touchZoomCamera == null) return;

			Rect2 boundingBox = GetNodesBoundingBox();
			Vector2 viewportSize = GetViewportRect().Size;

			// Berechnung des Zoom-Faktors unter Berücksichtigung beider Achsen
			float zoomFactor = Mathf.Clamp(
				Mathf.Max(boundingBox.Size.X / viewportSize.X, boundingBox.Size.Y / viewportSize.Y),
				0.5f, 2f
			);

			touchZoomCamera.Zoom = new Vector2(zoomFactor, zoomFactor);
		}

		private void UpdateGraphSimulation(double delta)
		{
			const float repulsionStrength = 20000f; 
			const float attractionStrength = 0.1f;

			foreach (var nodeA in genreMap.Values)
			{
				Vector2 totalForce = Vector2.Zero;

				// Abstoßende Kräfte zwischen allen Nodes berechnen
				foreach (var nodeB in genreMap.Values)
				{
					if (nodeA == nodeB) continue;

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

				Vector2 newPosition = nodeA.Position + totalForce * (float)delta;
				nodeA.Position = newPosition;
			}
		}

		private GenreNode CreateGenreNode(Vector2 position, float scale)
		{
			GenreNode node = genreNode.Instantiate<GenreNode>();
			node.Position = position;
			node.SetNodeSize(scale);
			return node;
		}
		
		public void AddGenre(string name, double weight)
		{
			if (genreMap.ContainsKey(name)) return;
			
			GD.Print($"Adding genre {name}. Current count before: {mainGenres.Count}/{MaxMainGenres}");

			var position = CalculateMainGenrePosition(700f + mainGenres.Count * 50f);
			var nodeToAdd = CreateGenreNode(position, 0.75f);

			nodeToAdd.setGenreTitle(name);
			nodeToAdd.SetNodeStyle(GetRandomColor(), GetRandomColor());

			AddChild(nodeToAdd);
			genreMap[name] = nodeToAdd;
			mainGenres.Add(nodeToAdd);

			CreateConnection(rootNode, nodeToAdd);

			int count = mainGenres.Count;

			if (count > 1)
				CreateConnection(mainGenres[count - 2], nodeToAdd);

			if (count > 2 && count == MaxMainGenres) 
				CreateConnection(mainGenres[0], nodeToAdd);
		}

		public void AddSubGenre(string parent, string name, double weight)
		{
			if (genreMap.TryGetValue(parent, out GenreNode parentNode) && !genreMap.ContainsKey(name))
			{
				GenreNode nodeToAdd = CreateGenreNode(CalculateSubGenrePosition(parentNode, CountSubGenres(parent)), 0.55f);
				nodeToAdd.setGenreTitle(name);
				nodeToAdd.SetNodeStyle(parentNode.ColorStyle);

				AddChild(nodeToAdd);
				genreMap[name] = nodeToAdd;

				CreateConnection(parentNode, nodeToAdd);
			}
		}

		private void CreateConnection(GenreNode startNode, GenreNode endNode)
		{
			float k = (startNode == rootNode) ? 0.002f : 0.01f;
			float length = (startNode == rootNode) ? 450f : 400f;

			OwnFdgSpring connection = new()
			{
				NodeStart = startNode,
				NodeEnd = endNode,
				K = k,
				length = length
			};

			startNode.AddChild(connection);
			UpdateGraphSimulation();
		}

		private Vector2 CalculateMainGenrePosition(float radius)
		{
			float angle = mainGenreCount * (2 * Mathf.Pi / MaxMainGenres);
			mainGenreCount++;

			return rootNode.Position + new Vector2(
				Mathf.Cos(angle) * radius,
				Mathf.Sin(angle) * radius
			);
		}

		private Vector2 CalculateSubGenrePosition(GenreNode parent, int subGenreCount)
		{
			const float BaseDistance = 150f; 
			float radius = BaseDistance + subGenreCount * 100f;
			float angle = subGenreCount * (Mathf.Pi / 6); // Winkel berechnen
			Vector2 direction = (parent.Position - rootNode.Position).Normalized();

			return parent.Position + direction.Rotated(angle) * radius;
		}

		private int CountSubGenres(string parent)
		{
			return genreMap.Count(connection => connection.Value != null && connection.Key.StartsWith(parent));
		}
	}
}
