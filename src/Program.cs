namespace LocalWebServer {
	public static class Program {
		private const string NO_ARGUMENT = "!";

		public static void Main(string[] args) {
			var serverSettings = GetServerSettings(args);
			var server = new HttpServer(serverSettings);
			server.Launch();
		}

		private static string?[] GetArgs(string[] args, int count) {
			var fullArgs = new string?[count];
			for( int i = 0; i < args.Length; ++i )
				fullArgs[i] = args[i] == NO_ARGUMENT ? null : args[i];
			return fullArgs;
		}

		private static HttpServerSettings GetServerSettings(string[] args) {
			var fullArgs = GetArgs(args, 5);
			string? ipAddress = fullArgs[0];
			int? port = fullArgs[1] == null ? null : int.Parse(fullArgs[1]!);
			string? homePage = fullArgs[2];
			string? notFound = fullArgs[3];
			string? workingDirectory = fullArgs[4];
			return new(ipAddress, port, homePage, notFound, workingDirectory);
		}
	}
}