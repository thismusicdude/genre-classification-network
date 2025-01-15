using Godot;
using System;
using System.Collections.Generic;

namespace GenreClassificationNetwork
{


    public partial class FdgFactory : OwnForceDirectedGraph
    {

        private PackedScene genreNode;

        private Dictionary<string, GenreNode> genreMap = new();
        // private OwnForceDirectedGraph fdg;
        private GenreNode rootNode;

        private GenreNode GetGenreNodeInstance(Vector2 position)
        {
            GenreNode result = genreNode.Instantiate() as GenreNode;
            result.Position = position;
            return result;
        }

        public override void _Ready()
        {
            base._Ready();

            genreNode = GD.Load<PackedScene>("res://scenes/2dTree/genreNode.tscn");
            rootNode = GetGenreNodeInstance(new Vector2(250, 250));
            AddChild(rootNode);
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            base._Process(delta);
        }

        public void AddGenre(string name, double weight)
        {
            // if genre Map already contains the genre
            if (genreMap.ContainsKey(name))
            {
                return;
            }

            RandomNumberGenerator rng = new();
            GenreNode nodeToAdd = GetGenreNodeInstance(rootNode.Position + new Vector2(rng.Randfn(), rng.Randfn()));
            nodeToAdd.setGenreTitle(name);
            AddChild(nodeToAdd);
            genreMap.Add(name, nodeToAdd);

            // add connection to root
            OwnFdgSpring connectionToRoot = new()
            {
                NodeStart = rootNode,
                NodeEnd = nodeToAdd
            };

            rootNode.AddChild(connectionToRoot);
            UpdateGraphSimulation();
        }


        public void AddConnection(string from, string to)
        {
            // if genre Map already contain
            // if genre Map already contains the genre
            if (!genreMap.ContainsKey(from) && !genreMap.ContainsKey(to))
            {
                return;
            }

            genreMap.TryGetValue(from, out GenreNode fromNode);
            genreMap.TryGetValue(to, out GenreNode toNode);

            // add connection to root
            OwnFdgSpring connection = new()
            {
                NodeStart = fromNode,
                NodeEnd = toNode
            };

            fromNode.AddChild(connection);
            UpdateGraphSimulation();
        }


        public void AddSubGenre(string parent, string name, double weight)
        {
            // if genre Map already contains the genre
            if (!genreMap.ContainsKey(name))
            {
                // TODO: change weight
                AddGenre(parent, 500);
            }

            GenreNode parentNode = genreMap.GetValueOrDefault(parent);

            // add new node
            RandomNumberGenerator rng = new();


            GenreNode nodeToAdd = GetGenreNodeInstance(parentNode.Position + new Vector2(rng.Randf(), rng.Randf()));
            nodeToAdd.setGenreTitle(name);
            AddChild(nodeToAdd);

            genreMap.Add(name, nodeToAdd);

            // add connection to parent
            OwnFdgSpring connectionToRoot = new()
            {
                NodeStart = parentNode,
                NodeEnd = nodeToAdd
            };

            parentNode.AddChild(connectionToRoot);
            UpdateGraphSimulation();
        }
    }
}
