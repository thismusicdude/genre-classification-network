using Godot;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GenreClassificationNetwork
{
    public class SubGenreStructure
    {
        public string Name;
        public string Parent;
        public SubGenreStructure(string parent, string name)
        {
            this.Name = name;
            this.Parent = parent;
        }

    }

    public class SubSubGenreStructure
    {
        public string Name;
        public string Parent;
        public string ParentParent;
        public SubSubGenreStructure(string parent, string parentparent, string name)

        {
            this.Name = name;
            this.Parent = parent;
            this.ParentParent = parentparent;
        }

    }
    public partial class MainTreeScene : Node2D
    {
        TextureRect _imageDisplay;

        SpotifyUserProfileResponse profileData;

        private List<string> m_genrelist = new();
        private List<SubGenreStructure> m_subgenrelist = new();
        private List<SubGenreStructure> m_subsubgenrelist = new();

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
                usernameLabel.Text = profileData.Display_Name;

                // Image Loading
                HttpRequest _httpRequest;
                _httpRequest = new HttpRequest();
                AddChild(_httpRequest);

                _httpRequest.RequestCompleted += OnRequestCompleted;

                string imageUrl = profileData.Images[0].Url;
                var result = _httpRequest.Request(imageUrl);

                if (result != Error.Ok)
                {
                    GD.PrintErr($"Error while retrieving Http Request: {result}");
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error while retrieving Spotify Data: {e.Message}");
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
                    GD.PrintErr("Error while loading image.");
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
                GD.PrintErr($"Error while retrieving HTTP Request for Profileimage. HTTP-Statuscode: {responseCode}");
            }
        }


        private string normalizeGenre(string genre)
        {
            return genre.ToLowerInvariant().Replace("-", "").Replace(" ", "");
        }


        private bool DetectAndLoadGenre(string genreStr, Genre parentGenre, SubGenre subgenre, FdgFactory fdgFac)
        {
            foreach (string subsubgenre in subgenre.Subsubgenres)
            {
                string subsubgenreName = normalizeGenre(subsubgenre);
                string genreStrName = normalizeGenre(genreStr);

                if (subsubgenreName == genreStrName)
                {
                    // fdgFac.AddGenre(parentGenre.Name, 500);
                    // fdgFac.AddSubGenre(parentGenre.Name, subgenre.Name, 500);
                    // fdgFac.AddSubGenre(subgenre.Name, subsubgenre, 500);
                    // await Task.Delay(100);

                    m_genrelist.Add(parentGenre.Name);
                    m_subgenrelist.Add(new(parentGenre.Name, subgenre.Name));
                    m_subsubgenrelist.Add(new(subgenre.Name, subsubgenre));
                    return true;
                }
            }
            return false;
        }

        private bool DetectAndLoadGenre(string genreStr, Genre genre, FdgFactory fdgFac)
        {
            foreach (SubGenre subGenre in genre.SubgenreList)
            {
                string subgenreName = normalizeGenre(subGenre.Name);
                string genreStrName = normalizeGenre(genreStr);

                if (subgenreName == genreStrName)
                {
                    // fdgFac.AddGenre(genre.Name, 500);
                    // fdgFac.AddSubGenre(genre.Name, subGenre.Name, 500);
                    // await Task.Delay(100);
                    m_genrelist.Add(genre.Name);
                    m_subgenrelist.Add(new(genre.Name, subGenre.Name));
                    return true;
                }
                else
                {
                    bool wasAdded = DetectAndLoadGenre(genreStr, genre, subGenre, fdgFac);
                    if (wasAdded)
                    {
                        return true;
                    };
                }
            }
            return false;
        }

        private void DetectAndLoadGenre(string genreStr, GenreFile genreFile, FdgFactory fdgFac)
        {
            foreach (Genre genre in genreFile.Genrelist)
            {
                string genreName = normalizeGenre(genre.Name);
                string genreStrName = normalizeGenre(genreStr);

                if (genreName == genreStrName)
                {
                    // fdgFac.AddGenre(genre.Name, 500);
                    // await Task.Delay(100);
                    m_genrelist.Add(genre.Name);
                    return;
                }
                else
                {
                    bool wasAdded = DetectAndLoadGenre(genreStr, genre, fdgFac);
                    if (wasAdded)
                    {
                        return;
                    };
                }
            }

            // fdgFac.AddGenre(genreStr, 500);
            m_genrelist.Add(genreStr);
            // await Task.Delay(100);

        }

        private async void DisplayGenreHierarchy()
        {
            FdgFactory fdgFac = GetNode<FdgFactory>("FdgFactory");
            // deactivate pinchpan camera
            fdgFac.isLoadingGenre = true;

            if (!SpotifyDataManager.Instance.isInitialized())
            {
                return;
            }

            List<(string Genre, int Count)> topGenres = await SpotifyDataManager.Instance.GetTopGenres(SpotifyDataManager.Instance.AccessToken);

            // load genre hirarchy
            GenreFile genreFile = LoadJsonGenres("data/main.json");
            // error handling already happens in LoadJsonGenres

            foreach ((string Genre, int Count) topGenre in topGenres)
            {
                DetectAndLoadGenre(topGenre.Genre, genreFile, fdgFac);
            }

            GD.Print("TEST");

            int counter = 0;

            var distinctGenre = m_genrelist.Distinct().ToList();
            fdgFac.maxMainGenres = distinctGenre.Count;

            foreach (string genre in distinctGenre)
            {
                fdgFac.AddGenre(genre, 0.01f, 1000);

                await Task.Delay(100);
            }
            await Task.Delay(1000);


            string first_genre = distinctGenre[0];
            string last_genre = distinctGenre.Last();

            const float length = 1000;
            const float k = 0.0f;
            foreach (string genre in distinctGenre)
            {
                if (counter == 0)
                {
                    counter += 1;
                    continue;
                }
                else
                {
                    fdgFac.CreateConnection(genre, distinctGenre[counter - 1], false, k, length);
                }
                counter += 1;
            }
            fdgFac.CreateConnection(first_genre, last_genre, false, k, length);
            await Task.Delay(1000);


            foreach (SubGenreStructure subgenre in m_subgenrelist)
            {
                // fdgFac.AddGenre(subgenre.Parent, 500);
                fdgFac.AddSubGenre(subgenre.Parent, subgenre.Name, 0.002f, 450);
                await Task.Delay(100);

            }
            await Task.Delay(1000);


            foreach (SubGenreStructure subsubgenre in m_subsubgenrelist)
            {
                // fdgFac.AddGenre(subsubgenre.ParentParent, 500);
                // fdgFac.AddSubGenre(subsubgenre.ParentParent, subsubgenre.Parent, 500);
                fdgFac.AddSubGenre(subsubgenre.Parent, subsubgenre.Name, 0.002f, 300);
                await Task.Delay(100);
            }

            // activate pinchpan camera
            fdgFac.isLoadingGenre = false;

        }

        private string FindClosestMatch(string input, HashSet<string> genres, int maxDistance = 2)
        {
            string bestMatch = null;
            int bestDistance = maxDistance + 1; // Set starting value slightly above the limit

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
                        dp[i - 1, j] + 1,   // Insert
                        dp[i, j - 1] + 1),  // Remove
                        dp[i - 1, j - 1] + cost); // Replace
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
