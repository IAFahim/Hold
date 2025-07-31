using Follows.Follows.Data;
using Unity.Entities;
using UnityEngine;

namespace Follows.Follows.Authoring
{
    public class FollowEnableComponentAuthoring : MonoBehaviour
    {
        public bool enable;
        public float stoppingDistance = 1f;

        private class Baker : Baker<FollowEnableComponentAuthoring>
        {
            public override void Bake(FollowEnableComponentAuthoring componentAuthoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new FollowEnableComponent
                {
                    StoppingDistanceSq = componentAuthoring.stoppingDistance * componentAuthoring.stoppingDistance
                });
                SetComponentEnabled<FollowEnableComponent>(entity, componentAuthoring.enable);
            }
        }
    }
}