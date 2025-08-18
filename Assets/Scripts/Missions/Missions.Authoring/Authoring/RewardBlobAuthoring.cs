using Missions.Missions.Authoring.BlobComponents;
using Missions.Missions.Authoring.Schemas;
using Missions.Missions.Authoring.Scriptable;
using Unity.Entities;
using UnityEngine;

namespace Missions.Missions.Authoring
{
    public class RewardBlobAuthoring : MonoBehaviour
    {
        public RewardSchema[] rewardSchemas;
        public class RewardBlobBaker : Baker<RewardBlobAuthoring>
        {
            public override void Bake(RewardBlobAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var blobAssetRef = BakingSchema<Reward>.ToAssetRef(authoring.rewardSchemas);
                AddComponent(entity, new RewardBlob
                {
                    BlobAssetRef = blobAssetRef
                });
            }
        }
    }
}
