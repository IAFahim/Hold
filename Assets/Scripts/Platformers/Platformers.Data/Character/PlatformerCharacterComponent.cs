using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Physics.Authoring;
using Unity.CharacterController;
using Unity.Physics;

[Serializable]
public struct PlatformerCharacterComponent : IComponentData
{
    [Header("References")] public Entity DefaultCameraTargetEntity;
    public Entity ClimbingCameraTargetEntity;
    public Entity SwimmingCameraTargetEntity;
    public Entity CrouchingCameraTargetEntity;
    public Entity MeshRootEntity;
    public Entity RopePrefabEntity;
    public Entity RollballMeshEntity;

    [Header("Ground movement")] public float GroundRunMaxSpeed;
    public float GroundSprintMaxSpeed;
    public float GroundedMovementSharpness;
    public float GroundedRotationSharpness;

    [Header("Crouching")] public float CrouchedMaxSpeed;
    public float CrouchedMovementSharpness;
    public float CrouchedRotationSharpness;

    [Header("Air movement")] public float AirAcceleration;
    public float AirMaxSpeed;
    public float AirDrag;
    public float AirRotationSharpness;

    [Header("Rolling")] public float RollingAcceleration;

    [Header("Wall run")] public float WallRunAcceleration;
    public float WallRunMaxSpeed;
    public float WallRunDrag;
    public float WallRunGravityFactor;
    public float WallRunJumpRatioFromCharacterUp;
    public float WallRunDetectionDistance;

    [Header("Flying")] public float FlyingMaxSpeed;
    public float FlyingMovementSharpness;

    [Header("Jumping")] public float GroundJumpSpeed;
    public float AirJumpSpeed;
    public float WallRunJumpSpeed;
    public float JumpHeldAcceleration;
    public float MaxHeldJumpTime;
    public byte MaxUngroundedJumps;
    public float JumpAfterUngroundedGraceTime;
    public float JumpBeforeGroundedGraceTime;

    [Header("Ledge Detection")] public float LedgeMoveSpeed;
    public float LedgeRotationSharpness;
    public float LedgeSurfaceProbingHeight;
    public float LedgeSurfaceObstructionProbingHeight;
    public float LedgeSideProbingLength;

    [Header("Dashing")] public float DashDuration;
    public float DashSpeed;

    [Header("Swimming")] public float SwimmingAcceleration;
    public float SwimmingMaxSpeed;
    public float SwimmingDrag;
    public float SwimmingRotationSharpness;
    public float SwimmingStandUpDistanceFromSurface;
    public float WaterDetectionDistance;
    public float SwimmingJumpSpeed;
    public float SwimmingSurfaceDiveThreshold;

    [Header("RopeSwing")] public float RopeSwingAcceleration;
    public float RopeSwingMaxSpeed;
    public float RopeSwingDrag;
    public float RopeLength;
    public float3 LocalRopeAnchorPoint;

    [Header("Sliding")] public float SlideFriction;
    public float SlideSteeringSharpness;
    public float SlideSpeedToExit;

    [Header("Climbing")] public float ClimbingDistanceFromSurface;
    public float ClimbingSpeed;
    public float ClimbingMovementSharpness;
    public float ClimbingRotationSharpness;


    [Header("Step & Slope")] public BasicStepAndSlopeHandlingParameters StepAndSlopeHandling;

    [Header("Misc")] public CustomPhysicsBodyTags StickySurfaceTag;
    public CustomPhysicsBodyTags ClimbableTag;
    public PhysicsCategoryTags WaterPhysicsCategory;
    public PhysicsCategoryTags RopeAnchorCategory;
    public float UpOrientationAdaptationSharpness;
    

    [HideInInspector] public float3 LocalLedgeDetectionPoint;
    [HideInInspector] public float3 LocalSwimmingDetectionPoint;
    [HideInInspector] public byte CurrentUngroundedJumps;
    [HideInInspector] public float HeldJumpTimeCounter;
    [HideInInspector] public bool JumpPressedBeforeBecameGrounded;
    [HideInInspector] public bool AllowJumpAfterBecameUngrounded;
    [HideInInspector] public bool AllowHeldJumpInAir;
    [HideInInspector] public float LastTimeJumpPressed;
    [HideInInspector] public float LastTimeWasGrounded;
    [HideInInspector] public bool HasDetectedMoveAgainstWall;
    [HideInInspector] public float3 LastKnownWallNormal;
    [HideInInspector] public float LedgeGrabBlockCounter;
    [HideInInspector] public float DistanceFromWaterSurface;
    [HideInInspector] public float3 DirectionToWaterSurface;
    [HideInInspector] public bool IsSprinting;
    [HideInInspector] public bool IsOnStickySurface;
}


[Serializable]
public struct CharacterCapsuleGeometry
{
    public CapsuleGeometryDefinition standing;
    public CapsuleGeometryDefinition crouching;
    public CapsuleGeometryDefinition rolling;
    public CapsuleGeometryDefinition climbing;
    public CapsuleGeometryDefinition swimming;
}

public struct CapsuleGeometryBlobComponent : IComponentData
{
    public BlobAssetReference<CharacterCapsuleGeometry> BlobAssetRef;
}

public struct PlatformerCharacterInitialized : IComponentData
{
}

[Serializable]
[BurstCompile]
public struct CapsuleGeometryDefinition
{
    public float Radius;
    public float Height;
    public float3 Center;

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CapsuleGeometry ToCapsuleGeometry()
    {
        Height = math.max(Height, (Radius + math.EPSILON) * 2f);
        var halfHeight = Height * 0.5f;

        return new CapsuleGeometry
        {
            Radius = Radius,
            Vertex0 = Center + -math.up() * (halfHeight - Radius),
            Vertex1 = Center + math.up() * (halfHeight - Radius)
        };
    }
}
