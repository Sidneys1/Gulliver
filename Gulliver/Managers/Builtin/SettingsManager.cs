using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gulliver.Base;

namespace Gulliver.Managers.Builtin {
    internal class SettingsManager : CliComponent {
        private static readonly Dictionary<string, Setting> _settings = new Dictionary<string, Setting>(StringComparer.OrdinalIgnoreCase);
        public static IReadOnlyDictionary<string, Setting> Settings => _settings;
        
        public override void ProcessType(Type type) {
            foreach (var setting in type.GetProperties(BindingFlags.Public | BindingFlags.Static)
                    .Where(p => Attribute.IsDefined((MemberInfo) p, typeof(SettingAttribute)))
                    .Select(p => new { prop = p, att = p.GetCustomAttribute<SettingAttribute>() })) {
                _settings.Add(setting.att.SettingName, new Setting(
                    setting.prop, 
                    setting.att.DefaultValue, 
                    setting.att.ValidatorName == null 
                        ? null 
                        : (Validator)Delegate.CreateDelegate(typeof(Validator), type.GetMethod(setting.att.ValidatorName))));
            }
        }
    }
}