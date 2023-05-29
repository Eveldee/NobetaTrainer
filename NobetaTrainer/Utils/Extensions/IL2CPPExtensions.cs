using System;

namespace NobetaTrainer.Utils.Extensions;

public static class IL2CPPExtensions
{
    public static Guid ToManaged(this Il2CppSystem.Guid guid) => new(guid._a, guid._b, guid._c, guid._d, guid._e,
        guid._f, guid._g, guid._h, guid._i, guid._j, guid._k);

    public static Il2CppSystem.Guid ToUnmanaged(this Guid guid) => new(guid.ToByteArray());
}