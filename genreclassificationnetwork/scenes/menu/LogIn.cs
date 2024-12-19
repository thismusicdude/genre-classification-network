using Godot;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;


namespace GenreTreeMenu
{
	public partial class LogIn : Control
	{
		private string clientId = "e966134530674e7bac88f61cf3857a54";
		private string clientSecret = "c64692ea60064fb390696997f8850aa7";
		private string redirectUri = "http://localhost:5000/callback";

		public override void _Ready()
		{
			var logInButton = GetNode<Button>("LogInButton");
			if (logInButton == null)
			{
				GD.PrintErr("logInButton wurde nicht gefunden!");
			}
			else
			{
				logInButton.Connect(Button.SignalName.Pressed, Callable.From(_OnLogInButtonPressed));
				GD.Print("logInButton verbunden!");
				GD.Print($"LogInButton gefunden: {logInButton.Name}");
			}

			var loadGenresButton = GetNode<Button>("LoadGenresButton");
			if (loadGenresButton != null)
			{
				loadGenresButton.Connect(Button.SignalName.Pressed, Callable.From(DisplayTopGenres));
				GD.Print("LoadGenresButton verbunden!");
			}

			var listGenresButton = GetNode<Button>("ListGenresButton");
			if (listGenresButton != null)
			{
				listGenresButton.Connect(Button.SignalName.Pressed, Callable.From(DisplayGenresAsListFromTracks));
				GD.Print("ListGenresButton verbunden!");
			}
			
			var loadTracksButton = GetNode<Button>("LoadTracksButton");
			if (loadTracksButton != null)
			{
				loadTracksButton.Connect(Button.SignalName.Pressed, Callable.From(DisplayTrackNames));
				GD.Print("LoadTracksButton verbunden!");
			}
			
			var listSubgenresButton = GetNode<Button>("ListSubgenresButton");
			if (listSubgenresButton != null)
			{
				listSubgenresButton.Connect(Button.SignalName.Pressed, Callable.From(DisplayGenreHierarchy));
				GD.Print("ListSubgenresButton verbunden!");
			}
		}

		// Öffnet eine URL im Standardbrowser
		public void OpenWebsite(string url)
		{
			OS.ShellOpen(url);
		}

		// Holt das Access Token von Spotify
		public async Task<string> GetAccessToken(string code)
		{
			using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
			{
				var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
				request.Content = new FormUrlEncodedContent(new[]
				{
				new KeyValuePair<string, string>("grant_type", "authorization_code"),
				new KeyValuePair<string, string>("code", code),
				new KeyValuePair<string, string>("redirect_uri", redirectUri),
				new KeyValuePair<string, string>("client_id", clientId),
				new KeyValuePair<string, string>("client_secret", clientSecret),
			});

				var response = await client.SendAsync(request);
				var responseString = await response.Content.ReadAsStringAsync();
				var tokenData = JsonConvert.DeserializeObject<dynamic>(responseString);

				return tokenData.access_token;
			}
		}

		// Holt das Benutzerprofil von Spotify
		public async Task<string> GetUserProfile(string accessToken)
		{
			using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
			{
				client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
				var response = await client.GetAsync("https://api.spotify.com/v1/me");
				return await response.Content.ReadAsStringAsync(); // JSON-Daten des Benutzerprofils
			}
		}

		public async Task _OnLogInButtonPressed()
		{
			GD.Print("LogIn-Button wurde gedrückt!");

			string scope = "user-read-private user-read-email user-top-read";
			string authUrl = $"https://accounts.spotify.com/authorize?response_type=code&client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope={Uri.EscapeDataString(scope)}";

			// Öffne Spotify-Authentifizierungsseite
			OpenWebsite(authUrl);

			// Starte lokalen Server und warte auf den Authentifizierungscode
			var server = new LocalServer("http://localhost:5000/callback/");
			string code = await server.WaitForCodeAsync();

			// Debug-Ausgabe des Codes
			//GD.Print($"Empfangener Authentifizierungscode: {code}");

			if (string.IsNullOrEmpty(code))
			{
				GD.PrintErr("Kein Authentifizierungscode empfangen!");
				return;
			}

			// Zugriffstoken anfordern
			string accessToken = await GetAccessToken(code);
			if (string.IsNullOrEmpty(accessToken))
			{
				GD.PrintErr("Kein Zugriffstoken erhalten!");
				return;
			}

			// GD.Print($"Empfangenes Access Token: {accessToken}");

			if (SpotifyDataManager.Instance == null)
			{
				GD.PrintErr("SpotifyDataManager.Instance ist null!");
				return;
			}
			else
			{
				GD.Print("SpotifyDataManager.Instance existiert.");
			}

			SpotifyDataManager.Instance.AccessToken = accessToken;

			// Benutzerprofil abrufen
			string profileData = await GetUserProfile(accessToken);

			//GD.Print($"Benutzerprofil: {profileData}");
			SpotifyDataManager.Instance.UserProfileData = profileData;
		}

		private async Task DisplayTopGenres()
		{
			if (SpotifyDataManager.Instance == null || string.IsNullOrEmpty(SpotifyDataManager.Instance.AccessToken))
			{
				GD.PrintErr("SpotifyDataManager ist nicht initialisiert oder kein AccessToken vorhanden.");
				return;
			}

			// Top-Genres abrufen
			var topGenres = await SpotifyDataManager.Instance.GetTopGenres(SpotifyDataManager.Instance.AccessToken);

			// Ergebnisse in der Konsole ausgeben
			GD.Print("");
			GD.Print("Deine Top-Genres (sortiert nach Häufigkeit):");
			foreach (var (genre, count) in topGenres)
			{
				GD.Print($"{genre}: {count}");
			}

			/*
			// Wenn nur die Namen der Genres benötigt werden
			var genreNames = topGenres.Select(g => g.Genre).ToList();
			GD.Print("Nur die Genre-Namen:");
			foreach (var genre in genreNames)
			{
				GD.Print(genre);
			}
			*/
		}
		
		// Beispiel für die Verwendung
		public async Task DisplayTrackNames()
		{
			if (SpotifyDataManager.Instance == null || string.IsNullOrEmpty(SpotifyDataManager.Instance.AccessToken))
			{
				GD.PrintErr("SpotifyDataManager ist nicht initialisiert oder kein AccessToken vorhanden.");
				return;
			}

			var trackNames = await SpotifyDataManager.Instance.GetTrackNamesAsList(SpotifyDataManager.Instance.AccessToken);

			GD.Print("");
			GD.Print("Deine Top-Lieder:");
			foreach (var name in trackNames)
			{
				GD.Print(name);
			}
		}

		public async Task<string> GetTopTracks(string accessToken)
		{
			using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
			{
				client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
				var response = await client.GetAsync("https://api.spotify.com/v1/me/top/tracks?limit=50");
				return await response.Content.ReadAsStringAsync();
			}
		}

		private async Task DisplayGenresAsListFromTracks()
		{
			if (SpotifyDataManager.Instance == null || string.IsNullOrEmpty(SpotifyDataManager.Instance.AccessToken))
			{
				GD.PrintErr("SpotifyDataManager ist nicht initialisiert oder kein AccessToken vorhanden.");
				return;
			}

			var topGenres = await SpotifyDataManager.Instance.GetTopGenresFromTracks(SpotifyDataManager.Instance.AccessToken);

			GD.Print("");
			GD.Print("Top-Genres basierend auf den meistgehörten Songs:");
			foreach (var (genre, count) in topGenres)
			{
				GD.Print($"{genre}: {count}");
			}
		}
		
		private async Task DisplayGenreHierarchy()
		{
			if (SpotifyDataManager.Instance == null || string.IsNullOrEmpty(SpotifyDataManager.Instance.AccessToken))
			{
				GD.PrintErr("SpotifyDataManager ist nicht initialisiert oder kein AccessToken vorhanden.");
				return;
			}

			var genreHierarchy = await SpotifyDataManager.Instance.GetGenresWithSubgenres(SpotifyDataManager.Instance.AccessToken);

			GD.Print("Genre-Hierarchie:");
			foreach (var (mainGenre, subgenres) in genreHierarchy)
			{
				GD.Print($"Hauptgenre: {mainGenre}");
				foreach (var subgenre in subgenres.Distinct())
				{
					GD.Print($"  - {subgenre}");
				}
			}
		}
	}
}
