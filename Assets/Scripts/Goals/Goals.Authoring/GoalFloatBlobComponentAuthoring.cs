using Goals.Goals.Authoring.Schema;
using Goals.Goals.Data.GoalBlob;
using Unity.Entities;
using UnityEngine;

namespace Goals.Goals.Authoring
{
    public class GoalFloatBlobComponentAuthoring : MonoBehaviour
    {
        public GoalFloatSchema[] datas;

        private class Baker : Baker<GoalFloatBlobComponentAuthoring>
        {
            public override void Bake(GoalFloatBlobComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new GoalFloatBlobComponent
                {
                    BlobAssetRef = GoalFloatSchema.CreateBlobAssetRef(authoring.datas)
                });
            }
        }
    }
}