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

        public int maxMainGenres = 12;
        public bool isLoadingGenre = false;

        public ImageTexture RootNodeTexture;

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

            pinchPanCamera = GetNodeOrNull<Marker2D>("../PinchPanCamera");
            TouchZoomCamera = GetNodeOrNull<Camera2D>("../PinchPanCamera/TouchZoomCamera2D");

            if (pinchPanCamera != null && TouchZoomCamera != null)
            {
                pinchPanCamera.GlobalPosition = rootNode.GlobalPosition;
                TouchZoomCamera.Zoom = new Vector2(0.8f, 0.8f);
            }
            else
            {
                GD.PrintErr("Couldnt Load Pinch Pan Camera!");
            }
        }

        private Texture2D loadJpgTexture(byte[] data)
        {
            Image image = new();
            Error err = image.LoadJpgFromBuffer(data);

            if (err != Error.Ok)
            {
                GD.PrintErr("Error while loading JPG: ", err);
                return null;
            }

            ImageTexture texture = ImageTexture.CreateFromImage(image);
            return texture;
        }

        public void setProfileImg(byte[] body)
        {

            Texture2D texture = loadJpgTexture(body);
            if (texture != null)
            {
                var sprite = this.GetNode<Sprite2D>("GenreNode/Sprite2D");
                if (sprite == null)
                {
                    GD.PrintErr("No Sprite called `GenreNode/Sprite2D` found");
                }
                Shader shader = GD.Load<Shader>("res://shader/round.gdshader");

                // create ShaderMaterial
                ShaderMaterial shaderMaterial = new()
                {
                    Shader = shader
                };
                sprite.Material = shaderMaterial;
                sprite.Texture = texture;

                var label = this.GetNode<Label>("GenreNode/Sprite2D/Label");
                label.Text = "";

            }
        }

        public override void _Process(double delta)
        {
            base._Process(delta); // Other nodes can continue to move

            if (pinchPanCamera != null && isLoadingGenre)
            {
                pinchPanCamera.GlobalPosition = rootNode.GlobalPosition;
            }

            UpdateGraphSimulation(delta);

            ApplyRepulsionForces(delta);
        }

        private void ApplyRepulsionForces(double delta)
        {
            const double repulsionStrength = 20000f;
            const double attractionStrength = 0.1f;

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


        private static Color GetRandomColor()
        {
            Random random = new();
            float r = (float)random.NextDouble(); // Random number between 0.0 and 1.0
            float g = (float)random.NextDouble();
            float b = (float)random.NextDouble();

            return new Color(r, g, b, 1.0f); // RGBA: Alpha here 1.0 (fully visible)
        }

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

            return new Rect2(min, max - min); // Bounding box as rectangle
        }

        private void AdjustCameraZoom()
        {
            if (TouchZoomCamera == null)
                return;

            // Calculate the bounding box of the nodes
            Rect2 boundingBox = GetNodesBoundingBox();

            // Determine viewport size
            Vector2 viewportSize = GetViewportRect().Size;

            // Calculate the required zoom
            float zoomX = boundingBox.Size.X / viewportSize.X;
            float zoomY = boundingBox.Size.Y / viewportSize.Y;

            // Select the larger zoom to ensure that everything is visible
            float zoomFactor = Mathf.Max(zoomX, zoomY);

            // Limit the zoom to a reasonable range
            zoomFactor = Mathf.Clamp(zoomFactor, 0.5f, 2f); // Example area

            // Set the zoom of the camera
            TouchZoomCamera.Zoom = new Vector2(zoomFactor, zoomFactor);
        }


        private void UpdateGraphSimulation(double delta)
        {
            // TODO: Connection between genres for repulsion
            const float repulsionStrength = 20000f; // Strength of the repulsive force
            const float attractionStrength = 0.1f; // Strength of the attractive force

            foreach (var nodeA in genreMap.Values)
            {
                Vector2 totalForce = Vector2.Zero;

                // Calculate repulsive forces between all nodes
                foreach (var nodeB in genreMap.Values)
                {
                    if (nodeA == nodeB) continue; // No power to yourself

                    Vector2 direction = nodeA.Position - nodeB.Position;
                    float distance = direction.Length();
                    if (distance > 0)
                    {
                        // Abstoßungskraft proportional zur Entfernung
                        Vector2 repulsionForce = direction.Normalized() * (repulsionStrength / (distance * distance));
                        totalForce += repulsionForce;
                    }
                }

                // Calculate attractive forces along the connections
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

                // Update the position of the node
                Vector2 newPosition = nodeA.Position + totalForce * (float)delta;
                nodeA.Position = newPosition;
            }
        }

        private GenreNode CreateGenreNode(Vector2 position, float scale)
        {
            GenreNode node = genreNode.Instantiate<GenreNode>();
            node.Position = position;
            node.SetNodeSize(scale); // Set size
            return node;
        }

        public void AddGenre(string name, double weight)
        {
            if (genreMap.ContainsKey(name))
                return;

            // Calculate dynamic distance based on the number of main genres
            float radius = 700f + mainGenreCount * 50f; // Dynamic distance from the root node
            Vector2 position = CalculateMainGenrePosition(radius);

            GenreNode nodeToAdd = CreateGenreNode(position, 0.75f); // Main genre with scaling 1.5
            nodeToAdd.setGenreTitle(name);
            AddChild(nodeToAdd);
            genreMap.Add(name, nodeToAdd);
            nodeToAdd.SetNodeStyle(GetRandomColor(), GetRandomColor());

            CreateConnection(rootNode, nodeToAdd);
        }


        public void AddSubGenre(string parent, string name, double weight)
        {
            if (!genreMap.ContainsKey(parent) || genreMap.ContainsKey(name))
                return;

            GenreNode parentNode = genreMap[parent];

            // Calculate subgenre position with even distribution around the main genre
            int subGenreCount = CountSubGenres(parent);
            Vector2 position = CalculateSubGenrePosition(parentNode, subGenreCount);

            GenreNode nodeToAdd = CreateGenreNode(position, 0.55f); // Subgenre with scaling 1.0
            nodeToAdd.setGenreTitle(name);
            nodeToAdd.SetNodeStyle(parentNode.ColorStyle);

            AddChild(nodeToAdd);
            genreMap.Add(name, nodeToAdd);

            CreateConnection(parentNode, nodeToAdd);
        }


        public void CreateConnection(string start, string end, bool isVisible = false)
        {
            GenreNode parentNode = genreMap[start];
            GenreNode childNode = genreMap[end];
            CreateConnection(parentNode, childNode, isVisible);

        }

        private void CreateConnection(GenreNode startNode, GenreNode endNode, bool isVisible = true)
        {
            OwnFdgSpring connection = new()
            {
                NodeStart = startNode,
                NodeEnd = endNode,
                draw_line = isVisible,
                
            };

            // Connection settings
            if (startNode == rootNode) // Root → Main genre
            {
                connection.K = 0.002f;      // Weaker spring force
                connection.length = 450f; // Dynamic target distance
            }
            else // Main genre → Subgenre
            {
                connection.K = 0.01f;      // Stronger spring force
                connection.length = 400f; // Larger target distance
            }

            startNode.AddChild(connection);
            UpdateGraphSimulation();
        }

        private Vector2 CalculateMainGenrePosition(float radius)
        {
            // Maximum main genres for even distribution
            float angleStep = 2 * Mathf.Pi / maxMainGenres; // Uniform angle

            float angle = mainGenreCount * angleStep;
            mainGenreCount++; // For the next main genre

            return rootNode.Position + new Vector2(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius
            );
        }

        private Vector2 CalculateSubGenrePosition(GenreNode parent, int subGenreCount)
        {
            const float baseDistance = 150; // Minimum distance to the main genre
            float radius = baseDistance + subGenreCount * 100f; // Dynamic removal

            float angleStep = Mathf.Pi / 6; // Uniform angle for subgenres
            float angle = subGenreCount * angleStep; // Angle based on the number of subgenres

            Vector2 direction = (parent.Position - rootNode.Position).Normalized();

            // Position further out along the vector from the main genre
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
