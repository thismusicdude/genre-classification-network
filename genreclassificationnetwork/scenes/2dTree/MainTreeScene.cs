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


        private async void DisplayGenreHierarchy()
        {
            FdgFactory fdgFac = GetNode<FdgFactory>("FdgFactory");

            fdgFac.isLoadingGenre = true;


            if (SpotifyDataManager.Instance == null || string.IsNullOrEmpty(SpotifyDataManager.Instance.AccessToken))
            {
                GD.PrintErr("SpotifyDataManager ist nicht initialisiert oder kein AccessToken vorhanden.");
                return;
            }

            var genreHierarchy = await SpotifyDataManager.Instance.GetGenresWithSubgenres(SpotifyDataManager.Instance.AccessToken);
            var jsonGenres = LoadJsonGenres("spotify-genres.json");

            if (jsonGenres == null || jsonGenres.Count == 0)
            {
                GD.PrintErr("JSON-Genres konnten nicht geladen werden oder sind leer.");
                return;
            }


            foreach (var (mainGenre, subgenres) in genreHierarchy)
            {

                // Normalisierung: Entferne Leerzeichen, konvertiere zu Kleinbuchstaben
                string normalizedMainGenre = mainGenre.Trim().ToLowerInvariant();

                var genreCorrectionMap = new Dictionary<string, string>
                {
                    { "hop", "hip hop" },
                };

                if (genreCorrectionMap.ContainsKey(normalizedMainGenre))
                {
                    normalizedMainGenre = genreCorrectionMap[normalizedMainGenre];
                }


                string closestMatch = FindClosestMatch(normalizedMainGenre, jsonGenres);
                if (closestMatch == null)
                {
                    GD.PrintErr($"Genre \"{mainGenre}\" ({normalizedMainGenre}) nicht in JSON-Liste gefunden. Überspringe...");
                    continue;
                }
                else
                {
                    GD.PrintErr($"Genre \"{mainGenre}\" ({normalizedMainGenre}) nicht gefunden. Verwende stattdessen \"{closestMatch}\".");
                    normalizedMainGenre = closestMatch; // Verwende das ähnlichste gefundene Genre
                }

                GD.Print($"Hauptgenre: {mainGenre}");
                fdgFac.AddGenre(mainGenre, 100);
                await Task.Delay(10);

                foreach (var subgenre in subgenres.Distinct())
                {
                    if (!string.IsNullOrEmpty(subgenre))
                    {
                        fdgFac.AddSubGenre(mainGenre, subgenre, 100);
                        GD.Print($"  - {subgenre}");
                        await Task.Delay(100);
                    }
                }

            }

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



        private HashSet<string> LoadJsonGenres(string jsonFilePath)
        {
            try
            {
                if (!System.IO.File.Exists(jsonFilePath))
                {
                    GD.PrintErr("JSON-Datei existiert nicht: " + jsonFilePath);
                    return new HashSet<string>();
                }

                string jsonString = System.IO.File.ReadAllText(jsonFilePath);
                var genres = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Genre>>(jsonString);

                if (genres == null || genres.Count == 0)
                {
                    GD.PrintErr("JSON-Datei ist leer oder konnte nicht geladen werden.");
                    return new HashSet<string>();
                }

                var normalizedGenres = genres
                    .Select(g => g.Name.Trim().ToLowerInvariant())  // In Kleinbuchstaben umwandeln
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToHashSet();

                GD.Print("🔍 Geladene JSON-Genres (nach Normalisierung):");
                foreach (var genre in normalizedGenres.Take(10)) // Zeige die ersten 10 Genres zur Überprüfung
                {
                    GD.Print(genre);
                }

                return normalizedGenres;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Fehler beim Laden der JSON-Datei: {ex.Message}");
                return new HashSet<string>();
            }
        }

    }


    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }


}
