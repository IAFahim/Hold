using System;
using BovineLabs.Reaction.Authoring.Conditions;
using Goals.Goals.Data.Component;
using Goals.Goals.Data.Enum;
using Goals.Goals.Data.GoalTable;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Goals.Goals.Authoring
{
    public class GoalTableFloatComponentAuthoring : MonoBehaviour
    {
        public GoalTableFloatData[] goalTables;
        public class GoalTableFloatComponentBaker : Baker<GoalTableFloatComponentAuthoring>
        {
            public override void Bake(GoalTableFloatComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var blobAssetRef = GoalTableFloatData.CreateBlobArray(authoring.goalTables);
                AddComponent(entity, new GoalTableFloatComponent
                {
                    GoalTables = blobAssetRef
                });
            }
        }

        [Serializable]
        public class GoalTableFloatData
        {
            public ConditionSchemaObject conditionSchemaObject;
            public ECheckType checkType;
            public float targetValue;
            public float upperLimit;

            public static BlobAssetReference<BlobArray<GoalTableFloat>> CreateBlobArray(
                GoalTableFloatData[] goalTableDatas)
            {
                using var builder = new BlobBuilder(Allocator.Temp);
                ref var blobArray = ref builder.ConstructRoot<BlobArray<GoalTableFloat>>();
                var arrayBuilder = builder.Allocate(ref blobArray, goalTableDatas.Length);
                for (int i = 0; i < goalTableDatas.Length; i++)
                {
                    var goalTableData = goalTableDatas[i];
                    arrayBuilder[i] = new GoalTableFloat
                    {
                        Key = goalTableData.conditionSchemaObject.Key,
                        CheckType = goalTableData.checkType,
                        TargetValue = goalTableData.targetValue,
                        UpperLimit = goalTableData.upperLimit
                    };
                }

                var blobAssetRef = builder.CreateBlobAssetReference<BlobArray<GoalTableFloat>>(Allocator.Persistent);
                return blobAssetRef;
            }
        }
    }
}