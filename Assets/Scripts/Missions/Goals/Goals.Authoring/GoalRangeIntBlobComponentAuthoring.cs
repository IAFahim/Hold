using Goals.Goals.Authoring.Schema;
using Goals.Goals.Data.GoalBlob;
using Unity.Entities;
using UnityEngine;

namespace Goals.Goals.Authoring
{
    public class GoalRangeIntBlobComponentAuthoring : MonoBehaviour
    {
        public GoalRangeIntSchema[] datas;

        private class Baker : Baker<GoalRangeIntBlobComponentAuthoring>
        {
            public override void Bake(GoalRangeIntBlobComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new GoalRangeIntBlobComponent
                {
                    BlobAssetRef = GoalRangeIntSchema.ToBlobAssetRef(authoring.datas)
                });
            }
        }
    }
}