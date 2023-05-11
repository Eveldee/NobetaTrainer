using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;

namespace NobetaTrainer.Config;

public class AutoConfigManager
{
    private readonly ConfigFile _configFile;
    private readonly Dictionary<FieldInfo, ConfigEntryBase> _bindEntries;
    private bool _initDone;

    private readonly MethodInfo _genericBindMethod =
        typeof(ConfigFile).GetMethods().Single(method => method.Name == nameof(ConfigFile.Bind) && method.GetParameters().Length == 3);

    public AutoConfigManager(ConfigFile configFile)
    {
        _configFile = configFile;
        _bindEntries = new Dictionary<FieldInfo, ConfigEntryBase>();
    }

    private void Init()
    {
        // Find class that contains a section attribute
        var sectionClasses = Assembly.GetExecutingAssembly().DefinedTypes
            .Where(t => t is { IsClass: true, IsSealed: true, IsAbstract: true })
            .Where(t => Attribute.IsDefined(t, typeof(SectionAttribute)));

        foreach (var sectionClass in sectionClasses)
        {
            // Get section name
            var sectionName = sectionClass.GetCustomAttribute<SectionAttribute>()!.SectionName;

            // Get all fields that have ConfigBindAttribute
            var targetFields = sectionClass.DeclaredFields
                .Where(f => f.IsStatic && f.IsDefined(typeof(BindAttribute)));

            foreach (var targetField in targetFields)
            {
                // Get associate attribute
                var bindAttribute = targetField.GetCustomAttribute<BindAttribute>()!;

                // Fetch values and handle not specified cases
                var defaultValue = bindAttribute.DefaultValue;
                var key = bindAttribute.Key ?? targetField.Name;
                var description = bindAttribute.Description ?? "No description provided";
                var fieldType = targetField.FieldType;

                // Create and save bind for later use
                var configDefinition = new ConfigDefinition(sectionName, key);

                var bindMethod = _genericBindMethod.MakeGenericMethod(fieldType);
                var entryBase = (ConfigEntryBase)bindMethod.Invoke(_configFile, new[]
                {
                    configDefinition, defaultValue, new ConfigDescription(description)
                });

                _bindEntries[targetField] = entryBase;
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
        foreach (var (targetField, entry) in _bindEntries)
        {
            // Read value in config
            var value = entry.BoxedValue;

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
        foreach (var (targetField, entry) in _bindEntries)
        {
            // Read value from field
            entry.BoxedValue = GetValueFromField(targetField);
        }
    }

    private object GetValueFromField(FieldInfo fieldInfo)
    {
        if (!fieldInfo.IsStatic)
        {
            throw new AutoConfigStaticException(fieldInfo);
        }

        return fieldInfo.GetValue(null);
    }

    private void SetValueToField(FieldInfo fieldInfo, object value)
    {
        if (!fieldInfo.IsStatic)
        {
            throw new AutoConfigStaticException(fieldInfo);
        }

        fieldInfo.SetValue(null, value);
    }
}