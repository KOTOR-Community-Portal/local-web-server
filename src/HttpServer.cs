using Microsoft.AspNetCore.StaticFiles;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LocalWebServer {
	public class HttpServer {
		private readonly HttpServerSettings _settings;
		private readonly HttpListener _listener;

		private int _requestCount;
		private bool _isListening;

		private const string DEFAULT_FILE = "index.html";

		public HttpServer(HttpServerSettings settings) {
			if( !(settings.IPAddress == "localhost" || System.Net.IPAddress.TryParse(settings.IPAddress, out _)) )
				throw new ArgumentException("The specified argument does not contain a valid IP address.", nameof(settings));
			if( settings.Port < 0 )
				throw new ArgumentOutOfRangeException(nameof(settings.Port), "The specified argument does not contain a valid port.");
			_settings = settings;
			_listener = new();
			AddPrefixes(_listener, _settings.IPAddress, _settings.Port);
		}

		public string IPAddress => _settings.IPAddress;
		public int Port => _settings.Port;
		public string Prefix => GetPrefix(_settings.IPAddress, _settings.Port);
		public string WorkingDirectory => _settings.WorkingDirectory;
		public int RequestCount => _requestCount;
		public bool IsListening => _isListening;

		public void Launch() {
			_listener.Start();
			LogLaunch(WorkingDirectory, Prefix, Console.Out);
			var listenTask = Listen();
			listenTask.GetAwaiter().GetResult();
		}

		public void Shutdown() {
			LogShutdown(Console.Out);
			_listener.Close();
		}

		private static void AddPrefixes(HttpListener listener, string ipAddress, int port) {
			if( ipAddress == "localhost" || ipAddress == "127.0.0.1" ) {
				listener.Prefixes.Add(GetPrefix("localhost", port));
				listener.Prefixes.Add(GetPrefix("127.0.0.1", port));
			}
			else {
				listener.Prefixes.Add(GetPrefix(ipAddress, port));
			}
		}

		private static bool CheckPath(string path) {
			if( File.Exists(path) )
				return true;
			else if( !Path.HasExtension(path) )
				return false;
			else if( Path.GetExtension(path) == ".html" )
				return false;
			else
				return true;
		}

		private static string GetContentType(string path) {
			var provider = new FileExtensionContentTypeProvider();
			return provider.TryGetContentType(path, out var contentType)
				? contentType
				: "application/octet-stream";
		}

		private static string GetPath(string localPath, string workingDirectory) {
			return localPath.StartsWith('/') || localPath.StartsWith('\\')
				? Path.HasExtension(localPath)
					? Path.Combine(workingDirectory, localPath[1..])
					: Path.Combine(workingDirectory, localPath[1..], DEFAULT_FILE)
				: Path.HasExtension(localPath)
					? localPath
					: Path.Combine(localPath, DEFAULT_FILE);
		}

		private static string GetPrefix(string ipAddress, int port) {
			return $"http://{ipAddress}:{port}/";
		}

		private static void LogLaunch(string workingDirectory, string prefix, TextWriter writer) {
			writer.WriteLine($"Working directory is '{workingDirectory}'.");
			writer.WriteLine($"Listening at '{prefix}'.");
		}

		private static void LogRequest(HttpListenerContext context, int requestId, TextWriter writer) {
			var request= context.Request;
			writer.WriteLine($"Request {requestId}:");
			writer.Write("  "); writer.WriteLine(request.Url?.LocalPath ?? "");
			writer.Write("  "); writer.WriteLine(request.HttpMethod);
			writer.Write("  "); writer.WriteLine(request.UserHostName);
			writer.Write("  "); writer.WriteLine(request.UserAgent);
		}

		private static void LogResponse(HttpListenerContext context, TextWriter writer) {
			int statusCode = context.Response.StatusCode;
			writer.Write("  -> "); writer.Write(statusCode); writer.Write(" "); writer.WriteLine((HttpStatusCode)statusCode);
		}

		private static void LogShutdown(TextWriter writer) {
			writer.WriteLine("Shutting down.");
		}

		private string FindPath(HttpListenerContext context) {
			var url = context.Request.Url;
			string localPath = url == null || url.LocalPath == "/" || url.LocalPath == "\\"
				? _settings.HomePage
				: url.LocalPath;
			return GetPath(localPath, _settings.WorkingDirectory);
		}

		private string GetNotFoundPath() {
			return GetPath(_settings.NotFound, _settings.WorkingDirectory);
		}

		private async Task Listen() {
			_isListening = true;
			do {
				try {
					var context = await _listener.GetContextAsync();
					++_requestCount;
					LogRequest(context, _requestCount, Console.Out);
					await Respond(context);
					LogResponse(context, Console.Out);
				}
				catch( Exception ex ) {
					Console.WriteLine(ex.Message);
				}
				Console.WriteLine();
			} while( _isListening );
			Shutdown();
		}

		private async Task Respond(HttpListenerContext context) {
			string requestPath = FindPath(context);
			var response = context.Response;
			if( !CheckPath(requestPath) ) {
				requestPath = GetNotFoundPath();
				response.StatusCode = 404;
			}
			try {
				byte[] data = File.ReadAllBytes(requestPath);
				response.ContentType = GetContentType(requestPath);
				response.ContentEncoding = Encoding.UTF8;
				response.ContentLength64 = data.LongLength;
				await response.OutputStream.WriteAsync(data);
			}
			catch( Exception ) {
				response.StatusCode = 404;
			}
			response.Close();
		}
	}
}