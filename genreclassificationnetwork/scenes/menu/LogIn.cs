using Godot;
using System;

namespace GenreTreeMenu {
	public partial class LogIn : Control
	{
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}


		public void _OnLogInButtonPressed()
		{
			// TODO: open spotify log in form in browser
			OpenWebsite("http://google.com");
		}

		public void OpenWebsite(string url)
		{
			if (OS.HasFeature("HTML5"))
			{
				throw new ArgumentException("Cant open Spotify Login");
			}
			else
			{
				OS.ShellOpen(url);
			}
		}
	}
}