using System;
using Gulliver.Base;

namespace Gulliver.Commands {
    [Command("exit", "Exits Gulliver")]
    internal class ExitCommand : Command {
        public override void Run(params string[] parameters) {
            Console.WriteLine("Exiting Gulliver...");
            GulliverCli.Running = false;
        }
    }
}
