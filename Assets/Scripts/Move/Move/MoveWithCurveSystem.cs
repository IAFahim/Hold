// MoveWithCurveSystem.cs

using Move.Move.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// Your BlobCurve namespace

namespace Move.Move
{
    [BurstCompile]
    public partial struct MoveWithCurveSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // No setup needed
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            // No teardown needed
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (mover, transform, entity) in
                     SystemAPI.Query<RefRW<MoveWithCurve>, RefRW<LocalTransform>>().WithEntityAccess())
            {
                // --- Update Timer ---
                mover.ValueRW.ElapsedTime += deltaTime;

                // --- Calculate Normalized Time ---
                // The time value (from 0 to 1) to sample the curve at.
                float normalizedTime = math.min(mover.ValueRO.ElapsedTime / mover.ValueRO.Duration, 1f);

                // --- Evaluate the Curve ---
                // Get a reference to the curve data inside the blob asset.
                ref var curve = ref mover.ValueRO.Curve.Value;

                // Evaluate the curve at the normalized time to get the eased progress.
                // Your BlobCurve.Evaluate is designed to take a 'time' value. Since our
                // AnimationCurve is authored from t=0 to t=1, normalizedTime is the correct input.
                float easedT = curve.Evaluate(normalizedTime);

                // --- Update Position ---
                // Linearly interpolate between start and end using the eased progress value.
                transform.ValueRW.Position = math.lerp(
                    mover.ValueRO.StartPosition,
                    mover.ValueRO.EndPosition,
                    easedT
                );

                // --- Check for Completion ---
            }
        }
    }
}