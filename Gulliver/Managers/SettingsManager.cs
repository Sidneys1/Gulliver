using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gulliver.Base;

namespace Gulliver.Managers {
    internal class SettingsManager : CliComponent {
        private static Dictionary<string, Setting> _settings;
        public static IReadOnlyDictionary<string, Setting> Settings => _settings;

        public override void Initialize() {
            _settings = new Dictionary<string, Setting>(StringComparer.OrdinalIgnoreCase);
        }

        public override void ProcessType(Type type) {
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Static)
                                     .Where(p => Attribute.IsDefined(p, typeof(SettingAttribute)))) {
                var att = prop.GetCustomAttribute<SettingAttribute>();
                _settings.Add(att.SettingName, new Setting(
                    prop, 
                    att.DefaultValue, 
                    att.ValidatorName == null 
                        ? null 
                        : (Validator)Delegate.CreateDelegate(typeof(Validator), type.GetMethod(att.ValidatorName))));
            }
        }
    }
}