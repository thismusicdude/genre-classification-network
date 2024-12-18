using Godot;
using System;

namespace GenreClassificationNetwork
{
	public partial class Testscene : Node2D
	{
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
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
