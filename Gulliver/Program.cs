using ExtendedConsole;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Gulliver.Base;
using _Console = ExtendedConsole.ExtendedConsole;

namespace Gulliver {
    internal class Program {
        private static void Main(string[] args) {
            GulliverCli.Start(string.Join(" ", args));
        }
    }

    internal static class GulliverCli {
        public const string Branding = "Gulliver 0.1\x03B1";

        public static FormattedText ProjectName = "untitled project".DarkGray();

        public static FormattedString State = "?".Red();

        public static FormattedString Header => Branding.Green() + " [" + ProjectName + "] ( " + State + " )";

        public static string Caret = "> ";

        public static bool Running { get; set; } = true;

        static GulliverCli() {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.OutputEncoding = Encoding.GetEncoding(437);
            Console.WriteLine($"Initializing {Branding}...");
            var s = new Stopwatch();
            s.Start();
            Initialize();
            s.Stop();
            Console.WriteLine($"Initialization done in {s.Elapsed.TotalMilliseconds:N0}ms!\r\n");
        }

        private static void Initialize() {
            Console.WriteLine($"Loaded {Command.Commands.Count:N0} Commands...");
        }

        public static void Start(string initialCommand = null) {
            PrintMotd();

            var command = initialCommand;
            do {
                if (string.IsNullOrWhiteSpace(command))
                    command = GetCommandLine();
                ExecuteCommand(command);
                command = null;
            } while (Running);
        }

        private static void PrintMotd() {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(" ██████╗ ██╗   ██╗██╗     ██╗     ██╗██╗   ██╗███████╗██████╗ ");
            Console.WriteLine("██╔════╝ ██║   ██║██║     ██║     ██║██║   ██║██╔════╝██╔══██╗");
            Console.WriteLine("██║  ███╗██║   ██║██║     ██║     ██║██║   ██║█████╗  ██████╔╝");
            Console.WriteLine("██║   ██║██║   ██║██║     ██║     ██║╚██╗ ██╔╝██╔══╝  ██╔══██╗");
            Console.WriteLine("╚██████╔╝╚██████╔╝███████╗███████╗██║ ╚████╔╝ ███████╗██║  ██║");
            Console.WriteLine(" ╚═════╝  ╚═════╝ ╚══════╝╚══════╝╚═╝  ╚═══╝  ╚══════╝╚═╝  ╚═╝\r\n");

            var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Gulliver.motd.txt");
            if (stream == null) {
                WriteError("Could not load MOTD!");
                return;
            }

            var lines = ReadLines(stream, Encoding.UTF8).ToArray();

            var r = new Random();
            var line = $"  {lines[r.Next(lines.Length)]}\r\n\r\n";
            ("Message of the Day:\r\n".Green() + line.White()).Write();
            ("Tip: Use the `".DarkGray()+"help".Magenta()+"` command to see other available commands.\n\n".DarkGray()).Write();
        }

        public static IEnumerable<string> ReadLines(Stream stream, Encoding encoding) {
            using (stream)
            using (var reader = new StreamReader(stream, encoding)) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    yield return line;
                }
            }
        }

        public static void ExecuteCommand(string commandLine) {
            var parts = commandLine.Split(' ');
            var commandType =
                Command.Commands.Where(c => c.Key.Equals(parts[0], StringComparison.OrdinalIgnoreCase)).Select(c => c.Value).FirstOrDefault();
            if (commandType == null) {
                WriteError("Could not find that command.");
                return;
            }

            try {
                var command = (Command)Activator.CreateInstance(commandType);
                command.Run(parts.Skip(1).ToArray());
            } catch (Exception e) {
                WriteError(e.Message);
            }
        }

        private static void WriteError(string msg) {
            ("ERROR:\r\n".Red() + msg.Red(true) + "\r\n").Write();
        }

        private static string GetCommandLine() {
            Header.Write();
            Console.WriteLine();
            Console.Write(Caret);

            return Console.ReadLine();
        }
    }
}
