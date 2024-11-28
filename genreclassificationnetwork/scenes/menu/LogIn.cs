using Godot;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GenreTreeMenu {
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
			}
		}

		public async Task _OnLogInButtonPressed()
		{
			GD.Print("LogIn-Button wurde gedrückt!"); 
			
			string scope = "user-read-private user-read-email";
			string authUrl = $"https://accounts.spotify.com/authorize?response_type=code&client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope={Uri.EscapeDataString(scope)}";

			OpenWebsite(authUrl);

			// Warte auf den Authentifizierungscode
			string code = "AUTHORIZATION_CODE"; // Ersetze durch tatsächlichen Code
			string accessToken = await GetAccessToken(code);

			SpotifyDataManager.Instance.AccessToken = accessToken;

			string profileData = await GetUserProfile(accessToken);
			SpotifyDataManager.Instance.UserProfileData = profileData;

			GD.Print("Benutzerprofil geladen: ", profileData);
		}

		public void OpenWebsite(string url)
		{
			OS.ShellOpen(url);
		}

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

		public async Task<string> GetUserProfile(string accessToken)
		{
			using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
			{
				client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
				var response = await client.GetAsync("https://api.spotify.com/v1/me");
				return await response.Content.ReadAsStringAsync(); // JSON-Daten des Benutzerprofils
			}
		}
	}
}
