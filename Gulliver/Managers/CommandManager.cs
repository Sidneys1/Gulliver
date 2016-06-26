using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gulliver.Base;

namespace Gulliver.Managers {
    internal static class CommandManager {
        private static Dictionary<string, Type> _commands;
        public static IReadOnlyDictionary<string, Type> Commands => _commands;

        public static void Initialize() {
            _commands = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Command)))
                .SelectMany(t=> t.GetCustomAttribute<CommandAttribute>(false).Names.Select(n=>new Tuple<string, Type>(n, t)))
                .ToDictionary(t => t.Item1, t=>t.Item2);
        }
    }
}