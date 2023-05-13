using System.Reflection;

namespace NobetaTrainer.Config;

public class AutoConfigStaticException : AutoConfigException
{
    public AutoConfigStaticException(TypeInfo typeInfo) : base($"The class '{typeInfo.Name}' must be static when annotated with SectionAttribute")
    {

    }
}