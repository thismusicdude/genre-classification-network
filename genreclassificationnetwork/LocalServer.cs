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
		Console.WriteLine("Warte auf den Authentifizierungscode...");

		var context = await _listener.GetContextAsync();
		var query = context.Request.QueryString["code"];

		// Rückmeldung an den Benutzer im Browser
		var response = context.Response;
		string responseString = "Authentication erfolgreich! Sie können dieses Fenster jetzt schließen.";
		byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
		response.ContentLength64 = buffer.Length;
		await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
		response.Close();

		_listener.Stop();
		return query;
	}
}
