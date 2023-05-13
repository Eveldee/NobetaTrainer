using System.Reflection;

namespace NobetaTrainer.Config;

public class BindUnsupportedTypeException : AutoConfigException
{
    public BindUnsupportedTypeException(FieldInfo fieldInfo) : base($"Unsupported bind type '{fieldInfo.FieldType}' for field '{fieldInfo.Name}' in '{fieldInfo.DeclaringType}', a converter is needed")
    {

    }
}