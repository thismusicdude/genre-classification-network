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
                SetProcess(false); // Node remains intact
            }
            else
            {
                QueueFree();
            }
        }


        public bool isInitialized()
        {
            if (Instance == null)
            {
                GD.PrintErr("SpotifyDataManager is not initialized");
                return false;
            }

            if (string.IsNullOrEmpty(Instance.AccessToken))
            {
                GD.PrintErr("SpotifyDataManager has no AccessToken.");
                return false;
            }
			return true;
        }

        // API-request: Profile Data
        public static async Task<string> GetProfileData(string accessToken)
        {
            using System.Net.Http.HttpClient client = new();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            var response = await client.GetAsync("https://api.spotify.com/v1/me");
            return await response.Content.ReadAsStringAsync();
        }


        // API-request: retrieve top artists
        public static async Task<string> GetTopArtists(string accessToken)
        {
            using System.Net.Http.HttpClient client = new();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            var response = await client.GetAsync("https://api.spotify.com/v1/me/top/artists?limit=50");
            return await response.Content.ReadAsStringAsync();
        }

        // API-request: Extracting genres from top artists
        public async Task<List<(string Genre, int Count)>> GetTopGenres(string accessToken)
        {
            string artistData = await GetTopArtists(accessToken);
            var artistResponse = JsonConvert.DeserializeObject<dynamic>(artistData);

            // Collect genres
            var genreList = new List<string>();
            foreach (var artist in artistResponse.items)
            {
                foreach (var genre in artist.genres)
                {
                    genreList.Add((string)genre);
                }
            }

            // Remove duplicate genres and sort by frequency
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

            // Dictionary to save main genres and associated subgenres
            var genreHierarchy = new Dictionary<string, List<string>>();

            foreach (var (genre, _) in topGenres)
            {
                var genreParts = genre.Split(' '); // Break down the genre string
                string mainGenre = genreParts.Last(); // Adopt last word as main genre

                if (!genreHierarchy.ContainsKey(mainGenre))
                {
                    genreHierarchy[mainGenre] = new List<string>();
                }

                genreHierarchy[mainGenre].Add(genre);
            }

            return genreHierarchy;
        }


        // Only return genres without a counter as a list
        public async Task<List<string>> GetGenresAsList(string accessToken)
        {
            var topGenres = await GetTopGenres(accessToken);
            return topGenres.Select(g => g.Genre).ToList();
        }

        public async Task<List<(string Genre, int Count)>> GetTopGenresFromTracks(string accessToken)
        {
            string trackData = await GetTopTracks(accessToken);
            var trackResponse = JsonConvert.DeserializeObject<dynamic>(trackData);

            // Collect sammeln
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

            // Remove duplicate genres and sort by frequency
            var topGenres = genreList
                .GroupBy(g => g)
                .OrderByDescending(g => g.Count())
                .Select(g => (Genre: g.Key, Count: g.Count()))
                .ToList();

            return topGenres; // Return of the value
        }

        public async Task<List<string>> GetTrackNamesAsList(string accessToken)
        {
            string trackData = await GetTopTracks(accessToken);
            var trackResponse = JsonConvert.DeserializeObject<dynamic>(trackData);

            // collect songtitles
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
}
