using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Godot;
using Newtonsoft.Json;

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

	// API-Aufruf: Top-Künstler abrufen
	public async Task<string> GetTopArtists(string accessToken)
	{
		using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
		{
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
			var response = await client.GetAsync("https://api.spotify.com/v1/me/top/artists?limit=50");			return await response.Content.ReadAsStringAsync();
		}
	}

	// Genres aus Top-Künstlern extrahieren
	public async Task<List<(string Genre, int Count)>> GetTopGenres(string accessToken)
	{
		string artistData = await GetTopArtists(accessToken);
		//GD.Print($"{artistData}");

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

		/*
		GD.Print("Deine Top-Genres (sortiert nach Häufigkeit):");
		foreach (var entry in topGenres)
		{
			GD.Print($"{entry.Genre}: {entry.Count}");
		}
		*/
		return topGenres;
	}
}

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
		var loadGenresButton = GetNode<Button>("LoadGenresButton");
		if (loadGenresButton != null)
		{
			loadGenresButton.Connect(Button.SignalName.Pressed, Callable.From(DisplayTopGenres));
			GD.Print("LoadGenresButton verbunden!");
		}
	}
}
