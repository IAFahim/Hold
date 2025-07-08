using Animations.Animation.Data.enums;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Animations.Animation.Data
{
    [BurstCompile]
    public struct CharacterStateAnimation : IComponentData
    {
        public ECharacterState Previous;
        public ECharacterState Current;

        public float2 moveVector;
        public float velocityMagnitude;
        public bool isSprinting;
        public float groundSprintMaxSpeed;
        public float groundRunMaxSpeed;
        public float crouchedMaxSpeed;
        public float climbingSpeed;
        public float ledgeMoveSpeed;
        public float swimmingMaxSpeed;
        public quaternion rotation;
        public float3 lastKnownWallNormal;

        [BurstCompile]
        public readonly void Update(
            float2 moveVector,
            float velocityMagnitude,
            bool isSprinting,
            float groundSprintMaxSpeed,
            float groundRunMaxSpeed,
            float crouchedMaxSpeed,
            float climbingSpeed,
            float ledgeMoveSpeed,
            float swimmingMaxSpeed,
            quaternion rotation,
            float3 lastKnownWallNormal,
            out float speed,
            out EAnimationState state
        )
        {
            speed = 1f;
            state = EAnimationState.Idle;

            switch (Current)
            {
                case ECharacterState.GroundMove:
                {
                    if (math.lengthsq(moveVector) < 0.0001f)
                    {
                        speed = 1f;
                        state = EAnimationState.Idle;
                    }
                    else
                    {
                        if (isSprinting)
                        {
                            float velocityRatio = velocityMagnitude / groundSprintMaxSpeed;
                            speed = velocityRatio;
                            state = EAnimationState.Sprint;
                        }
                        else
                        {
                            float velocityRatio = velocityMagnitude / groundRunMaxSpeed;
                            speed = velocityRatio;
                            state = EAnimationState.Run;
                        }
                    }
                }
                    break;
                case ECharacterState.Crouched:
                {
                    if (math.lengthsq(moveVector) < 0.0001f)
                    {
                        speed = 1f;
                        state = EAnimationState.CrouchIdle;
                    }
                    else
                    {
                        float velocityRatio = velocityMagnitude / crouchedMaxSpeed;
                        speed = velocityRatio;
                        state = EAnimationState.CrouchMove;
                    }
                }
                    break;
                case ECharacterState.AirMove:
                {
                    speed = 1f;
                    state = EAnimationState.InAir;
                }
                    break;
                case ECharacterState.Dashing:
                {
                    speed = 1f;
                    state = EAnimationState.Dash;
                }
                    break;
                case ECharacterState.WallRun:
                {
                    float3 rightVector = math.mul(rotation, new float3(1f, 0f, 0f));
                    bool wallIsOnTheLeft = math.dot(rightVector, lastKnownWallNormal) > 0f;
                    speed = 1f;
                    state = wallIsOnTheLeft ? EAnimationState.WallRunLeft : EAnimationState.WallRunRight;
                }
                    break;
                case ECharacterState.RopeSwing:
                {
                    speed = 1f;
                    state = EAnimationState.RopeHang;
                }
                    break;
                case ECharacterState.Climbing:
                {
                    float velocityRatio = climbingSpeed > 0f ? velocityMagnitude / climbingSpeed : 0f;
                    speed = velocityRatio;
                    state = EAnimationState.ClimbingMove;
                }
                    break;
                case ECharacterState.LedgeGrab:
                {
                    float velocityRatio = ledgeMoveSpeed > 0f ? velocityMagnitude / ledgeMoveSpeed : 0f;
                    speed = velocityRatio;
                    state = EAnimationState.LedgeGrabMove;
                }
                    break;
                case ECharacterState.LedgeStandingUp:
                {
                    speed = 1f;
                    state = EAnimationState.LedgeStandUp;
                }
                    break;
                case ECharacterState.Swimming:
                {
                    float velocityRatio = swimmingMaxSpeed > 0f ? velocityMagnitude / swimmingMaxSpeed : 0f;
                    if (velocityRatio < 0.1f)
                    {
                        speed = 1f;
                        state = EAnimationState.SwimmingIdle;
                    }
                    else
                    {
                        speed = velocityRatio;
                        state = EAnimationState.SwimmingMove;
                    }
                }
                    break;
                case ECharacterState.Sliding:
                {
                    speed = 1f;
                    state = EAnimationState.Sliding;
                }
                    break;
                case ECharacterState.Rolling:
                case ECharacterState.FlyingNoCollisions:
                {
                    speed = 1f;
                    state = EAnimationState.Idle;
                }
                    break;
                default:
                    speed = 1f;
                    state = EAnimationState.Idle;
                    break;
            }
        }
    }
}