using System;
using System.Reflection;

namespace Gulliver.Base {
    [AttributeUsage(AttributeTargets.Property)]
    internal class SettingAttribute : Attribute {
        public SettingAttribute(string settingName, Type valueType, object defaultValue = null, string validatorName = null) {
            if (defaultValue == null && valueType.IsValueType) defaultValue = Activator.CreateInstance(valueType);
            SettingName = settingName;
            DefaultValue = defaultValue;
            ValidatorName = validatorName;
        }

        public string SettingName { get; }
        public object DefaultValue { get; }
        public string ValidatorName { get; }
    }

    public delegate object Validator(string input);

    internal class Setting {
        private readonly PropertyInfo _prop;
        public readonly object DefaultValue;
        public readonly Validator Validator;

        public Setting(PropertyInfo property, object defaultValue, Validator validator = null) {
            _prop = property;
            DefaultValue = defaultValue;
            Validator = validator;
        }

        public void SetValue(string value) {
            object set=value;
            if (Validator != null)
                set = Validator(value);
            _prop.SetValue(null, set);
        }

        public void Reset() => _prop.SetValue(null, DefaultValue);

        public object GetValue() => _prop.GetValue(null);

        public T GetValue<T>() => (T)_prop.GetValue(null);

        public Type SettingType => _prop.PropertyType;
    }
}