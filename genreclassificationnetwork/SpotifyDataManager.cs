using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Godot;
using Newtonsoft.Json;


namespace GenreClassificationNetwork
{
	public partial class SpotifyDataManager : Node
	{
		public static SpotifyDataManager Instance { get; private set; }

		public string AccessToken { get; set; }
		public string UserProfileData { get; set; }

		public override void _Ready()
		{
			if (Instance == null)
			{
				Instance = this;
				SetProcess(false); // Node bleibt erhalten
			}
			else
			{
				QueueFree();
			}
		}


		// API-Aufruf: Profile Data
		public static async Task<string> GetProfileData(string accessToken)
		{
			using System.Net.Http.HttpClient client = new();
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
			var response = await client.GetAsync("https://api.spotify.com/v1/me");
			return await response.Content.ReadAsStringAsync();
		}


		// API-Aufruf: Top-Künstler abrufen
		public static async Task<string> GetTopArtists(string accessToken)
		{
			using System.Net.Http.HttpClient client = new();
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
			var response = await client.GetAsync("https://api.spotify.com/v1/me/top/artists?limit=50");
			return await response.Content.ReadAsStringAsync();
		}

		// Genres aus Top-Künstlern extrahieren
		public async Task<List<(string Genre, int Count)>> GetTopGenres(string accessToken)
		{
			string artistData = await GetTopArtists(accessToken);
			var artistResponse = JsonConvert.DeserializeObject<dynamic>(artistData);

			// Genres sammeln
			var genreList = new List<string>();
			foreach (var artist in artistResponse.items)
			{
				foreach (var genre in artist.genres)
				{
					genreList.Add((string)genre);
				}
			}

			// Doppelte Genres entfernen und nach Häufigkeit sortieren
			var topGenres = genreList
				.GroupBy(g => g)
				.OrderByDescending(g => g.Count())
				.Select(g => (Genre: g.Key, Count: g.Count()))
				.ToList();

			return topGenres;
		}

		public async Task<Dictionary<string, List<string>>> GetGenresWithSubgenres(string accessToken)
		{
			var topGenres = await GetTopGenres(accessToken);

			// Dictionary, um Hauptgenres und zugehörige Subgenres zu speichern
			var genreHierarchy = new Dictionary<string, List<string>>();

			foreach (var (genre, _) in topGenres)
			{
				var genreParts = genre.Split(' '); // Zerlege den Genre-String
				string mainGenre = genreParts.Last(); // Letztes Wort als Hauptgenre annehmen

				if (!genreHierarchy.ContainsKey(mainGenre))
				{
					genreHierarchy[mainGenre] = new List<string>();
				}

				genreHierarchy[mainGenre].Add(genre);
			}

			return genreHierarchy;
		}


		// Nur Genres ohne Zähler als Liste zurückgeben
		public async Task<List<string>> GetGenresAsList(string accessToken)
		{
			var topGenres = await GetTopGenres(accessToken);
			return topGenres.Select(g => g.Genre).ToList();
		}

		public async Task<List<(string Genre, int Count)>> GetTopGenresFromTracks(string accessToken)
		{
			string trackData = await GetTopTracks(accessToken);
			var trackResponse = JsonConvert.DeserializeObject<dynamic>(trackData);

			// Genres sammeln
			var genreList = new List<string>();
			foreach (var track in trackResponse.items)
			{
				foreach (var artist in track.artists)
				{
					var artistId = (string)artist.id;

					using System.Net.Http.HttpClient client = new();
					client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
					var artistResponse = await client.GetAsync($"https://api.spotify.com/v1/artists/{artistId}");
					var artistData = await artistResponse.Content.ReadAsStringAsync();
					var artistDetails = JsonConvert.DeserializeObject<dynamic>(artistData);

					foreach (var genre in artistDetails.genres)
					{
						genreList.Add((string)genre);
					}
				}
			}

			// Doppelte Genres entfernen und nach Häufigkeit sortieren
			var topGenres = genreList
				.GroupBy(g => g)
				.OrderByDescending(g => g.Count())
				.Select(g => (Genre: g.Key, Count: g.Count()))
				.ToList();

			return topGenres; // Rückgabe des Wertes
		}

		public async Task<List<string>> GetTrackNamesAsList(string accessToken)
		{
			string trackData = await GetTopTracks(accessToken);
			var trackResponse = JsonConvert.DeserializeObject<dynamic>(trackData);

			// Liedernamen sammeln
			var trackNames = new List<string>();
			foreach (var track in trackResponse.items)
			{
				string trackName = (string)track.name;
				trackNames.Add(trackName);
			}

			return trackNames;
		}

		public static async Task<string> GetTopTracks(string accessToken)
		{
			using System.Net.Http.HttpClient client = new();
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
			var response = await client.GetAsync("https://api.spotify.com/v1/me/top/tracks?limit=50");
			return await response.Content.ReadAsStringAsync();
		}
	}

	/*
		// Beispiel für die Verwendung
		public partial class LogIn : Control
		{
			private async Task DisplayTopGenres()
			{
				if (SpotifyDataManager.Instance == null || string.IsNullOrEmpty(SpotifyDataManager.Instance.AccessToken))
				{
					GD.PrintErr("SpotifyDataManager ist nicht initialisiert oder kein AccessToken vorhanden.");
					return;
				}

				var topGenres = await SpotifyDataManager.Instance.GetTopGenres(SpotifyDataManager.Instance.AccessToken);

				GD.Print("Deine Top-Genres:");
				foreach (var (genre, count) in topGenres)
				{
					GD.Print($"{genre}: {count}");
				}
			}


			public override void _Ready()
			{
				//var loadGenresButton = GetNode<Button>("LoadGenresButton");
				//if (loadGenresButton != null)
				//{
				//loadGenresButton.Connect(Button.SignalName.Pressed, Callable.From(DisplayTopGenres));
				//GD.Print("LoadGenresButton verbunden!");
				//}
				//
				//var listGenresButton = GetNode<Button>("ListGenresButton");
				//if (listGenresButton != null)
				//{
				//listGenresButton.Connect(Button.SignalName.Pressed, Callable.From(DisplayGenresAsList));
				//GD.Print("ListGenresButton verbunden!");
				//}
			}
		}
		*/
}
