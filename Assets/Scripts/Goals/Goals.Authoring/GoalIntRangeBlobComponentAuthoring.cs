using System;
using BovineLabs.Reaction.Authoring.Conditions;
using Goals.Goals.Data.Enum;
using Goals.Goals.Data.Goals;
using Goals.Goals.Data.GoalBlob;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Goals.Goals.Authoring
{
    public class GoalIntRangeBlobComponentAuthoring : MonoBehaviour
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
                    var data = datas[i];
                    arrayBuilder[i] = new GoalRangeInt
                    {
                        Key = data.conditionSchemaObject.Key,
                        CheckType = data.rangeCheckType,
                        LowerLimit = data.lowerLimit,
                        UpperLimit = data.upperLimit
                    };
                }

                var blobAssetRef = builder.CreateBlobAssetReference<BlobArray<GoalRangeInt>>(Allocator.Persistent);
                return blobAssetRef;
            }
        }
    }
}