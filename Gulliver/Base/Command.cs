using System;

namespace Gulliver.Base {
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    internal class CommandAttribute : Attribute {
        public CommandAttribute(params string[] names) {
            Names = names;
        }

        public string[] Names { get; }
        public string TabCallback { get; set; }
    }

    internal abstract class Command {
        public abstract void Run(params string[] parameters);
    }
}
