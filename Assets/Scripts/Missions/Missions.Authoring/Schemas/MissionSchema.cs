using System;
using BovineLabs.Core.ObjectManagement;
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
            FieldName, TypeString + "/" + FieldName
        )
    ]
    public class MissionSchema : BaseSchema
    {
        private const string FieldName = nameof(MissionSchema);
        private const string TypeString = "Missions";
        
        public NameSchema nameSchema;
        public StationSchema stationSchema;
        public GoalSchema[] goals = Array.Empty<GoalSchema>();

        public static BlobAssetReference<BlobArray<Mission>> ToAssetRef(MissionSchema[] missions)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var blobArray = ref builder.ConstructRoot<BlobArray<Mission>>();
            ToBlobArray(ref builder, ref blobArray, missions);
            var blobAssetRef = builder.CreateBlobAssetReference<BlobArray<Mission>>(Allocator.Persistent);
            builder.Dispose();
            return blobAssetRef;
        }
        
        public static void ToBlobArray(ref BlobBuilder builder, ref BlobArray<Mission> blobArray,
            MissionSchema[] schemas)
        {
            // Allocate space for all missions in the blob array
            var missions = builder.Allocate(ref blobArray, schemas.Length);

            for (int i = 0; i < schemas.Length; i++)
            {
                if(schemas[i] == null) continue;
                // Convert basic mission properties
                missions[i] = new Mission
                {
                    id = (ushort)schemas[i].ID,
                    station = (ushort)schemas[i].stationSchema.ID,
                    name = (ushort)schemas[i].nameSchema.ID
                };

                // Handle the goals array efficiently
                if (schemas[i].goals != null && schemas[i].goals.Length > 0)
                {
                    // Allocate exactly the right amount of space for goals
                    var goalsBlobArray = builder.Allocate(ref missions[i].Goals, schemas[i].goals.Length);

                    // Convert each goal schema to its ID in a single pass
                    for (int j = 0; j < schemas[i].goals.Length; j++)
                    {
                        goalsBlobArray[j] = (ushort)schemas[i].goals[j].ID;
                    }
                }
                else
                {
                    // Initialize empty goals array to prevent null reference issues
                    builder.Allocate(ref missions[i].Goals, 0);
                }
            }
        }
    }
}