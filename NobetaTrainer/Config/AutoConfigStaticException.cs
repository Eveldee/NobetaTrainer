using System.Reflection;

namespace NobetaTrainer.Config;

public class AutoConfigStaticException : AutoConfigException
{
    public AutoConfigStaticException(FieldInfo fieldInfo) : base($"The field '{fieldInfo.Name}' in '{fieldInfo.DeclaringType}' must be static")
    {

    }
}