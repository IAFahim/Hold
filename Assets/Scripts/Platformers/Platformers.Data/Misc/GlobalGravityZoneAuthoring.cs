using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GlobalGravityZoneAuthoring : MonoBehaviour
{
    public float3 Gravity;

    private class Baker : Baker<GlobalGravityZoneAuthoring>
    {
        public override void Bake(GlobalGravityZoneAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new GlobalGravityZone { Gravity = authoring.Gravity });
        }
    }
}