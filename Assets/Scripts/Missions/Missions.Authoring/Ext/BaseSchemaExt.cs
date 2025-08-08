using System;
using Missions.Missions.Authoring.Schemas;
using Missions.Missions.Authoring.Scriptable;

namespace Missions.Missions.Authoring
{
    internal static class BaseSchemaExt
    {
        public static ENumType ToRangeType(this BaseSchema baseSchema)
        {
            return baseSchema switch
            {
                RangeFloatSchema => ENumType.Float,
                RangeIntSchema => ENumType.Int,
                _ => throw new ArgumentOutOfRangeException(nameof(baseSchema), baseSchema, null)
            };
        }

        public static ECrossLinkType ToCrossLinkType(this BaseSchema baseSchema)
        {
            return baseSchema switch
            {
                MissionSchema => ECrossLinkType.Mission,
                GoalSchema => ECrossLinkType.Goal,
                RewardSchema => ECrossLinkType.Reward,
                _ => throw new ArgumentOutOfRangeException(nameof(baseSchema), baseSchema, null)
            };
        }
        
        public static ETargetType ToTargetType(this BaseSchema baseSchema)
        {
            return baseSchema switch
            {
                
            };
        }
    }
}