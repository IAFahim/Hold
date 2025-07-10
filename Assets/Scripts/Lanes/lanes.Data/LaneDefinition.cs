
using Unity.Entities;

namespace Lanes.Lines.Data
{
    public struct LaneDefinition : IComponentData
    {
        public int NumberOfLanes;
        public float LaneWidth;
    }
}
