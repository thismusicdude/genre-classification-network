using Godot;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;


namespace GenreClassificationNetwork
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
				GD.PrintErr("logInButton couldnt be accessed!");
			}
			else
			{
				logInButton.Connect(Button.SignalName.Pressed, Callable.From(_OnLogInButtonPressed));
				GD.Print($"{logInButton.Name} - logInButton connected");
			}

		}

		// Open URL in Standard Browser
		public static void OpenWebsite(string url)
		{
			OS.ShellOpen(url);
		}

		// Get Access Token from Spotify
		public async Task<string> GetAccessToken(string code)
		{
			using System.Net.Http.HttpClient client = new();
			var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token")
			{
				Content = new FormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("grant_type", "authorization_code"),
				new KeyValuePair<string, string>("code", code),
				new KeyValuePair<string, string>("redirect_uri", redirectUri),
				new KeyValuePair<string, string>("client_id", clientId),
				new KeyValuePair<string, string>("client_secret", clientSecret),
			})
			};

			var response = await client.SendAsync(request);
			var responseString = await response.Content.ReadAsStringAsync();
			var tokenData = JsonConvert.DeserializeObject<dynamic>(responseString);

			return tokenData.access_token;
		}

		// Get User Profile from Spotify
		public static async Task<string> GetUserProfile(string accessToken)
		{
			using System.Net.Http.HttpClient client = new();
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
			var response = await client.GetAsync("https://api.spotify.com/v1/me");
			return await response.Content.ReadAsStringAsync(); // JSON-Daten des Benutzerprofils
		}

		public async Task _OnLogInButtonPressed()
		{
			GD.Print("LogIn Button was pressed!");

			string scope = "user-read-private user-read-email user-top-read";
			string authUrl = $"https://accounts.spotify.com/authorize?response_type=code&client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope={Uri.EscapeDataString(scope)}";

			// Open Spotify-Auth Website
			OpenWebsite(authUrl);

			// start local server and wait for authentification
			var server = new LocalServer("http://localhost:5000/callback/");
			string code = await server.WaitForCodeAsync();

			if (string.IsNullOrEmpty(code))
			{
				GD.PrintErr("No Authentification code received!");
				return;
			}

			// Request Accesstoken
			string accessToken = await GetAccessToken(code);
			if (string.IsNullOrEmpty(accessToken))
			{
				GD.PrintErr("No Access Token received!");
				return;
			}

			if (SpotifyDataManager.Instance == null)
			{
				GD.PrintErr("SpotifyDataManager.Instance is null!");
				return;
			}

			SpotifyDataManager.Instance.AccessToken = accessToken;

			// request userprofile
			string profileData = await GetUserProfile(accessToken);

			SpotifyDataManager.Instance.UserProfileData = profileData;

			_ = GetTree().ChangeSceneToFile("res://scenes/2dTree/MainTreeScene.tscn");
		}


		public static async Task<string> GetTopTracks(string accessToken)
		{
			using System.Net.Http.HttpClient client = new();
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
			var response = await client.GetAsync("https://api.spotify.com/v1/me/top/tracks?limit=50");
			return await response.Content.ReadAsStringAsync();
		}

		/// DEMO PRINTS

		private static async Task DemoPrintTopGenres()
		{
			if (!SpotifyDataManager.Instance.isInitialized())
			{
				return;
			}

			var topGenres = await SpotifyDataManager.Instance.GetTopGenres(SpotifyDataManager.Instance.AccessToken);

			GD.Print("########\nYour Top-Genres (sorted by frequency):");
			foreach (var (genre, count) in topGenres)
			{
				GD.Print($"{genre}: {count}");
			}
		}

		public static async Task DemoPrintTrackNames()
		{
			if (!SpotifyDataManager.Instance.isInitialized())
			{
				return;
			}

			var trackNames = await SpotifyDataManager.Instance.GetTrackNamesAsList(SpotifyDataManager.Instance.AccessToken);

			GD.Print("#########\nYour Top Songs:");
			foreach (var name in trackNames)
			{
				GD.Print(name);
			}
		}


		private static async Task DemoPrintGenresAsListFromTracks()
		{
			if (!SpotifyDataManager.Instance.isInitialized())
			{
				return;
			}

			var topGenres = await SpotifyDataManager.Instance.GetTopGenresFromTracks(SpotifyDataManager.Instance.AccessToken);
			GD.Print("########\nTop genres based on the most listened to songs:");
			foreach (var (genre, count) in topGenres)
			{
				GD.Print($"{genre}: {count}");
			}
		}

		private static async Task DemoPrintGenreHierarchy()
		{

			if (!SpotifyDataManager.Instance.isInitialized())
			{
				return;
			}

			var genreHierarchy = await SpotifyDataManager.Instance.GetGenresWithSubgenres(SpotifyDataManager.Instance.AccessToken);

			GD.Print("########\nGenre hierarchy:");
			foreach (var (mainGenre, subgenres) in genreHierarchy)
			{
				GD.Print($"Main Genre: {mainGenre}");
				foreach (var subgenre in subgenres.Distinct())
				{
					GD.Print($"  - {subgenre}");
				}
			}
		}
	}
}
