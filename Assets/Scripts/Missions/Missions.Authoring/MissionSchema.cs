using System;
using System.Collections;
using BovineLabs.Core.ObjectManagement;
using Goals.Goals.Authoring.Schema;
using Maps.Maps.Data;
using Missions.Missions.Data;
using Rewards.Rewards.Authoring.Schema;
using SchemaSettings.SchemaSettings.Authoring;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Missions.Missions.Authoring
{
    [CreateAssetMenu(menuName = "Hold/" + TypeString + "/Create " + FieldName, fileName = FieldName)]
    [
        AutoRef(
            nameof(MissionSettings), nameof(MissionSettings.schemas),
            FieldName, TypeString + "/" + FieldName
        )
    ]
    public class MissionSchema : BaseSchema<Mission>
    {
        private const string FieldName = nameof(MissionSchema);
        private const string TypeString = "Mission";

        public Segment segment;
        public EParcelType parcel;
        public GoalRangeIntSchema[] goalRangeInts = Array.Empty<GoalRangeIntSchema>();
        public GoalRangeFloatSchema[] goalRangeFloats = Array.Empty<GoalRangeFloatSchema>();
        public RewardGoalIntSchema[] rewardInts = Array.Empty<RewardGoalIntSchema>();
        public RewardGoalFloatSchema[] rewardFloats = Array.Empty<RewardGoalFloatSchema>();


        public BlobAssetReference<Mission> ToBlobAssetReference()
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var mission = ref builder.ConstructRoot<Mission>();

            mission.id = id;
            mission.segment = segment;
            mission.parcel = parcel;

            ToBlobArray(ref builder, ref mission.goalRangeIntIndexes, goalRangeInts);
            ToBlobArray(ref builder, ref mission.goalRangeFloatIndexes, goalRangeFloats);
            ToBlobArray(ref builder, ref mission.rewardIntIndexes, rewardInts);
            ToBlobArray(ref builder, ref mission.rewardFloatIndexes, rewardFloats);

            var blobAssetRef = builder.CreateBlobAssetReference<Mission>(Allocator.Persistent);
            builder.Dispose();
            return blobAssetRef;
        }


        public BlobBuilderArray<ushort> ToIndexBlob<T>(BlobBuilder builder, T list) where T : IList
        {
            ref var blobArray = ref builder.ConstructRoot<BlobArray<ushort>>();
            BlobBuilderArray<ushort> arrayBuilder = builder.Allocate(ref blobArray, list.Count);
            for (var i = 0; i < list.Count; i++) arrayBuilder[i] = (ushort)((IUID)list[i]).ID;
            return arrayBuilder;
        }
    }
}