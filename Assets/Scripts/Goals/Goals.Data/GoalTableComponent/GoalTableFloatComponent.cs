using Goals.Goals.Data.GoalTable;
using Unity.Entities;

namespace Goals.Goals.Data.Component
{
    public struct GoalTableFloatComponent : IComponentData
    {
        public BlobAssetReference<BlobArray<GoalTableFloat>> GoalTables;
    }
}