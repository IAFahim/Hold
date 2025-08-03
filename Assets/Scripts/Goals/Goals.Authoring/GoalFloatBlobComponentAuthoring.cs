using System;
using BovineLabs.Reaction.Authoring.Conditions;
using Goals.Goals.Data.Enum;
using Goals.Goals.Data.Goals;
using Goals.Goals.Data.GoalTableComponent;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Goals.Goals.Authoring
{
    public class GoalFloatBlobComponentAuthoring : MonoBehaviour
    {
        public GoalFloatData[] datas;

        private class Baker : Baker<GoalFloatBlobComponentAuthoring>
        {
            public override void Bake(GoalFloatBlobComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new GoalFloatBlobComponent
                {
                    BlobAssetRef = GoalFloatData.CreateBlobAssetRef(authoring.datas)
                });
            }
        }


        [Serializable]
        public class GoalFloatData
        {
            public ConditionSchemaObject conditionSchemaObject;
            public ECheckType checkType;
            public float targetValue;

            public static BlobAssetReference<BlobArray<GoalFloat>> CreateBlobAssetRef(GoalFloatData[] datas)
            {
                using var builder = new BlobBuilder(Allocator.Temp);
                ref var blobArray = ref builder.ConstructRoot<BlobArray<GoalFloat>>();
                var arrayBuilder = builder.Allocate(ref blobArray, datas.Length);
                for (int i = 0; i < datas.Length; i++)
                {
                    var data = datas[i];
                    arrayBuilder[i] = new GoalFloat
                    {
                        Key = data.conditionSchemaObject.Key,
                        CheckType = data.checkType,
                        TargetValue = data.targetValue
                    };
                }

                var blobAssetRef = builder.CreateBlobAssetReference<BlobArray<GoalFloat>>(Allocator.Persistent);
                return blobAssetRef;
            }
        }
    }
}