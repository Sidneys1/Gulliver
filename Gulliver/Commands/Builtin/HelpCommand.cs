using System;
using ExtendedConsole;
using Gulliver.Base;
using Gulliver.Managers;
using SimpleArgv;

namespace Gulliver.Commands.Builtin {
    [Command("help")]
    [HelpTopic("help", Topic.Commands, Summary, nameof(HelpTopic))]
    internal sealed class HelpCommand : Command {
        #region Help TopicGetter

        private const string Summary = "Prints information related to a variety of topics.";

        public static readonly Topic HelpTopic = new Topic("Help", Topic.Commands,
            Summary,
            Summary,
            subHeaders: new[] {
                new Topic("Usage", null,
                    "> " + "help [TOPIC] [-f/--full]".Cyan() + "",
                    subHeaders: new[] {
                        new Topic("Parameters", null,
                            " * " + "TOPIC".Cyan() + ": The title of the topic to print.\n" +
                            " * " + "-f/--full".Cyan() + ": Displays more information or topics.",
                            essential: false
                        )
                    }
                ),
                new Topic("Examples", null,
                    " * List all core topics:\n" +
                    "   > " + "help\n".Cyan() +
                    " * List every available topic:\n" +
                    "   > " + "help -f\n".Cyan() +
                    " * Show topic about the '" + "exit".Cyan() + "' command:\n" +
                    "   > " + "help exit\n".Cyan() +
                    " * Show this entire help topic:\n" +
                    "   > " + "help help --full".Cyan() +
                    "",
                    essential: false
                ),
            }
        );

        #endregion

        private readonly CommandLine _parser = new CommandLine(new[] { "-", "--" });
        
        public override void Run(params string[] parameters) {
           _parser.Parse(parameters);
            Console.WriteLine();

            var topic = _parser.GetValue(string.Empty, (string)null);
            if (topic == null) {
                HelpManager.PrintTopicTree(_parser.GetValue("--full", false));
                Console.WriteLine();
                return;
            }
            
            var rootTopic = HelpManager.Topics[topic];
            rootTopic.Print(_parser.GetValue("--full", false));
            Console.WriteLine();
        }
        
        public HelpCommand() {
            _parser.AddArgument(s => {
                    var full = string.Join(" ", s);
                    if (string.IsNullOrWhiteSpace(full)) return null;
                    if (!HelpManager.Topics.ContainsKey(full))
                        throw new ArgumentException($"Topic not found: '{full}'", "TOPIC");
                    return full;
                },
                string.Empty);
            _parser.AddArgument(s => true, "--full", "-f");
        }
    }
}