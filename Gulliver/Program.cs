﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ExtendedConsole;
using Gulliver.Base;
using Gulliver.Managers;
using _Console = ExtendedConsole.ExtendedConsole;

namespace Gulliver {
    internal class Program {
        private static void Main(string[] args) {
            //var r = new Random();
            //using (var file = File.CreateText(@".\data.bin")) {
            //    var len = r.Next(512, 1024);
            //    file.WriteLine(len);
            //    for (var i = 0; i < len; i++) {
            //        var data = new byte[r.Next(8, 1024)];
            //        r.NextBytes(data);
            //        data[0] = 255;
            //        data[1] = data[2] = data[3] = 0;
            //        file.WriteLine(Convert.ToBase64String(data));
            //        data = null;
            //    }
            //    file.Flush();
            //}
            //return;
            //DataManager.SetData(@".\data.bin");

            GulliverCli.Start(string.Join(" ", args));
        }
    }

    internal static class DataManagerTranslator {
        private static readonly string[] Postfixes = { "B", "KB", "MB", "GB", "TB", "PB" };

        private static long _lastDataSize;
        private static string _lastDataSizeString = "0B";

        public static FormattedString Endianness
        {
            get
            {
                switch (DataManager.Endianness) {
                    case DataManager.EndianValue.Unknown:
                        return "Unknown Endianness".Yellow();
                    case DataManager.EndianValue.Little:
                        return "Little-Endian".Green();
                    case DataManager.EndianValue.LittleAuto:
                        return $"Little-Endian (Automatically Detected, {DataManager.EndianConfidence:P2})".Green(true);
                    case DataManager.EndianValue.Big:
                        return "Big-Endian".Green();
                    case DataManager.EndianValue.BigAuto:
                        return $"Little-Endian (Automatically Detected, {DataManager.EndianConfidence:P2})".Green(true);
                }
                return "ERROR".Red();
            }
        }

        public static FormattedString Representation
        {
            get
            {
                var bldr = " " + Endianness + " [ ]";
                return bldr;
            }
        }

        public static string DataSize
        {
            get
            {
                if (DataManager.DataSize != _lastDataSize) {
                    double ds = _lastDataSize = DataManager.DataSize;
                    var t = 0;
                    while (ds >= 1024 && t < Postfixes.Length) {
                        t++;
                        ds /= 1024.0;
                    }
                    _lastDataSizeString = $"{ds:#,0.##}{Postfixes[t]}";
                }
                return _lastDataSizeString;
            }
        }
    }

    internal static class GulliverCli {
        public const string Branding = "Gulliver 0.1\x03B1";

        public static FormattedText ProjectName = "untitled project".DarkGray();

        public static FormattedString State = "?".Red();

        public static FormattedString Caret = "\rGulliver".Blue() + "> ";

        static GulliverCli() {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.OutputEncoding = Encoding.GetEncoding(437);
            Console.WriteLine($"Initializing {Branding}...");
            var s = new Stopwatch();
            s.Start();
            Initialize();
            s.Stop();
            Console.WriteLine($"Initialization done in {s.Elapsed.TotalMilliseconds:N2}ms!");
            Console.WriteLine($"Loaded {CommandManager.Commands.Count:N0} Commands...");
            Console.WriteLine($"Loaded {SettingsManager.Settings.Count:N0} Settings...");
            Console.WriteLine($"Loaded {HelpManager.Topics.Count:N0} Help Topics...\n");
        }

        public static FormattedString Header =>
                $"{DataManager.Count} Entries, {DataManagerTranslator.DataSize}\n" + DataManagerTranslator.Endianness + "\n[ " + State + " ]";

        public static bool Running { get; set; } = true;

        private static void Initialize() {
            CommandManager.Initialize();
            SettingsManager.Initialize();
            HelpManager.Initialize();
        }

        public static void Start(string initialCommand = null) {
            PrintMotd();

            var command = initialCommand;
            do {
                Console.Title = $"{Branding} - {ProjectName}";
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

            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Gulliver.motd.txt");
            if (stream == null) {
                WriteError("Could not load MOTD!");
                return;
            }

            var lines = ReadLines(stream, Encoding.UTF8).ToArray();

            var rawline = $"  {lines[new Random().Next(lines.Length)]}\r\n\r\n";
            var line = FormattedString.Join("",
                rawline.Split('`').Select((s, i) => new FormattedString(i % 2 == 0 ? s.White() : s.Cyan())));
            ("Message of the Day:\r\n".Green() + line).Write();
            ("Tip: Use the ".DarkGray() + "help".Cyan() + " command to see other available commands.\n\n".DarkGray())
                .Write();
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
            var parts = commandLine.Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries);
            var commandType =
                CommandManager.Commands.Where(c => c.Key.Equals(parts[0], StringComparison.OrdinalIgnoreCase))
                    .Select(c => c.Value)
                    .FirstOrDefault();
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
            ("ERROR:\n".Red() + msg.Red(true) + "\n\n").Write();
        }

        private static string GetCommandLine() {
            Header.Write();
            Console.WriteLine();
            Caret.Write();
            return TabCompleteLine();
        }

        private static string TabCompleteLine() {
            var buffer = "";
            var insertionPos = 0;

            while (true) {
                Caret.Write();
                var parts = buffer.Split(' ');
                if (CommandManager.Commands.ContainsKey(parts[0])) {
                    (parts[0] + ' ').Cyan().Write();
                    string.Join(" ", parts.Skip(1)).Reset().Write();
                } else
                    Console.Write(buffer);
                Console.Write(' ');
                Console.CursorLeft = Caret.Length + insertionPos - 1;
                var key = Console.ReadKey(true);
                switch (key.Key) {
                    case ConsoleKey.Enter:
                        return buffer;
                    //case ConsoleKey.Home:
                    //    insertionPos = 0;
                    //    break;
                    //case ConsoleKey.End:
                    //    insertionPos = buffer.Length;
                    //    break;
                    //case ConsoleKey.LeftArrow:
                    //    if (insertionPos > 0)
                    //        --insertionPos;
                    //    break;
                    //case ConsoleKey.RightArrow:
                    //    if (insertionPos < buffer.Length)
                    //        ++insertionPos;
                    //    break;
                    case ConsoleKey.Backspace:
                        if (insertionPos > 0)
                            buffer = buffer.Remove(--insertionPos, 1);
                        break;
                    case ConsoleKey.Tab:
                        if (parts.Length == 1 && !CommandManager.Commands.ContainsKey(parts[0])) {
                            var options = CommandManager.Commands.Keys.Where(k => k.StartsWith(parts[0])).ToArray();
                            if (options.Length > 1) {
                                var len = parts[0].Length;
                                var max = options.Max(p => p.Length);
                                while (len < max && options.GroupBy(o => o.Length > len ? o[len] : '\0').Count() == 1)
                                    ++len;
                                if (len > 0) {
                                    buffer = options[0].Substring(0, len);
                                    insertionPos = buffer.Length;
                                }
                                Console.WriteLine($"\nOptions: {string.Join(", ", options.OrderBy(o=>o))}");
                            } else if (options.Length == 1) {
                                buffer = $"{options[0]} ";
                                insertionPos = buffer.Length;
                            }
                        } else if (parts.Length > 1 && CommandManager.Commands.ContainsKey(parts[0])) {
                            var type = CommandManager.Commands[parts[0]];
                            var cmdAtt = type.GetCustomAttribute<CommandAttribute>();
                            if (cmdAtt.TabCallback != null) {
                                var options = (string[])type.GetMethod(cmdAtt.TabCallback)
                                    .Invoke(null, BindingFlags.Static, null,
                                        new object[] {parts.Length - 2, parts.Last()},
                                        null);
                                if (options != null) {
                                    if (options.Length == 1) {
                                        parts[parts.Length - 1] = options[0];
                                        buffer = string.Join(" ", parts)+" ";
                                        insertionPos = buffer.Length;
                                    } else if (options.Length > 1) {
                                        var len = parts[parts.Length-1].Length;
                                        var max = options.Max(p => p.Length);
                                        while (len < max && options.GroupBy(o => o.Length > len ? o[len] : '\0').Count() == 1)
                                            ++len;
                                        if (len > 0) {
                                            parts[parts.Length - 1] = options[0].Substring(0, len);
                                            buffer = string.Join(" ", parts);
                                            insertionPos = buffer.Length;
                                        }
                                        Console.WriteLine($"\nOptions: {string.Join(", ", options.OrderBy(o=>o))}");
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        if (char.IsLetterOrDigit(key.KeyChar) || char.IsPunctuation(key.KeyChar) ||
                            char.IsSeparator(key.KeyChar)) {
                            buffer = buffer.Insert(insertionPos, key.KeyChar.ToString());
                            insertionPos++;
                        }
                        break;
                }
            }
        }
    }
}