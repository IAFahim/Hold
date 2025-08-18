using System;
using BovineLabs.Core.ObjectManagement;
using Missions.Missions.Authoring.Data;
using Missions.Missions.Authoring.Scriptable;
using Missions.Missions.Authoring.Settings;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Missions.Missions.Authoring.Schemas
{
    [CreateAssetMenu(menuName = "Hold/" + TypeString + "/Create " + FieldName, fileName = FieldName)]
    [
        AutoRef(
            nameof(MissionSettings), nameof(MissionSettings.schemas),
            FieldName, "Schemas/" + TypeString + "/" + FieldName
        )
    ]
    public class MissionSchema : BakingSchema<Mission>
    {
        private const string FieldName = nameof(MissionSchema);
        private const string TypeString = "Missions";

        public NameSchema nameSchema;
        public LocationSchema locationSchema;
        public GoalSchema[] goals = Array.Empty<GoalSchema>();

        public override Mission ToData()
        {
            return new Mission
            {
                id = (ushort)ID,
                locationId = (ushort)locationSchema.ID,
                nameId = (ushort)nameSchema.ID,
            };
        }

        public static BlobAssetReference<BlobArray<Mission>> ToAssetRef(MissionSchema[] missions)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var blobArray = ref builder.ConstructRoot<BlobArray<Mission>>();
            ToBlobArray(ref builder, ref blobArray, missions);
            var blobAssetRef = builder.CreateBlobAssetReference<BlobArray<Mission>>(Allocator.Persistent);
            builder.Dispose();
            return blobAssetRef;
        }

        private static void ToBlobArray(ref BlobBuilder builder, ref BlobArray<Mission> blobArray,
            MissionSchema[] schemas)
        {
            // Allocate space for all missions in the blob array
            var missions = builder.Allocate(ref blobArray, schemas.Length);

            for (int i = 0; i < schemas.Length; i++)
            {
                if (schemas[i] == null) continue;

                missions[i] = schemas[i].ToData();

                if (schemas[i].goals != null && schemas[i].goals.Length > 0)
                {
                    var goalsBlobArray = builder.Allocate(ref missions[i].Goals, schemas[i].goals.Length);

                    for (int j = 0; j < schemas[i].goals.Length; j++)
                    {
                        goalsBlobArray[j] = (ushort)schemas[i].goals[j].ID;
                    }
                }
                else
                {
                    builder.Allocate(ref missions[i].Goals, 0);
                }
            }
        }
    }
}