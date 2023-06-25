using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace NobetaTrainer.Utils;

public static class UnityUtils
{
    public static GameObject FindGameObjectByNameForced(string name)
    {
        #if V1031
        var objects = Object.FindObjectsOfTypeAll(Il2CppType.Of<GameObject>());
        return objects.Cast<Il2CppReferenceArray<GameObject>>().FirstOrDefault(gameObject => gameObject.name == name);
        #else
        return Object.FindObjectsOfType<GameObject>(true)
            .FirstOrDefault(gameObject => gameObject.name == name);
        #endif
    }

    public static Il2CppArrayBase<TComponent> FindComponentsByTypeForced<TComponent>() where TComponent : Component
    {
        #if V1031
        var objects = Object.FindObjectsOfTypeAll(Il2CppType.Of<TComponent>());
        return objects.Cast<Il2CppReferenceArray<TComponent>>();
        #else
        return Object.FindObjectsOfType<TComponent>(true);
        #endif
    }

    public static TComponent FindComponentByNameForced<TComponent>(string name)
    {
        var gameObject = FindGameObjectByNameForced(name);

        return gameObject is not null ? gameObject.GetComponent<TComponent>() : default;
    }
}