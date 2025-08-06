using Data;
using Unity.Entities;

namespace Missions.Missions.Authoring.BlobComponents
{
    public struct GoalBlob : IComponentData
    {
        public BlobAssetReference<BlobArray<Goal>> BlobAssetRef;
    }
}