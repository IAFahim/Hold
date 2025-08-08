using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[Serializable]
public struct OrbitCamera : IComponentData
{
    public float RotationSpeed;
    public float MaxVAngle;
    public float MinVAngle;
    public bool RotateWithCharacterParent;

    public float MinDistance;
    public float MaxDistance;
    public float DistanceMovementSpeed;
    public float DistanceMovementSharpness;

    public float ObstructionRadius;
    public float ObstructionInnerSmoothingSharpness;
    public float ObstructionOuterSmoothingSharpness;
    public bool PreventFixedUpdateJitter;

    public float CameraTargetTransitionTime;

    public float TargetDistance;
    public float SmoothedTargetDistance;
    public float ObstructedDistance;
    public float PitchAngle;
    public float3 PlanarForward;

    public Entity ActiveCameraTarget;
    public Entity PreviousCameraTarget;
    public float CameraTargetTransitionStartTime;
    public RigidTransform CameraTargetTransform;
    public RigidTransform CameraTargetTransitionFromTransform;
    public bool PreviousCalculateUpFromGravity;
    
    [Header("Smart Mode Settings")]
    /// <summary>
    /// Enable/disable smart mode that automatically positions camera behind player
    /// </summary>
    public bool SmartModeEnabled;
    
    /// <summary>
    /// How long to wait (in seconds) after player stops providing input before smart mode activates
    /// </summary>
    public float SmartModeActivationDelay;
    
    /// <summary>
    /// Speed at which smart mode rotates the camera (degrees per second)
    /// </summary>
    public float SmartModeRotationSpeed;
    
    /// <summary>
    /// Minimum angle difference (in degrees) before smart mode kicks in
    /// </summary>
    public float SmartModeAngleThreshold;
    
    // Internal tracking variables
    /// <summary>
    /// Internal timer tracking how long since last player input
    /// </summary>
    public float SmartModeIdleTimer;
}

[Serializable]
public struct OrbitCameraControl : IComponentData
{
    public Entity FollowedCharacterEntity;
    public float2 LookDegreesDelta;
    public float ZoomDelta;
}

[Serializable]
public struct OrbitCameraIgnoredEntityBufferElement : IBufferElementData
{
    public Entity Entity;
}