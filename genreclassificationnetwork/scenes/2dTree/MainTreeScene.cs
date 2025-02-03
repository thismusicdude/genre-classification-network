using Godot;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GenreClassificationNetwork
{
    public partial class MainTreeScene : Node2D
    {
        TextureRect _imageDisplay;

        SpotifyUserProfileResponse profileData;
        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            _imageDisplay = GetNode<TextureRect>("CanvasLayer/HBoxContainer/ProfileImageContainer/ProfileImage");
            FetchSpotifyProfileAsync();

        }


        private async void FetchSpotifyProfileAsync()
        {
            try
            {
                string jsonProfile = await SpotifyDataManager.GetProfileData(SpotifyDataManager.Instance.AccessToken);
                profileData = JsonConvert.DeserializeObject<SpotifyUserProfileResponse>(jsonProfile);


                var usernameLabel = GetNode<Label>("CanvasLayer/HBoxContainer/UserName");
                GD.Print(profileData.Display_Name);
                usernameLabel.Text = profileData.Display_Name;
                GD.Print("Name erfolgreich geladen!");

                // Bild Laden
                HttpRequest _httpRequest;
                _httpRequest = new HttpRequest();
                AddChild(_httpRequest);

                _httpRequest.RequestCompleted += OnRequestCompleted;

                string imageUrl = profileData.Images[0].Url;
                GD.Print(imageUrl);

                var result = _httpRequest.Request(imageUrl);

                if (result != Error.Ok)
                {
                    GD.PrintErr($"Fehler beim Starten der HTTP-Anfrage: {result}");
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Fehler beim Abrufen der Spotify-Daten: {e.Message}");
            }
        }

        private void OnRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
        {
            if (responseCode == 200)
            {
                Image image = new();
                var error = image.LoadJpgFromBuffer(body);
                if (error != Error.Ok)
                {
                    GD.PrintErr("Fehler beim Laden des Bildes.");
                    return;
                }

                // Profile Image setting
                ImageTexture texture = new();
                texture.SetImage(image);
                _imageDisplay.Texture = texture;

                // RootNode Set Profile Image
                FdgFactory fdgFac = GetNode<FdgFactory>("FdgFactory");
                fdgFac.setProfileImg(body);
            }
            else
            {
                GD.PrintErr($"Fehler beim Laden des Bildes. HTTP-Statuscode: {responseCode}");
            }
        }


        private string normalizeGenre(string genre)
        {
            return genre.ToLowerInvariant().Replace("-", "").Replace(" ", "");
        }


        private async Task<bool> DetectAndLoadGenre(string genreStr, Genre parentGenre, SubGenre subgenre, FdgFactory fdgFac)
        {
            foreach (string subsubgenre in subgenre.Subsubgenres)
            {
                string subsubgenreName = normalizeGenre(subsubgenre);
                string genreStrName = normalizeGenre(genreStr);
                if (genreStrName == "mathrock")
                {
                    GD.Print($"{subsubgenreName} :: {genreStrName}");
                }

                if (subsubgenreName == genreStrName)
                {
                    // GD.Print("WHEEEE BIATCH");
                    fdgFac.AddGenre(parentGenre.Name, 500);
                    fdgFac.AddSubGenre(parentGenre.Name, subgenre.Name, 500);
                    fdgFac.AddSubGenre(subgenre.Name, subsubgenre, 500);
                    await Task.Delay(100);
                    return true;
                }
            }
            return false;
        }

        private async Task<bool> DetectAndLoadGenre(string genreStr, Genre genre, FdgFactory fdgFac)
        {
            foreach (SubGenre subGenre in genre.SubgenreList)
            {
                string subgenreName = normalizeGenre(subGenre.Name);
                string genreStrName = normalizeGenre(genreStr);
                if (genreStrName == "mathrock")
                {
                    GD.Print($"{subgenreName} :: {genreStrName}");
                }

                if (subgenreName == genreStrName)
                {
                    fdgFac.AddGenre(genre.Name, 500);
                    fdgFac.AddSubGenre(genre.Name, subGenre.Name, 500);
                    await Task.Delay(100);
                    return true;
                }
                else
                {
                    bool wasAdded = await DetectAndLoadGenre(genreStr, genre, subGenre, fdgFac);
                    if (wasAdded)
                    {
                        return true;
                    };
                }
            }
            return false;
        }

        private async Task DetectAndLoadGenre(string genreStr, GenreFile genreFile, FdgFactory fdgFac)
        {
            foreach (Genre genre in genreFile.Genrelist)
            {
                string genreName = normalizeGenre(genre.Name);
                string genreStrName = normalizeGenre(genreStr);
                if (genreStrName == "mathrock")
                {
                    GD.Print($"{genreName} :: {genreStrName}");
                }
                if (genreName == genreStrName)
                {
                    fdgFac.AddGenre(genre.Name, 500);
                    await Task.Delay(100);
                    return;
                }
                else
                {
                    bool wasAdded = await DetectAndLoadGenre(genreStr, genre, fdgFac);
                    if (wasAdded)
                    {
                        return;
                    };
                }
            }

            fdgFac.AddGenre(genreStr, 500);
            await Task.Delay(100);

        }

        private async void DisplayGenreHierarchy()
        {
            FdgFactory fdgFac = GetNode<FdgFactory>("FdgFactory");
            // deactivate pinchpan camera
            fdgFac.isLoadingGenre = true;


            if (SpotifyDataManager.Instance == null)
            {
                throw new Exception("SpotifyDataManager is not initialized oder kein AccessToken vorhanden.");
            }

            if (string.IsNullOrEmpty(SpotifyDataManager.Instance.AccessToken))
            {
                throw new Exception("there is no Spotify Access Token avaiable");
            }

            List<(string Genre, int Count)> topGenres = await SpotifyDataManager.Instance.GetTopGenres(SpotifyDataManager.Instance.AccessToken);

            // load genre hirarchy
            GenreFile genreFile = LoadJsonGenres("data/main.json");
            // error handling already happens in LoadJsonGenres

            foreach ((string Genre, int Count) topGenre in topGenres)
            {
                await DetectAndLoadGenre(topGenre.Genre, genreFile, fdgFac);
            }
            // activate pinchpan camera
            fdgFac.isLoadingGenre = false;

        }

        private string FindClosestMatch(string input, HashSet<string> genres, int maxDistance = 2)
        {
            string bestMatch = null;
            int bestDistance = maxDistance + 1; // Startwert etwas über der Grenze setzen

            foreach (var genre in genres)
            {
                int distance = LevenshteinDistance(input, genre);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestMatch = genre;
                }
            }
            return bestMatch;
        }

        private int LevenshteinDistance(string a, string b)
        {
            if (string.IsNullOrEmpty(a)) return b?.Length ?? 0;
            if (string.IsNullOrEmpty(b)) return a.Length;

            int[,] dp = new int[a.Length + 1, b.Length + 1];

            for (int i = 0; i <= a.Length; i++) dp[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) dp[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;
                    dp[i, j] = Math.Min(Math.Min(
                        dp[i - 1, j] + 1,   // Einfügen
                        dp[i, j - 1] + 1),  // Entfernen
                        dp[i - 1, j - 1] + cost); // Ersetzen
                }
            }

            return dp[a.Length, b.Length];
        }



        private static GenreFile LoadJsonGenres(string jsonFilePath)
        {
            try
            {
                if (!System.IO.File.Exists(jsonFilePath))
                {
                    GD.PrintErr("JSON-File is not avaiable: " + jsonFilePath);
                    throw new Exception($"JSON-File is not avaiable: {jsonFilePath}");
                }

                string jsonString = System.IO.File.ReadAllText(jsonFilePath);
                var genrefile = JsonConvert.DeserializeObject<GenreFile>(jsonString);

                if (genrefile == null || genrefile.Genrelist.Count == 0)
                {
                    GD.PrintErr("JSON-File is empty or couldn't be loaded");
                    throw new Exception($"JSON-File is empty or couldn't be loaded {jsonFilePath}");
                }

                foreach (Genre g in genrefile.Genrelist)
                {
                    GD.Print(g.Name);

                    foreach (SubGenre sg in g.SubgenreList)
                    {
                        GD.Print($"---{sg.Name}");
                    }
                }
                return genrefile;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error while loading JSON File: {ex.Message}");
                throw new Exception($"Error while loading JSON File: {ex.Message}");

            }

        }

    }

    public class GenreFile
    {
        public List<Genre> Genrelist { get; set; }
    }

    public class Genre
    {
        public string Name { get; set; }
        public List<SubGenre> SubgenreList { get; set; }
    }

    public class SubGenre
    {
        public string Name { get; set; }
        public List<string> Subsubgenres { get; set; }
    }

}
