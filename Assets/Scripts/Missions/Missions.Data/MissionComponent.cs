using Unity.Entities;

namespace Missions.Missions.Data
{
    public struct MissionComponent : IComponentData
    {
        public BlobAssetReference<Mission> BlobAssetRef;
    }
}