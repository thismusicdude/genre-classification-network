using Godot;

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
			SetProcess(false); // Node bleibt erhalten
		}
		else
		{
			QueueFree();
		}
	}
}
