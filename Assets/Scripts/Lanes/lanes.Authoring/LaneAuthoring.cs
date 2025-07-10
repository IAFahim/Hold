using Lanes.Lines.Data;
using Unity.Entities;
using UnityEngine;

namespace Lanes.lanes.Authoring
{
    public class LaneAuthoring : MonoBehaviour
    {
        public int numberOfLanes = 3;
        public float laneWidth = 2f;

        private class LaneBaker : Baker<LaneAuthoring>
        {
            public override void Bake(LaneAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new LaneDefinition
                {
                    NumberOfLanes = authoring.numberOfLanes,
                    LaneWidth = authoring.laneWidth
                });
            }
        }
    }
}