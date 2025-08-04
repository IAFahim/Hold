using System;
using BovineLabs.Core.ObjectManagement;
using BovineLabs.Core.Settings;
using Goals.Goals.Authoring.Schema;
using Goals.Goals.Data.Goals;
using Rewards.Rewards.Data;
using UnityEngine;

namespace Maps.Maps.Data
{
    [Serializable]
    public struct Mission : IUID
    {
        public ushort id;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public Segment segment;

        public GoalInt[] goalInts;
        public GoalRangeInt[] goalRangeInts;

        public GoalFloat[] goalFloats;
        public GoalRangeFloat[] goalRangeFloats;

        public RewardInt[] rewardInts;
        public RewardFloat[] rewardFloats;

        public GoalTime time;
    }

    [CreateAssetMenu(menuName = "Hold/Mission/Create " + nameof(MissionSchema), fileName = nameof(MissionSchema))]
    [AutoRef(nameof(MissionSettings), nameof(MissionSettings.schemas), nameof(MissionSchema),
        "Goal/" + nameof(MissionSchema))]
    public class MissionSchema : GoalSchema<GoalFloat>
    {
        
    }

    public class MissionSettings : ScriptableObject, ISettings
    {
        [SerializeField] public MissionSchema[] schemas = Array.Empty<MissionSchema>();
    }
}