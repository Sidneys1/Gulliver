using System;
using Gulliver.Base;

namespace Gulliver.Commands.Builtin {
    [Command("cls", "clear"), AutoHelpTopic(Topic.Commands, "Clears the Gulliver screen.", false, "cls", "clear")]
    internal class ClsCommand : Command {
        public override void Run(params string[] parameters) {
            if(parameters.Length != 0)
                throw new ArgumentException("Cls does not accept parameters.");
            Console.Clear();
        }
    }
}
