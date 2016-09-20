using System;

namespace Gulliver.Base {
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CommandAttribute : Attribute {
        public CommandAttribute(params string[] names) { Names = names; }

        public string[] Names { get; }

        public ConsoleColor CommandColor { get; set; } = ConsoleColor.Cyan;
    }

    /// <summary>
    /// Marks the tab callback for a Command. Should return a string[]
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class TabCallbackAttribute : Attribute { }

    public abstract class Command {
        public abstract void Run(params string[] parameters);
    }
}
