using System;
using System.Collections.Generic;
using System.Reflection;
using Gulliver.Base;

namespace Gulliver.Managers {
    internal class CommandManager : CliComponent {
        private static Dictionary<string, Type> _commands;
        public static IReadOnlyDictionary<string, Type> Commands => _commands;

        public override void Initialize() {
            _commands = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        }

        public override void ProcessType(Type type) {
            if (!type.IsSubclassOf(typeof(Command))) return;
            foreach (var item in type.GetCustomAttribute<CommandAttribute>(false).Names)
                _commands.Add(item, type);
        }
    }
}