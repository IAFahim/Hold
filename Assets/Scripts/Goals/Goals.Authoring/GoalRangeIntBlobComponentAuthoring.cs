using Goals.Goals.Authoring.Schema;
using Goals.Goals.Data.GoalBlob;
using Unity.Entities;
using UnityEngine;

namespace Goals.Goals.Authoring
{
    public class GoalRangeIntBlobComponentAuthoring : MonoBehaviour
    {
        public GoalIntRangeSchema[] datas;

        private class Baker : Baker<GoalRangeIntBlobComponentAuthoring>
        {
            public override void Bake(GoalRangeIntBlobComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new GoalIntRangeBlobComponent
                {
                    BlobAssetRef = GoalIntRangeSchema.CreateBlobAssetRef(authoring.datas)
                });
            }
        }
    }
}