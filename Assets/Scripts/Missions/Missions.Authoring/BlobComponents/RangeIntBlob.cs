using Unity.Entities;

namespace Missions.Missions.Authoring.BlobComponents
{
    public struct RangeIntBlob : IComponentData
    {
        public BlobAssetReference<BlobArray<RangeInt>> BlobAssetRef;
    }
}