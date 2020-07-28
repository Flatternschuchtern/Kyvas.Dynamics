using System;
using System.Linq;

namespace Kyvas.Dynamics.PluginBase
{
    public class PluginOutput
    {
        public object Value;
        public bool HasValue;

        #region Constructors

        public PluginOutput(object value, bool hasValue)
        {
            Value = value;
            HasValue = hasValue;
        }
        public PluginOutput()
        {
            HasValue = false;
        }

        #endregion
        #region Conversion

        // method for explicit conversion. works fine during compiling, doesn't work during runtime
        public static explicit operator PluginOutput(PluginOutput<object> pluginOutput) =>
            new PluginOutput(pluginOutput.Value, pluginOutput.HasValue);

        // backup methods for conversion during runtime
        public PluginOutput<T> ToGeneric<T>() => new PluginOutput<T>(Value == null ? default(T) : (T)Value, HasValue);

        public object ToGeneric(Type type) =>
            typeof(PluginOutput)
                .GetMethods()
                .Single(m =>
                    m.Name == nameof(ToGeneric)
                    && m.IsGenericMethodDefinition)
                .MakeGenericMethod(type)
                .Invoke(this, null);

        // to non generic static method is located here and not in generic class, because you wouldn't be able to call it otherwise
        public static PluginOutput ToNonGeneric(object value) =>
            value.GetType().GetGenericTypeDefinition() == typeof(PluginOutput<>) 
                ? (PluginOutput)
                // ReSharper disable once PossibleNullReferenceException
                typeof(PluginOutput<>)
                    .MakeGenericType(value.GetType().GetGenericArguments())
                    .GetMethod(nameof(ToNonGeneric))
                    .Invoke(value, null)
                : throw new ArgumentException($"Value passed to {nameof(ToNonGeneric)} is of wrong type.");

        #endregion
    }

    public class PluginOutput<T>
    {
        private T _value;
        public bool HasValue { get; private set; }

        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                HasValue = _value?.Equals(default(T)) == false;
            }
        }

        #region Constructors

        public PluginOutput(T value, bool hasValue)
        {
            Value = value;
            HasValue = hasValue;
        }
        public PluginOutput()
        {
            HasValue = false;
        }

        #endregion
        #region Conversion

        // method for explicit conversion. works fine during compiling, doesn't work during runtime
        public static explicit operator PluginOutput<T>(PluginOutput pluginOutput) =>
            new PluginOutput<T>(pluginOutput.Value == null ? default(T) : (T)pluginOutput.Value, pluginOutput.HasValue);

        public PluginOutput ToNonGeneric() => new PluginOutput(Value, HasValue);

        #endregion

    }
}
