using Godot;
using System;

namespace MusicGenreTreeView {
	public partial class VisualizationTree : Node3D
	{
		Node3D genreTreeNodes; 
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{			
		}

		public void _OnAddSpherePressed(){
			GenreTreeNodeCollection nodeCollection = GetNode<GenreTreeNodeCollection>("GenreTreeNodeCollection");

			nodeCollection.AddGenreNode("Metal", Vector3.Zero, 0.5f, Color.FromHtml("#732b87"));
		}
	}
}