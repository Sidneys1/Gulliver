using System;
using Gulliver.Base;

namespace Gulliver.Commands.Builtin {
    [Command("exit", CommandColor = ConsoleColor.Red), AutoHelpTopic(Topic.Commands, "Exits Gulliver", true, "exit")]
    internal class ExitCommand : Command {
        public override void Run(params string[] parameters) {
            if (parameters.Length != 0)
                throw new ArgumentException("Exit does not accept parameters.");
            Console.WriteLine("Exiting Gulliver...");
            GulliverCli.Running = false;
        }
    }
}
