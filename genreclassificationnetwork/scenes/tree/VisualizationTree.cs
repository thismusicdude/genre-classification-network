using Godot;
using System;

namespace MusicGenreTreeView {
	public partial class VisualizationTree : Node3D
	{
		public override void _Ready()
		{
			var genreTreeNodes = GetNode<Node3D>("GenreTreeNodeCollection");
			if (genreTreeNodes == null)
			{
				GD.PrintErr("GenreTreeNodeCollection wurde nicht gefunden!");
			}
			else
			{
				GD.Print("GenreTreeNodeCollection erfolgreich gefunden.");
			}
			if (SpotifyDataManager.Instance.AccessToken != null)
			{
				GD.Print("Spotify Access Token vorhanden: ", SpotifyDataManager.Instance.AccessToken);

				// Hier Benutzer-/Genre-Daten weiterverarbeiten
				string userData = SpotifyDataManager.Instance.UserProfileData;
				if (!string.IsNullOrEmpty(userData))
				{
					GD.Print("Benutzerdaten: ", userData);

					// Beispiel: Genre-Daten visualisieren
					_VisualizeGenreData(userData);
				}
			}
			else
			{
				GD.PrintErr("Keine Spotify-Daten verfügbar!");
			}
		}

		private void _VisualizeGenreData(string userData)
		{
			// Genre-Daten in 3D-Knoten visualisieren
			GenreTreeNodeCollection nodeCollection = GetNode<GenreTreeNodeCollection>("GenreTreeNodeCollection");

			// Dummy-Daten
			nodeCollection.AddGenreNode("Pop", new Vector3(1, 0, 0), 0.5f, Color.FromHtml("#ff0000"));
			nodeCollection.AddGenreNode("Rock", new Vector3(-1, 0, 0), 0.5f, Color.FromHtml("#00ff00"));
		}
	}
}
