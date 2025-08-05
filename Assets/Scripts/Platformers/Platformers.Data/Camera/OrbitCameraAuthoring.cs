﻿using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class OrbitCameraAuthoring : MonoBehaviour
{
    [Header("Rotation")] public float RotationSpeed = 2f;
    public float MaxVAngle = 89f;
    public float MinVAngle = -89f;
    public bool RotateWithCharacterParent = true;

    [Header("Distance")] public float StartDistance = 5f;
    public float MinDistance = 0f;
    public float MaxDistance = 10f;
    public float DistanceMovementSpeed = 0.1f;
    public float DistanceMovementSharpness = 20f;

    [Header("Obstructions")] public float ObstructionRadius = 0.1f;
    public float ObstructionInnerSmoothingSharpness = float.MaxValue;
    public float ObstructionOuterSmoothingSharpness = 5f;
    public bool PreventFixedUpdateJitter = true;

    [Header("Misc")] public float CameraTargetTransitionTime = 0.4f;
    public List<GameObject> IgnoredEntities = new();
    public float pitchAngle = 25f;
    public float3 planarForward = math.forward();

    public bool smartModeEnabled = true;
    public float smartModeActivationDelay = 2.0f;
    public float smartModeRotationSpeed = 90.0f;
    public float smartModeAngleThreshold = 5.0f;

    private void OnValidate()
    {
        planarForward = math.normalize(planarForward);
    }

    public class Baker : Baker<OrbitCameraAuthoring>
    {
        public override void Bake(OrbitCameraAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);

            AddComponent(entity, new OrbitCamera
            {
                RotationSpeed = authoring.RotationSpeed,
                MaxVAngle = authoring.MaxVAngle,
                MinVAngle = authoring.MinVAngle,
                RotateWithCharacterParent = authoring.RotateWithCharacterParent,

                MinDistance = authoring.MinDistance,
                MaxDistance = authoring.MaxDistance,
                DistanceMovementSpeed = authoring.DistanceMovementSpeed,
                DistanceMovementSharpness = authoring.DistanceMovementSharpness,

                ObstructionRadius = authoring.ObstructionRadius,
                ObstructionInnerSmoothingSharpness = authoring.ObstructionInnerSmoothingSharpness,
                ObstructionOuterSmoothingSharpness = authoring.ObstructionOuterSmoothingSharpness,
                PreventFixedUpdateJitter = authoring.PreventFixedUpdateJitter,

                CameraTargetTransitionTime = authoring.CameraTargetTransitionTime,

                TargetDistance = authoring.StartDistance,
                SmoothedTargetDistance = authoring.StartDistance,
                ObstructedDistance = authoring.StartDistance,

                PitchAngle = authoring.pitchAngle,
                PlanarForward = math.normalize(authoring.planarForward),
                SmartModeEnabled = authoring.smartModeEnabled,
                SmartModeActivationDelay = authoring.smartModeActivationDelay,
                SmartModeRotationSpeed = authoring.smartModeRotationSpeed,
                SmartModeAngleThreshold = authoring.smartModeAngleThreshold,
            });

            AddComponent(entity, new OrbitCameraControl());

            var ignoredEntitiesBuffer = AddBuffer<OrbitCameraIgnoredEntityBufferElement>(entity);
            foreach (var obj in authoring.IgnoredEntities)
            {
                ignoredEntitiesBuffer.Add(new OrbitCameraIgnoredEntityBufferElement
                {
                    Entity = GetEntity(obj, TransformUsageFlags.None)
                });
            }
        }
    }
}