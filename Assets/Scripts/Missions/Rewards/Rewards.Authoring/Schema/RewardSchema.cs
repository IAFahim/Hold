
using SchemaSettings.SchemaSettings.Authoring;

namespace Rewards.Rewards.Authoring.Schema
{
    public abstract class RewardSchema<T> : BakingSchema<T> where T : struct
    {
        protected const string TypeString = "Reward";
    }
}
