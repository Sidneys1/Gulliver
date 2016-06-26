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

    internal class Setting {
        private readonly PropertyInfo _prop;
        public object DefaultValue { get; }
        private readonly Func<object, object> _validator;

        public Setting(PropertyInfo property, object defaultValue, Func<object, object> validator = null) {
            _prop = property;
            DefaultValue = defaultValue;
            _validator = validator;
        }

        public void SetValue(object value) {
            if (_validator != null)
                value = _validator.Invoke(value);
            _prop.SetValue(null, value);
        }

        public void Reset() => _prop.SetValue(null, DefaultValue);

        public object GetValue() => _prop.GetValue(null);

        public T GetValue<T>() => (T)_prop.GetValue(null);
    }
}