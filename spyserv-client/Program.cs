using System.Diagnostics;

namespace spyserv
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args is null || args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            {
                Console.WriteLine("Unknown command");
                return;
            }

            switch (args[0].ToLower())
            {
                case "start":
                    StartApiAsDetachedProcess();
                    break;

                case "status":
                    Console.WriteLine("The application is running!");
                    break;

                case "help":
                    Console.WriteLine("Available commands:");
                    Console.WriteLine("  start  - Start the web application");
                    Console.WriteLine("  status - Show application status");
                    Console.WriteLine("  help   - Show available commands");
                    break;

                default:
                    Console.WriteLine($"Unknown command: {args[0]}");
                    break;
            }
        }
        private static void StartApiAsDetachedProcess()
        {
            var baseDirectory = AppContext.BaseDirectory;
            var folderPath = Path.Combine(baseDirectory, @"..\..\..\..\spyserv-c-api\bin\Release\net8.0\spyserv-c-api.exe");

            var fullPath = Path.GetFullPath(folderPath);

            var processInfo = new ProcessStartInfo
            {
                FileName = fullPath, 
                WorkingDirectory = Path.GetDirectoryName(fullPath),
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Minimized
            };

            try
            {
                Process.Start(processInfo);
                Console.WriteLine("API started as a detached process.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start API: {ex.Message}");
            }
        }
    }
}