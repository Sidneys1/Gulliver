using System;
using System.Linq;
using System.Reflection;
using ExtendedConsole;
using Gulliver.Base;

namespace Gulliver.Commands {
    [Command(
        "help",
        "Prints information related to a variety of topics.\r\nUse the `--full/-f` parameter to display more information (if available).",
        "TOPIC [--full/-f]")]
    internal sealed class HelpCommand : Command {
        public override void Run(params string[] parameters) {
            base.Run(parameters);

            var topic = Parser.GetValue(string.Empty, (Type)null);
            if (topic == null) {
                "Available Topics:\r\n\r\n".Green().Write();
                PrintTopicTree();
                Console.WriteLine();
                ("Use '".DarkGray() + "help TOPIC".Magenta() + "' to display information about a topic.\r\n\r\n".DarkGray()).Write();
                return;
            }

            var commandAttribute = topic.GetCustomAttribute<CommandAttribute>(false);
            var title = $"{commandAttribute.Name} Topic{(Parser.GetValue("-f", false) ? " (Full)" : string.Empty)}";
            Console.WriteLine(title);
            Console.WriteLine(new string('=', title.Length));
            Console.WriteLine();
            Console.WriteLine(Parser.GetValue("-f", false) ? commandAttribute.Help : commandAttribute.Synopsis);

            Console.WriteLine();
            Console.WriteLine("Usage");
            Console.WriteLine("=====");
            Console.WriteLine();
            Console.WriteLine($"> {commandAttribute.Name} {commandAttribute.Usage}\r\n");
        }

        private void PrintTopicTree() {
            var topics = Commands.Select(t => t.Key);
            "Commands\r\n========\r\n".Reset().Write();
            Console.WriteLine("  " + string.Join("\r\n  ", topics));
        }

        public HelpCommand() {
            Parser.AddArgument(
                s =>
                {
                    var full = string.Join(" ", s);
                    if (string.IsNullOrWhiteSpace(full)) return null;
                    var ret = Commands.Where(t => t.Key.Equals(full, StringComparison.OrdinalIgnoreCase)).Select(t => t.Value).FirstOrDefault();
                    if (ret == null)
                        throw new ArgumentException("Value cannot be null", "TOPIC");
                    return ret;
                },
                string.Empty);
            Parser.AddArgument(s => true, "--full", "-f");
        }
    }
}