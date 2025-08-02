using System;
using BovineLabs.Reaction.Authoring.Conditions;
using Goals.Goals.Data;
using Goals.Goals.Data.Enum;
using Goals.Goals.Data.GoalTable;
using Unity.Collections;
using Unity.Entities;

namespace Goals.Goals.Authoring
{
    public class GoalTableIntComponentAuthoring : UnityEngine.MonoBehaviour
    {
        public GoalTableIntData[] goalTables;

        private class Baker : Baker<GoalTableIntComponentAuthoring>
        {
            public override void Bake(GoalTableIntComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var blobAssetRef = GoalTableIntData.CreateBlobArray(authoring.goalTables);
                AddComponent(entity, new GoalTableIntComponent
                {
                    GoalTables = blobAssetRef
                });
            }
        }


        [Serializable]
        public class GoalTableIntData
        {
            public ConditionSchemaObject conditionSchemaObject;
            public ECheckType checkType;
            public int targetValue;
            public int upperLimit;

            public static BlobAssetReference<BlobArray<GoalTableInt>> CreateBlobArray(GoalTableIntData[] goalTableDatas)
            {
                using var builder = new BlobBuilder(Allocator.Temp);
                ref var blobArray = ref builder.ConstructRoot<BlobArray<GoalTableInt>>();
                var arrayBuilder = builder.Allocate(ref blobArray, goalTableDatas.Length);
                for (int i = 0; i < goalTableDatas.Length; i++)
                {
                    var goalTableData = goalTableDatas[i];
                    arrayBuilder[i] = new GoalTableInt
                    {
                        Key = goalTableData.conditionSchemaObject.Key,
                        CheckType = goalTableData.checkType,
                        TargetValue = goalTableData.targetValue,
                        UpperLimit = goalTableData.upperLimit
                    };
                }

                var blobAssetRef = builder.CreateBlobAssetReference<BlobArray<GoalTableInt>>(Allocator.Persistent);
                return blobAssetRef;
            }
        }

        
    }
}