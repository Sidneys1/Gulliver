using System;
using System.Collections.Generic;
using System.Reflection;
using Gulliver.Base;

namespace Gulliver.Managers.Builtin {
    internal class CommandManager : CliComponent {
        private static readonly Dictionary<string, Type> _commands = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        public static IReadOnlyDictionary<string, Type> Commands => _commands;
        
        public override void ProcessType(Type type) {
            if (!type.IsSubclassOf(typeof(Command))) return;
            foreach (var item in type.GetCustomAttribute<CommandAttribute>(false).Names)
                _commands.Add(item, type);
        }
    }
}