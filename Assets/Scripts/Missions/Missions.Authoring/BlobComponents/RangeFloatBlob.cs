using Unity.Entities;

namespace Missions.Missions.Authoring.BlobComponents
{
    public struct RangeFloatBlob : IComponentData
    {
        public BlobAssetReference<BlobArray<RangeFloat>> BlobAssetRef;
    }
}