using Hacks.Hacks.Data;
using Unity.Entities;
using UnityEngine;

namespace Hacks.Hacks.Authoring
{
    public class HackReadyAuthoring : MonoBehaviour
    {
        public float activeDuration = 1f;
        public bool enable;

        public class Baker : Baker<HackReadyAuthoring>
        {
            public override void Bake(HackReadyAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new HackReady()
                {
                    ActiveDuration = authoring.activeDuration
                });
                SetComponentEnabled<HackReady>(entity, authoring.enable);
            }
        }
    }
}