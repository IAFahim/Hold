using Eases.Ease.Data;
using Moves.Move.Data;
using Times.Time.Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Moves.Move
{
    public partial struct EaseGroupJobEntity : IJobEntity
    {
        [ReadOnly] public float DeltaTime;

        private void Execute(
            ref LocalTransform local,
            in EaseComponent ease,
            in AliveTimeComponent aliveTime,
            in MoveStartComponent moveStart,
            in MoveEndComponent moveEnd
        )
        {
            if (ease.Leading3Bit == 0)
            {
                if (ease.TryEvaluate(aliveTime.Value, 5, DeltaTime, out var t))
                {
                    local.Position = math.lerp(moveStart.Value, moveEnd.Value, t);
                }
            }
        }
    }
}