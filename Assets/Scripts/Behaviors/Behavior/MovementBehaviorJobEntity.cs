using Unity.Burst;
using Unity.Entities;

namespace Behaviors.Behavior
{
    [BurstCompile]
    public partial struct MovementBehaviorJobEntity : IJobEntity
    {
        
        [BurstCompile]
        private void Execute()
        {
            byte animation = 0;
        }
        
        // public 
        
        
    }
}