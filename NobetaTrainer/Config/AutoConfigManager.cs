using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using BepInEx.Configuration;

namespace NobetaTrainer.Config;

public class AutoConfigManager
{
    private record BindEntry(ConfigEntryBase Entry, IBindConverter BindConverter = null);

    private readonly ConfigFile _configFile;
    private readonly Dictionary<FieldInfo, BindEntry> _bindEntries;
    private bool _initDone;

    private readonly MethodInfo _genericBindMethod =
        typeof(ConfigFile).GetMethods().Single(method => method.Name == nameof(ConfigFile.Bind) && method.GetParameters().Length == 3);

    private readonly ISet<Type> _supportedTypes = new HashSet<Type>
    {
        typeof(string),
        typeof(bool),
        typeof(byte),
        typeof(sbyte),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(float),
        typeof(double),
        typeof(decimal),
        typeof(Enum)
    };

    private readonly Dictionary<Type, IBindConverter> _converters = new()
    {
        { typeof(Vector3), new Vector3BindConverter() }
    };

    public AutoConfigManager(ConfigFile configFile)
    {
        _configFile = configFile;
        _bindEntries = new Dictionary<FieldInfo, BindEntry>();
    }

    private void Init()
    {
        // Find class that contains a section attribute
        var sectionClasses = Assembly.GetExecutingAssembly().DefinedTypes
            .Where(t => t.IsClass)
            .Where(t => Attribute.IsDefined(t, typeof(SectionAttribute)));

        foreach (var sectionClass in sectionClasses)
        {
            // Get section name
            var sectionName = sectionClass.GetCustomAttribute<SectionAttribute>()!.SectionName;

            // Get all fields that have ConfigBindAttribute
            var targetFields = sectionClass.DeclaredFields
                .Where(field => field.IsDefined(typeof(BindAttribute)));

            foreach (var targetField in targetFields)
            {
                // Check that field is static
                if (!targetField.IsStatic)
                {
                    throw new AutoConfigStaticException(targetField);
                }

                // Get associate attribute
                var bindAttribute = targetField.GetCustomAttribute<BindAttribute>()!;

                // Fetch values and handle not specified cases
                var defaultValue = bindAttribute.DefaultValue ?? GetValueFromField(targetField);
                var key = bindAttribute.Key ?? targetField.Name;
                var description = bindAttribute.Description ?? "No description provided";
                var fieldType = targetField.FieldType;

                // Create and save bind for later use
                var configDefinition = new ConfigDefinition(sectionName, key);

                // Check if field type is a supported type
                IBindConverter bindConverter = null;
                if (!_supportedTypes.Contains(fieldType))
                {
                    // Needs a converter
                    if (!_converters.TryGetValue(fieldType, out bindConverter))
                    {
                        throw new BindUnsupportedTypeException(targetField);
                    }
                }

                ConfigEntryBase entryBase;

                // Store a a string if a converter is used
                if (bindConverter is not null)
                {
                    var bindMethod = _genericBindMethod.MakeGenericMethod(typeof(string));
                    entryBase = (ConfigEntryBase)bindMethod.Invoke(_configFile, new[]
                    {
                        configDefinition, (object)bindConverter.Serialize(defaultValue), new ConfigDescription(description)
                    });
                }
                else
                {
                    var bindMethod = _genericBindMethod.MakeGenericMethod(fieldType);
                    entryBase = (ConfigEntryBase)bindMethod.Invoke(_configFile, new[]
                    {
                        configDefinition, defaultValue, new ConfigDescription(description)
                    });
                }

                _bindEntries[targetField] = new BindEntry(entryBase, bindConverter);
            }
        }

        _configFile.Save();
        _initDone = true;
    }

    public void LoadValuesToFields()
    {
        if (!_initDone)
        {
            Init();
        }

        // Set value from config to target fields
        foreach (var (targetField, bindEntry) in _bindEntries)
        {
            // Read value in config
            object value;
            if (bindEntry.BindConverter is { } bindConverter)
            {
                value = bindConverter.Deserialize((string)bindEntry.Entry.BoxedValue);
            }
            else
            {
                value = bindEntry.Entry.BoxedValue;
            }

            SetValueToField(targetField, value);
        }
    }

    public void FetchValuesFromFields()
    {
        if (!_initDone)
        {
            Init();
        }

        // Set value from config to target fields
        foreach (var (targetField, bindEntry) in _bindEntries)
        {
            // Read value from field
            object value = GetValueFromField(targetField);
            if (bindEntry.BindConverter is { } bindConverter)
            {
                value = bindConverter.Serialize(value);
            }

            bindEntry.Entry.BoxedValue = value;
        }
    }

    private object GetValueFromField(FieldInfo fieldInfo)
    {
        return fieldInfo.GetValue(null);
    }

    private void SetValueToField(FieldInfo fieldInfo, object value)
    {
        fieldInfo.SetValue(null, value);
    }
}