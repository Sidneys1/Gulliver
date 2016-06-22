using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using SimpleArgv;

namespace Gulliver.Base {
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    internal class CommandAttribute : Attribute {
        public CommandAttribute(string name, string synopsis, string usage = "", string help = null) {
            Name = name;
            Synopsis = synopsis;
            Usage = usage;
            Help = help ?? synopsis;
        }

        public string Name { get; }
        public string Synopsis { get; }
        public string Help { get; }
        public string Usage { get; }
    }

    internal abstract class Command {
        public static Dictionary<string, Type> Commands;

        public readonly CommandLine Parser = new CommandLine(new[] { "-", "--" });

        static Command() {

            Commands = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Command)))
                .ToDictionary(t => t.GetCustomAttribute<CommandAttribute>(false).Name);
        }

        public virtual void Run(params string[] parameters) {
            Parser.Parse(parameters);
        }
    }
}
