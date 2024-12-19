using Godot;
using System;


namespace MusicGenreTreeView {
	public partial class GenreTreeNodeCollection : Node3D
	{

		PackedScene genreTreeNode = (PackedScene)ResourceLoader.Load("res://scenes/tree/GenreTreeNode.tscn");

		public void AddGenreNode(string title, Vector3 pos, float size, Color color){
			GenreTreeNode newNode = genreTreeNode.Instantiate<GenreTreeNode>() as GenreTreeNode;
			
			newNode.SetGenreTitle(title);
			newNode.SetSize(size);
			newNode.SetSphereColor(color);

			this.AddChild(newNode);
		}
	}
}
