using System;
using System.Net;
using System.Threading.Tasks;

public class LocalServer
{
	private HttpListener _listener;

	public LocalServer(string url)
	{
		_listener = new HttpListener();
		_listener.Prefixes.Add(url);
	}

	public async Task<string> WaitForCodeAsync()
	{
		_listener.Start();
		Console.WriteLine("Wait for the authentication code...");

		var context = await _listener.GetContextAsync();
		var query = context.Request.QueryString["code"];

		// Feedback to user in browser
		var response = context.Response;
		string responseString = "<h1>Authentication successful! You can now close this window.<h1>";
		byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
		response.ContentLength64 = buffer.Length;
		await response.OutputStream.WriteAsync(buffer);
		response.Close();

		_listener.Stop();
		return query;
	}
}
