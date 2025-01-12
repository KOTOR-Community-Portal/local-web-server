using System.IO;

namespace LocalWebServer {
	public sealed class HttpServerSettings {
		private const string DEFAULT_IP_ADDRESS = "localhost";
		private const int DEFAULT_PORT = 8000;
		private const string DEFAULT_HOME_PAGE = "index.html";
		private const string DEFAULT_NOT_FOUND = "not_found.html";
		
		private readonly string _ipAddress;
		private readonly int _port;
		private readonly string _homePage;
		private readonly string _notFound;
		private string _workingDiretory;

		public HttpServerSettings(
			string? ipAddress = null,
			int? port = null,
			string? homePage = null,
			string? notFound = null,
			string? workingDiretory = null
		) {
			_ipAddress = ipAddress ?? DEFAULT_IP_ADDRESS;
			_port = port ?? DEFAULT_PORT;
			_homePage = homePage ?? DEFAULT_HOME_PAGE;
			_notFound = notFound ?? DEFAULT_NOT_FOUND;
			_workingDiretory = workingDiretory ?? Directory.GetCurrentDirectory();
		}

		public string IPAddress => _ipAddress;
		public int Port => _port;
		public string HomePage => _homePage;
		public string NotFound => _notFound;
		public string WorkingDirectory => _workingDiretory;
	}
}