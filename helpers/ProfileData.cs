namespace GenreClassificationNetwork
{
	public class ExternalUrls
	{
		public string Spotify { get; set; }
	}

	public class Followers
	{
		public string Href { get; set; }
		public int Total { get; set; }
	}

	public class ProfileImages
	{
		public int Height { get; set; }
		public string Url { get; set; }
		public int Width { get; set; }
	}

	public class SpotifyUserProfileResponse
	{
		public string Country { get; set; }
		public string Display_Name { get; set; }
		public string Email { get; set; }
		public ExternalUrls ExternalUrls { get; set; }
		public Followers Followers { get; set; }
		public string Href { get; set; }
		public string Id { get; set; }
		public System.Collections.Generic.List<ProfileImages> Images { get; set; }
		public string Product { get; set; }
		public string Type { get; set; }
		public string Uri { get; set; }
	}

}
