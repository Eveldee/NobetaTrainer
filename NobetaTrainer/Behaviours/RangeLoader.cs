using System;
using Il2CppInterop.Runtime.Injection;
using NobetaTrainer.Patches;
using Unity.Mathematics;
using UnityEngine;
using Math = Il2CppSystem.Math;
using Random = UnityEngine.Random;

namespace NobetaTrainer.Behaviours;

public class RangeLoader : MonoBehaviour
{
    static RangeLoader()
    {
        ClassInjector.RegisterTypeInIl2Cpp<RangeLoader>();
    }

    public static Vector3 TriggerPosition { get; set; }

    public GameObject Target { get; set; }
    public Vector3 TargetPosition { get; set; }
    public float Range { get; set; }

    private float _distance;
    private int _timer;

    private void Awake()
    {
        _timer += Random.Range(0, 60);
    }

    private void Update()
    {
        // if (Target == null)
        // {
        //     return;
        // }

        // Enable object only when trigger is closer than range
        if (++_timer != 59)
        {
            return;
        }

        _timer = 0;

        _distance = Mathf.Abs(TargetPosition.x - TriggerPosition.x) + Mathf.Abs(TargetPosition.y - TriggerPosition.y) + Mathf.Abs(TargetPosition.z - TriggerPosition.z);
        if (_distance <= Range)
        {
            if (!Target.activeSelf)
            {
                Target.SetActive(true);
            }
        }
        else
        {
            if (Target.activeSelf)
            {
                Target.SetActive(false);
            }
        }
    }
}