using Move.Move.Data;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Move.Move.Authoring
{
    public class MoveStartComponentAuthoring : MonoBehaviour
    {
        public bool enable;
        public float3 value;

        public class MoveStartComponentBaker : Baker<MoveStartComponentAuthoring>
        {
            public override void Bake(MoveStartComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new MoveStartComponent { Value = authoring.value });
                SetComponentEnabled<MoveStartComponent>(entity, authoring.enable);
            }
        }
    }
}