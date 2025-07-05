using Moves.Move.Data.Blobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Moves.Move
{
    [BurstCompile]
    public partial struct EaseGroupJobChunk : IJobEntity
    {
        [ReadOnly] public float DeltaTime;
        [ReadOnly] public NativeArray<EaseCache>.ReadOnly EaseCache;
        [ReadOnly] public NativeArray<float3>.ReadOnly PositionCache;


        [BurstCompile]
        public void Execute(
            ref LocalTransform lt,
            ref EaseLinkComponent easeLink
        )
        {
            var currentConfig = EaseCache[easeLink.Current];
            var isComplete = currentConfig.Ease.TryComplete(
                ref easeLink.ElapsedTime,
                currentConfig.Duration,
                DeltaTime,
                out var t
            );

            switch (currentConfig.Ease.Leading3Bit())
            {
                // case 0:
                //     local.Position.x = math.lerp(moveStartComponents[i].Value.x, moveEndComponents[i].Value.x, t);
                //     break;
                //
                // case 1:
                //     local.Position.y = math.lerp(moveStartComponents[i].Value.y, moveEndComponents[i].Value.y, t);
                //     break;
                //
                // case 2:
                //     local.Position.z = math.lerp(moveStartComponents[i].Value.z, moveEndComponents[i].Value.z, t);
                //     break;
                //
                case 3:
                    var start = PositionCache[easeLink.Current];
                    var end = PositionCache[currentConfig.Next];
                    lt.Position = math.lerp(start, end, t);
                    break;

                // case 4:
                //     var angleX = math.lerp(startRots[i].Value, endRots[i].Value, t);
                //     local.Rotation = quaternion.RotateX(angleX);
                //     break;
                //
                // case 5:
                //     var angleY = math.lerp(startRots[i].Value, endRots[i].Value, t);
                //     local.Rotation = quaternion.RotateY(angleY);
                //     break;
                //
                // case 6:
                //     var angleZ = math.lerp(startRots[i].Value, endRots[i].Value, t);
                //     local.Rotation = quaternion.RotateZ(angleZ);
                //     break;
                //
                // case 7:
                //     break;
            }

            if (isComplete) easeLink.Current = currentConfig.Next;
        }
    }
}