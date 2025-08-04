using BovineLabs.Core.ObjectManagement;
using BovineLabs.Essence.Authoring;
using Goals.Goals.Authoring.Schema;
using Rewards.Rewards.Authoring.Settings;
using Rewards.Rewards.Data;
using Rewards.Rewards.Data.GoalReward;
using UnityEngine;

namespace Rewards.Rewards.Authoring.Schema
{
    [CreateAssetMenu(menuName = "Hold/" + TypeString + "/Create " + FieldName, fileName = FieldName)]
    [
        AutoRef(nameof(RewardGoalIntSettings), nameof(RewardGoalIntSettings.schemas),
            FieldName, TypeString + "/" + FieldName, createNull: false, defaultFileName: "Reward Goal Int"
        )
    ]
    public class RewardGoalIntSchema : RewardSchema<RewardGoalInt>
    {
        private const string FieldName = nameof(RewardGoalIntSchema);

        public IntrinsicSchemaObject schemaObject;
        public ERewardGoalType rewardGoalType;
        public GoalRangeIntSchema goalIntSchema;
        public int reward;

        public override RewardGoalInt ToData()
        {
            return new RewardGoalInt
            {
                id = (ushort)ID,
                rewardGoalType = rewardGoalType,
                goalId = goalIntSchema.id,
                reward = reward,
            };
        }
    }
}