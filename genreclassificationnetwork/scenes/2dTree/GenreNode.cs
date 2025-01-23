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

		// Methode, um den Titel des Genres zu setzen
		public void setGenreTitle(String title)
		{
			Label genreTitle = GetNode<Label>("Sprite2D/Label");
			genreTitle.Text = title;
			
			// Text zentrieren
			genreTitle.HorizontalAlignment = HorizontalAlignment.Center;
			genreTitle.VerticalAlignment = VerticalAlignment.Center;
		}

		// Methode, um die Größe des Knotens zu ändern
		public void SetNodeSize(float scale)
		{
			Sprite2D sprite = GetNode<Sprite2D>("Sprite2D");
			sprite.Scale = new Vector2(scale, scale);

			Label genreTitle = GetNode<Label>("Sprite2D/Label");
			//genreTitle.Scale = new Vector2(scale, scale); // Skalierung des Labels entsprechend anpassen
			genreTitle.Scale = Vector2.One; // Skalierung des Textes bleibt konstant
		}
	}
}
