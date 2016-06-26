using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gulliver.Base;

namespace Gulliver.Managers {
    internal static class SettingsManager {
        private static readonly Dictionary<string, Setting> PrivateSettings = new Dictionary<string, Setting>();
        public static IReadOnlyDictionary<string, Setting> Settings => PrivateSettings;

        public static void Initialize() {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
                foreach (var propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Static).Where(p => Attribute.IsDefined((MemberInfo) p, typeof(SettingAttribute)))) {
                    var att = (SettingAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(SettingAttribute));
                    Func<object, object> validator = null;
                    if (att.ValidatorName != null)
                        validator =
                            (Func<object, object>)Delegate.CreateDelegate(typeof(Func<object, object>), type.GetMethod(att.ValidatorName));
                    PrivateSettings.Add(att.SettingName, new Setting(propertyInfo, att.DefaultValue, validator));
                }
            }
        }
    }
}