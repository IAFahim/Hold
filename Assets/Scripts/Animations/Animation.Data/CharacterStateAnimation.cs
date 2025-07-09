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
        public EAnimationState Animation;
        public half Speed;

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
        public readonly void GetAnimationState(
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
            out CharacterStateAnimation characterState
        )
        {
            characterState = this;
            switch (characterState.Current)
            {
                case ECharacterState.GroundMove:
                {
                    if (math.lengthsq(moveVector) < 0.0001f)
                    {
                        characterState.Speed = new half(1);
                        characterState.Animation = EAnimationState.Idle;
                    }
                    else
                    {
                        if (isSprinting)
                        {
                            var velocityRatio = (half)(velocityMagnitude / groundSprintMaxSpeed);
                            characterState.Speed = velocityRatio;
                            characterState.Animation = EAnimationState.Sprint;
                        }
                        else
                        {
                            var velocityRatio = (half)(velocityMagnitude / groundRunMaxSpeed);
                            characterState.Speed = velocityRatio;
                            characterState.Animation = EAnimationState.Run;
                        }
                    }
                }
                    break;
                case ECharacterState.Crouched:
                {
                    if (math.lengthsq(moveVector) < 0.0001f)
                    {
                        characterState.Speed = new half(1);
                        characterState.Animation = EAnimationState.CrouchIdle;
                    }
                    else
                    {
                        half velocityRatio = (half)(velocityMagnitude / crouchedMaxSpeed);
                        characterState.Speed = velocityRatio;
                        characterState.Animation = EAnimationState.CrouchMove;
                    }
                }
                    break;
                case ECharacterState.AirMove:
                {
                    characterState.Speed = new half(1);
                    characterState.Animation = EAnimationState.InAir;
                }
                    break;
                case ECharacterState.Dashing:
                {
                    characterState.Speed = new half(1);
                    characterState.Animation = EAnimationState.Dash;
                }
                    break;
                case ECharacterState.WallRun:
                {
                    float3 rightVector = math.mul(rotation, new float3(1f, 0f, 0f));
                    bool wallIsOnTheLeft = math.dot(rightVector, lastKnownWallNormal) > 0f;
                    characterState.Speed = new half(1);
                    characterState.Animation =
                        wallIsOnTheLeft ? EAnimationState.WallRunLeft : EAnimationState.WallRunRight;
                }
                    break;
                case ECharacterState.RopeSwing:
                {
                    characterState.Speed = new half(1);
                    characterState.Animation = EAnimationState.RopeHang;
                }
                    break;
                case ECharacterState.Climbing:
                {
                    half velocityRatio = (half)(climbingSpeed > 0f ? velocityMagnitude / climbingSpeed : 0f);
                    characterState.Speed = velocityRatio;
                    characterState.Animation = EAnimationState.ClimbingMove;
                }
                    break;
                case ECharacterState.LedgeGrab:
                {
                    half velocityRatio = (half)(ledgeMoveSpeed > 0f ? velocityMagnitude / ledgeMoveSpeed : 0f);
                    characterState.Speed = velocityRatio;
                    characterState.Animation = EAnimationState.LedgeGrabMove;
                }
                    break;
                case ECharacterState.LedgeStandingUp:
                {
                    characterState.Speed = new half(1);
                    characterState.Animation = EAnimationState.LedgeStandUp;
                }
                    break;
                case ECharacterState.Swimming:
                {
                    float velocityRatio = swimmingMaxSpeed > 0f ? velocityMagnitude / swimmingMaxSpeed : 0f;
                    if (velocityRatio < 0.1f)
                    {
                        characterState.Speed = new half(1);
                        characterState.Animation = EAnimationState.SwimmingIdle;
                    }
                    else
                    {
                        characterState.Speed = (half)velocityRatio;
                        characterState.Animation = EAnimationState.SwimmingMove;
                    }
                }
                    break;
                case ECharacterState.Sliding:
                {
                    characterState.Speed = new half(1);
                    characterState.Animation = EAnimationState.Sliding;
                }
                    break;
                case ECharacterState.Rolling:
                case ECharacterState.FlyingNoCollisions:
                {
                    characterState.Speed = new half(1);
                    characterState.Animation = EAnimationState.Idle;
                }
                    break;
                default:
                {
                    characterState.Speed = new half(1);
                    characterState.Animation = EAnimationState.Idle;
                }
                    break;
            }
        }
    }
}