using Unity.Entities;

namespace Missions.Missions.Authoring.BlobComponents
{
    public struct MissionBlob : IComponentData
    {
        public BlobAssetReference<BlobArray<Mission>> BlobAssetRef;
    }
}