using System;
using BovineLabs.Reaction.Authoring.Conditions;
using Goals.Goals.Data.Enum;
using Goals.Goals.Data.Goals;
using Goals.Data.GoalBlob;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Goals.Goals.Authoring
{
    public class GoalIntBlobComponentAuthoring : MonoBehaviour
    {
        public GoalIntData[] datas;
        public class Baker : Baker<GoalIntBlobComponentAuthoring>
        {
            public override void Bake(GoalIntBlobComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity,new GoalIntBlobComponent
                {
                    BlobAssetRef = GoalIntData.CreateBlobAssetRef(authoring.datas)
                });
            }
        }
        
        [Serializable]
        public class GoalIntData
        {
            public ConditionSchemaObject conditionSchemaObject;
            public ECheckType checkType;
            public int targetValue;

            public static BlobAssetReference<BlobArray<GoalInt>> CreateBlobAssetRef(GoalIntData[] datas)
            {
                using var builder = new BlobBuilder(Allocator.Temp);
                ref var blobArray = ref builder.ConstructRoot<BlobArray<GoalInt>>();
                var arrayBuilder = builder.Allocate(ref blobArray, datas.Length);
                for (int i = 0; i < datas.Length; i++)
                {
                    var data = datas[i];
                    arrayBuilder[i] = new GoalInt
                    {
                        Key = data.conditionSchemaObject.Key,
                        CheckType = data.checkType,
                        TargetValue = data.targetValue,
                    };
                }

                var blobAssetRef = builder.CreateBlobAssetReference<BlobArray<GoalInt>>(Allocator.Persistent);
                return blobAssetRef;
            }
        }
    }
}