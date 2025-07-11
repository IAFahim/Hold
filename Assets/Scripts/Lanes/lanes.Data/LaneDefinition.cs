
using Unity.Entities;

namespace Lanes.lanes.Data
{
    public struct LaneDefinition : IComponentData
    {
        public float leftLane;
        public float rightLane;
    }
}
