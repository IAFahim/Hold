using Goals.Goals.Data.GoalTable;
using Unity.Entities;

namespace Goals.Goals.Data
{
    public struct GoalTableIntComponent : IComponentData
    {
        public BlobAssetReference<BlobArray<GoalTableInt>> GoalTables;
    }
}