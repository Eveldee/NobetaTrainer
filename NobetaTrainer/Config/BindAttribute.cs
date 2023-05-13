using System;

namespace NobetaTrainer.Config;

[AttributeUsage(AttributeTargets.Field)]
public class BindAttribute : Attribute
{
    public string Description { get; }
    public string Key { get; }
    public object DefaultValue { get;  }

    public BindAttribute(string description = default, string key = default, object defaultValue = default)
    {
        Description = description;
        Key = key;
        DefaultValue = defaultValue;
    }
}