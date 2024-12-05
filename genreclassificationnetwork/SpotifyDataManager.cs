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

/*
	// API-Aufruf: Top-Künstler abrufen
	public async Task<string> GetTopArtists(string accessToken)
	{
		using (HttpClient client = new HttpClient())
		{
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
			var response = await client.GetAsync("https://api.spotify.com/v1/me/top/artists?limit=50");
			return await response.Content.ReadAsStringAsync();
		}
	}

	// Genres aus Top-Künstlern extrahieren
	public async Task<List<string>> GetTopGenres(string accessToken)
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
			.Select(g => g.Key)
			.ToList();

		return topGenres;
	}
	*/
}
