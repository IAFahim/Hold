using System;
using BovineLabs.Core.ObjectManagement;
using BovineLabs.Essence.Authoring;
using BovineLabs.Reaction.Authoring.Conditions;
using Goals.Goals.Authoring.Schema;
using Goals.Goals.Data.Enum;
using Goals.Goals.Data.Goals;
using Goals.Goals.Data.GoalBlob;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Goals.Goals.Authoring
{
    public class GoalRangeFloatBlobComponentAuthoring : MonoBehaviour
    {
        public GoalRangeFloatSchema[] datas;

        private class Baker : Baker<GoalRangeFloatBlobComponentAuthoring>
        {
            public override void Bake(GoalRangeFloatBlobComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var blobAssetRef = GoalRangeFloatSchema.CreateBlobAssetRef(authoring.datas);
                AddComponent(entity, new GoalFloatRangeBlobComponent
                {
                    BlobAssetRef = blobAssetRef
                });
            }
        }
    }
}