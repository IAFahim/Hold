using Goals.Data.GoalBlob;
using Goals.Goals.Authoring.Schema;
using Unity.Entities;
using UnityEngine;

namespace Goals.Goals.Authoring
{
    public class GoalIntBlobComponentAuthoring : MonoBehaviour
    {
        public GoalIntSchema[] datas;

        public class Baker : Baker<GoalIntBlobComponentAuthoring>
        {
            public override void Bake(GoalIntBlobComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new GoalIntBlobComponent
                {
                    BlobAssetRef = GoalIntSchema.CreateBlobAssetRef(authoring.datas)
                });
            }
        }
    }
}