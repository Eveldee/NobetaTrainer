using System.Reflection;

namespace NobetaTrainer.Config;

public class AutoConfigStaticException : AutoConfigException
{
    public AutoConfigStaticException(FieldInfo fieldInfo) : base($"The field '{fieldInfo.Name}' in class '{fieldInfo.DeclaringType?.Name}' must be static when annotated with BindAttribute")
    {

    }
}