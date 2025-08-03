using System;
using BovineLabs.Reaction.Authoring.Conditions;
using Goals.Goals.Data.Component;
using Goals.Goals.Data.Enum;
using Goals.Goals.Data.Goals;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Goals.Goals.Authoring
{
    public class GoalFloatRangeBlobComponentAuthoring : MonoBehaviour
    {
        public GoalFloatRangeData[] goalTables;

        private class GoalTableFloatComponentBaker : Baker<GoalFloatRangeBlobComponentAuthoring>
        {
            public override void Bake(GoalFloatRangeBlobComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var blobAssetRef = GoalFloatRangeData.CreateBlobAssetRef(authoring.goalTables);
                AddComponent(entity, new GoalFloatRangeBlobComponent
                {
                    BlobAssetRef = blobAssetRef
                });
            }
        }

        [Serializable]
        public class GoalFloatRangeData
        {
            public ConditionSchemaObject conditionSchemaObject;
            public ERangeCheckType rangeCheckType;
            public float lowerLimit;
            public float upperLimit;

            public static BlobAssetReference<BlobArray<GoalRangeFloat>> CreateBlobAssetRef(GoalFloatRangeData[] datas)
            {
                using var builder = new BlobBuilder(Allocator.Temp);
                ref var blobArray = ref builder.ConstructRoot<BlobArray<GoalRangeFloat>>();
                var arrayBuilder = builder.Allocate(ref blobArray, datas.Length);
                for (int i = 0; i < datas.Length; i++)
                {
                    var data = datas[i];
                    arrayBuilder[i] = new GoalRangeFloat
                    {
                        Key = data.conditionSchemaObject.Key,
                        CheckType = data.rangeCheckType,
                        LowerLimit = data.lowerLimit,
                        UpperLimit = data.upperLimit
                    };
                }

                var blobAssetRef = builder.CreateBlobAssetReference<BlobArray<GoalRangeFloat>>(Allocator.Persistent);
                return blobAssetRef;
            }
        }
    }
}