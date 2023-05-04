using System.Collections.Generic;

namespace NobetaTrainer.Utils;

public static class EnvironmentUtils
{
    public static ISet<string> LavaTrapNamePrefix { get; } = new HashSet<string>
    {
        "_Lava",
        "ChageFogLava",
        "ChangeFogLava",
        "Tarp_Lava",
        "Trap_Lava"
    };
}