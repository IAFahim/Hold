using SchemaSettings.SchemaSettings.Authoring;

namespace Goals.Goals.Authoring.Schema
{
    public abstract class GoalSchema<T> : BakingSchema<T> where T : struct
    {
        protected const string TypeString = "Goal";
    }
}