using System;

namespace NobetaTrainer.Config;

[AttributeUsage(AttributeTargets.Field)]
public class BindAttribute : Attribute
{
    public object DefaultValue { get;  }
    public string Key { get; }
    public string Description { get; }

    public BindAttribute(object defaultValue, string key = default, string description = default)
    {
        DefaultValue = defaultValue;
        Key = key;
        Description = description;
    }
}