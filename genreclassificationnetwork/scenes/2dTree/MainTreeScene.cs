using Godot;
using Newtonsoft.Json;
using System;

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

                // Textur aus dem Bild erstellen
                ImageTexture texture = new();
                texture.SetImage(image);

                // Textur in das TextureRect einfügen
                _imageDisplay.Texture = texture;

                GD.Print("Bild erfolgreich geladen!");

            }
            else
            {
                GD.PrintErr($"Fehler beim Laden des Bildes. HTTP-Statuscode: {responseCode}");
            }
        }

        public void OnButtonPressedEvent()
        {
            FdgFactory fdgFac = GetNode<FdgFactory>("FdgFactory");
            TextEdit textEdit = GetNode<TextEdit>("CanvasLayer/GenreNameEdit");

            fdgFac.AddGenre(textEdit.Text, 500);
        }


        public void OnButtonSubPressedEvent()
        {
            FdgFactory fdgFac = GetNode<FdgFactory>("FdgFactory");
            TextEdit subgenrename = GetNode<TextEdit>("CanvasLayer/SubGenreNameEdit");

            TextEdit parentname = GetNode<TextEdit>("CanvasLayer/ParentGenreNameEdit");

            fdgFac.AddSubGenre(parentname.Text, subgenrename.Text, 500);
        }
    }

}
