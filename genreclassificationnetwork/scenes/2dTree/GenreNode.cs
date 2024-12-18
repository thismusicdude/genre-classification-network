using Godot;
using System;

namespace GenreClassificationNetwork
{
	public partial class GenreNode : OwnFdgNode
	{
		public override void _Ready()
		{
			base._Ready();
		}
		public override void _Process(double delta)
		{
			base._Process(delta);
		}

		public void setImagePath()
		{

		}

		public void setGenreTitle(String title)
		{
			Label genreTitle = GetNode<Label>("Sprite2D/Label");
			genreTitle.Text = title;
		}
	}
}
