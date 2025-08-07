using Unity.Entities;

namespace Missions.Missions.Authoring.BlobComponents
{
    public struct TimeBlob : IComponentData
    {
        public BlobAssetReference<BlobArray<TimeStruct>> BlobAssetRef;
    }
}