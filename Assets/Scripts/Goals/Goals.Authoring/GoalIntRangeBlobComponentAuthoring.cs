using System;
using BovineLabs.Reaction.Authoring.Conditions;
using Goals.Goals.Data;
using Goals.Goals.Data.Enum;
using Goals.Goals.Data.Goals;
using Unity.Collections;
using Unity.Entities;

namespace Goals.Goals.Authoring
{
    public class GoalIntRangeBlobComponentAuthoring : UnityEngine.MonoBehaviour
    {
        public GoalIntRangeData[] datas;

        private class Baker : Baker<GoalIntRangeBlobComponentAuthoring>
        {
            public override void Bake(GoalIntRangeBlobComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new GoalIntRangeBlobComponent
                {
                    BlobAssetRef = GoalIntRangeData.CreateBlobAssetRef(authoring.datas)
                });
            }
        }


        [Serializable]
        public class GoalIntRangeData
        {
            public ConditionSchemaObject conditionSchemaObject;
            public ERangeCheckType rangeCheckType;
            public int lowerLimit;
            public int upperLimit;

            public static BlobAssetReference<BlobArray<GoalRangeInt>> CreateBlobAssetRef(GoalIntRangeData[] datas)
            {
                using var builder = new BlobBuilder(Allocator.Temp);
                ref var blobArray = ref builder.ConstructRoot<BlobArray<GoalRangeInt>>();
                var arrayBuilder = builder.Allocate(ref blobArray, datas.Length);
                for (int i = 0; i < datas.Length; i++)
                {
                    var goalTableData = datas[i];
                    arrayBuilder[i] = new GoalRangeInt
                    {
                        Key = goalTableData.conditionSchemaObject.Key,
                        CheckType = goalTableData.rangeCheckType,
                        LowerLimit = goalTableData.lowerLimit,
                        UpperLimit = goalTableData.upperLimit
                    };
                }

                var blobAssetRef = builder.CreateBlobAssetReference<BlobArray<GoalRangeInt>>(Allocator.Persistent);
                return blobAssetRef;
            }
        }
    }
}