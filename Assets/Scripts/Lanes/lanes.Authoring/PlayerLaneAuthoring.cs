using Lanes.lanes.Data;
using Unity.Entities;
using UnityEngine;

namespace Lanes.lanes.Authoring
{
    public class PlayerLaneAuthoring : MonoBehaviour
    {
        private class PlayerLaneBaker : Baker<PlayerLaneAuthoring>
        {
            public override void Bake(PlayerLaneAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new LaneMover { TargetLane = 1 }); // Start in the middle lane
            }
        }
    }
}