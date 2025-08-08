using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Serialization;

[BurstCompile]
[Serializable]
public struct CarryingComponent : IComponentData
{
    public float currentWeight;
    public float capacity;
    public float minSpeedAtMaxLoad; // 0..1, minimum multiplier at full capacity

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float ComputeSpeedMultiplier()
    {
        if (capacity <= 0f)
        {
            return 1f;
        }

        float loadRatio = math.saturate(currentWeight / capacity);
        float multiplier = 1f - loadRatio;
        return math.max(multiplier, math.saturate(minSpeedAtMaxLoad));
    }
}
