using System;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace Kyvas.Dynamics.PluginBase
{
    public class PluginInput
    {
        public readonly object Value;
        public readonly bool HasValue;

        #region Constructors

        public PluginInput(object value, bool hasValue)
        {
            Value = value;
            HasValue = hasValue;
        }

        public PluginInput(IPluginExecutionContext context, string name)
        {
            if (context.InputParameters.TryGetValue(name, out var valueObj))
            {
                Value = valueObj;
                HasValue = true;
            }
            else
                HasValue = false;
        }

        #endregion
        #region Conversion

        // method for explicit conversion. works fine during compiling, doesn't work during runtime
        public static explicit operator PluginInput(PluginInput<object> pluginInput) =>
            new PluginInput(pluginInput.Value, pluginInput.HasValue);

        // backup method for conversion during runtime
        public PluginInput<T> ToGeneric<T>() => new PluginInput<T>(Value == null ? default(T) : (T)Value, HasValue);

        public object ToGeneric(Type type) =>
            typeof(PluginInput)
                .GetMethods()
                .Single(m =>
                    m.Name == nameof(ToGeneric)
                    && m.IsGenericMethodDefinition)
                .MakeGenericMethod(type)
                .Invoke(this, null);

        // to non generic static method is located here and not in generic class, because you wouldn't be able to call it otherwise
        public static PluginInput ToNonGeneric(object value) =>
            value.GetType().GetGenericTypeDefinition() == typeof(PluginInput<>)
                ? (PluginInput)
                // ReSharper disable once PossibleNullReferenceException
                typeof(PluginInput<>)
                    .MakeGenericType(value.GetType().GetGenericArguments())
                    .GetMethod(nameof(ToNonGeneric))
                    .Invoke(value, null)
                : throw new ArgumentException($"Value passed to {nameof(ToNonGeneric)} is of wrong type.");

        #endregion
    }

    public class PluginInput<T>
    {
        public readonly T Value;
        public readonly bool HasValue;

        #region Constructors

        public PluginInput(T value, bool hasValue)
        {
            Value = value;
            HasValue = hasValue;
        }
        public PluginInput(IPluginExecutionContext context, string name)
        {
            if (context.InputParameters.TryGetValue(name, out var valueObj) && valueObj is T tValue)
            {
                Value = tValue;
                HasValue = true;
            }
            else
                HasValue = false;
        }

        #endregion
        #region Conversion

        // method for explicit conversion. works fine during compiling, doesn't work during runtime
        public static explicit operator PluginInput<T>(PluginInput pluginInput) =>
            new PluginInput<T>(pluginInput.Value == null ? default(T) : (T)pluginInput.Value, pluginInput.HasValue);

        public PluginInput ToNonGeneric() => new PluginInput(Value, HasValue);

        #endregion
    }
}
