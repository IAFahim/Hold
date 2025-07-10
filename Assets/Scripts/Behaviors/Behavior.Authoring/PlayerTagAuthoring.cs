using Behaviors.Behavior.Data;
using Unity.Entities;
using UnityEngine;

namespace Behaviors.Behavior.Authoring
{
    public class PlayerTagAuthoring : MonoBehaviour
    {
        public bool enable;
        public class PlayerTagBaker : Baker<PlayerTagAuthoring>
        {
            public override void Bake(PlayerTagAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PlayerTag>(entity);
                SetComponentEnabled<PlayerTag>(entity, authoring.enable);
            }
        }
    }
}