using Goals.Goals.Authoring.Schema;
using Goals.Goals.Data.GoalBlob;
using Unity.Entities;
using UnityEngine;

namespace Goals.Goals.Authoring
{
    public class GoalRangeFloatBlobComponentAuthoring : MonoBehaviour
    {
        public GoalRangeFloatSchema[] datas;

        private class Baker : Baker<GoalRangeFloatBlobComponentAuthoring>
        {
            public override void Bake(GoalRangeFloatBlobComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var blobAssetRef = GoalRangeFloatSchema.ToBlobAssetRef(authoring.datas);
                AddComponent(entity, new GoalRangeFloatBlobComponent
                {
                    BlobAssetRef = blobAssetRef
                });
            }
        }
    }
}