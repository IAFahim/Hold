using Unity.Entities;
using Unity.CharacterController;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public struct SwimmingState : IPlatformerCharacterState
{
    public bool HasJumpedWhileSwimming;
    public bool HasDetectedGrounding;
    public bool ShouldExitSwimming;

    private const float kDistanceFromSurfaceToAllowJumping = -0.05f;
    private const float kForcedDistanceFromSurface = 0.01f;

    public void OnStateEnter(CharacterState previousState, ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var characterProperties = ref aspect.CharacterAspect.CharacterProperties.ValueRW;
        ref var capsuleGeometry = ref aspect.CapsuleGeometry.ValueRO.BlobAssetRef.Value;
        aspect.SetCapsuleGeometry(capsuleGeometry.swimming.ToCapsuleGeometry());

        characterProperties.SnapToGround = false;
        characterBody.IsGrounded = false;

        HasJumpedWhileSwimming = false;
        ShouldExitSwimming = false;
    }

    public void OnStateExit(CharacterState nextState, ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        ref var characterProperties = ref aspect.CharacterAspect.CharacterProperties.ValueRW;
        ref var character = ref aspect.Character.ValueRW;

        characterProperties.SnapToGround = true;
    }

    public void OnStatePhysicsUpdate(ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        aspect.HandlePhysicsUpdatePhase1(ref context, ref baseContext, true, true);

        PreMovementUpdate(ref context, ref baseContext, in aspect);

        aspect.HandlePhysicsUpdatePhase2(ref context, ref baseContext, false, false, true, false, true);

        PostMovementUpdate(ref context, ref baseContext, in aspect);

        DetectTransitions(ref context, ref baseContext, in aspect);
    }

    public void OnStateVariableUpdate(ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        var deltaTime = baseContext.Time.DeltaTime;
        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var character = ref aspect.Character.ValueRW;
        ref var characterControl = ref aspect.CharacterControl.ValueRW;
        ref var characterPosition = ref aspect.CharacterAspect.LocalTransform.ValueRW.Position;
        ref var characterRotation = ref aspect.CharacterAspect.LocalTransform.ValueRW.Rotation;
        var customGravity = aspect.CustomGravity.ValueRO;
        ref var capsuleGeometry = ref aspect.CapsuleGeometry.ValueRO.BlobAssetRef.Value;

        if (!ShouldExitSwimming)
        {
            if (character.DistanceFromWaterSurface > character.SwimmingStandUpDistanceFromSurface)
            {
                // when close to surface, orient self up
                var upPlane = -math.normalizesafe(customGravity.Gravity);
                float3 targetForward = default;
                if (math.lengthsq(characterControl.MoveVector) > 0f)
                {
                    targetForward =
                        math.normalizesafe(MathUtilities.ProjectOnPlane(characterControl.MoveVector, upPlane));
                }
                else
                {
                    targetForward =
                        math.normalizesafe(
                            MathUtilities.ProjectOnPlane(MathUtilities.GetForwardFromRotation(characterRotation),
                                upPlane));
                    if (math.dot(characterBody.GroundingUp, upPlane) < 0f) targetForward = -targetForward;
                }

                var targetRotation = MathUtilities.CreateRotationWithUpPriority(upPlane, targetForward);
                targetRotation = math.slerp(characterRotation, targetRotation,
                    MathUtilities.GetSharpnessInterpolant(character.SwimmingRotationSharpness, deltaTime));
                MathUtilities.SetRotationAroundPoint(ref characterRotation, ref characterPosition,
                    aspect.GetGeometryCenter(capsuleGeometry.swimming), targetRotation);
            }
            else
            {
                if (math.lengthsq(characterControl.MoveVector) > 0f)
                {
                    // Make character up face the movement direction, and character forward face gravity direction as much as it can
                    var targetRotation = MathUtilities.CreateRotationWithUpPriority(
                        math.normalizesafe(characterControl.MoveVector), math.normalizesafe(customGravity.Gravity));
                    targetRotation = math.slerp(characterRotation, targetRotation,
                        MathUtilities.GetSharpnessInterpolant(character.SwimmingRotationSharpness, deltaTime));
                    MathUtilities.SetRotationAroundPoint(ref characterRotation, ref characterPosition,
                        aspect.GetGeometryCenter(capsuleGeometry.swimming), targetRotation);
                }
            }
        }
    }

    public void GetCameraParameters(in PlatformerCharacterComponent character, out Entity cameraTarget,
        out bool calculateUpFromGravity)
    {
        cameraTarget = character.SwimmingCameraTargetEntity;
        calculateUpFromGravity = true;
    }

    public void GetMoveVectorFromPlayerInput(in PlatformerPlayerInputs inputs, quaternion cameraRotation,
        out float3 moveVector)
    {
        var cameraFwd = math.mul(cameraRotation, math.forward());
        var cameraRight = math.mul(cameraRotation, math.right());
        var cameraUp = math.mul(cameraRotation, math.up());

        moveVector = cameraRight * inputs.Move.x + cameraFwd * inputs.Move.y;
        if (inputs.JumpHeld) moveVector += cameraUp;
        if (inputs.RollHeld) moveVector -= cameraUp;
        moveVector = MathUtilities.ClampToMaxLength(moveVector, 1f);
    }

    public void PreMovementUpdate(ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        var deltaTime = baseContext.Time.DeltaTime;
        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var character = ref aspect.Character.ValueRW;
        ref var characterControl = ref aspect.CharacterControl.ValueRW;
        ref var characterRotation = ref aspect.CharacterAspect.LocalTransform.ValueRW.Rotation;

        HasDetectedGrounding = characterBody.IsGrounded;
        characterBody.IsGrounded = false;

        if (DetectWaterZones(ref context, ref baseContext, in aspect, out character.DirectionToWaterSurface,
                out character.DistanceFromWaterSurface))
        {
            // Movement
            var addedMoveVector = float3.zero;
            if (character.DistanceFromWaterSurface > character.SwimmingStandUpDistanceFromSurface)
            {
                // When close to water surface, prevent moving down unless the input points strongly down
                var dotMoveDirectionWithSurface = math.dot(math.normalizesafe(characterControl.MoveVector),
                    character.DirectionToWaterSurface);
                if (dotMoveDirectionWithSurface > character.SwimmingSurfaceDiveThreshold)
                    characterControl.MoveVector = MathUtilities.ProjectOnPlane(characterControl.MoveVector,
                        character.DirectionToWaterSurface);

                // Add an automatic move towards surface
                addedMoveVector = character.DirectionToWaterSurface * 0.1f;
            }

            var acceleration = (characterControl.MoveVector + addedMoveVector) * character.SwimmingAcceleration;
            var speedMultiplier = aspect.Carrying.ValueRO.ComputeSpeedMultiplier();
            var swimMax = character.SwimmingMaxSpeed * speedMultiplier;
            CharacterControlUtilities.StandardAirMove(ref characterBody.RelativeVelocity, acceleration,
                swimMax, -MathUtilities.GetForwardFromRotation(characterRotation), deltaTime, true);

            // Water drag
            CharacterControlUtilities.ApplyDragToVelocity(ref characterBody.RelativeVelocity, deltaTime,
                character.SwimmingDrag);

            // Handle jumping out of water when close to water surface
            HasJumpedWhileSwimming = false;
            if (characterControl.IsJumpPressed() &&
                character.DistanceFromWaterSurface > kDistanceFromSurfaceToAllowJumping)
            {
                CharacterControlUtilities.StandardJump(ref characterBody,
                    characterBody.GroundingUp * character.SwimmingJumpSpeed, true, characterBody.GroundingUp);
                HasJumpedWhileSwimming = true;
            }
        }
        else
        {
            ShouldExitSwimming = true;
        }
    }

    public void PostMovementUpdate(ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var character = ref aspect.Character.ValueRW;
        ref var characterPosition = ref aspect.CharacterAspect.LocalTransform.ValueRW.Position;

        var determinedHasExitedWater = false;
        if (DetectWaterZones(ref context, ref baseContext, in aspect, out character.DirectionToWaterSurface,
                out character.DistanceFromWaterSurface))
            // Handle snapping to water surface when trying to swim out of the water
            if (character.DistanceFromWaterSurface > -kForcedDistanceFromSurface)
            {
                var currentDistanceToTargetDistance = -kForcedDistanceFromSurface - character.DistanceFromWaterSurface;
                var translationSnappedToWaterSurface = characterPosition +
                                                       character.DirectionToWaterSurface *
                                                       currentDistanceToTargetDistance;

                // Only snap to water surface if we're not jumping out of the water, or if we'd be obstructed when trying to snap back (allows us to walk out of water)
                if (HasJumpedWhileSwimming || characterBody.GroundHit.Entity != Entity.Null)
                {
                    determinedHasExitedWater = true;
                }
                else
                {
                    // Snap position bact to water surface
                    characterPosition = translationSnappedToWaterSurface;

                    // Project velocity on water surface normal
                    characterBody.RelativeVelocity = MathUtilities.ProjectOnPlane(characterBody.RelativeVelocity,
                        character.DirectionToWaterSurface);
                }
            }

        ShouldExitSwimming = determinedHasExitedWater;
    }

    public bool DetectTransitions(ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        ref var stateMachine = ref aspect.StateMachine.ValueRW;

        if (ShouldExitSwimming || HasDetectedGrounding)
        {
            if (HasDetectedGrounding)
            {
                stateMachine.TransitionToState(CharacterState.GroundMove, ref context, ref baseContext, in aspect);
                return true;
            }
            else
            {
                stateMachine.TransitionToState(CharacterState.AirMove, ref context, ref baseContext, in aspect);
                return true;
            }
        }

        return aspect.DetectGlobalTransitions(ref context, ref baseContext);
    }

    public static unsafe bool DetectWaterZones(ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect,
        out float3 directionToWaterSurface, out float waterSurfaceDistance)
    {
        directionToWaterSurface = default;
        waterSurfaceDistance = 0f;

        ref var physicsCollider = ref aspect.CharacterAspect.PhysicsCollider.ValueRW;
        ref var character = ref aspect.Character.ValueRW;
        ref var characterPosition = ref aspect.CharacterAspect.LocalTransform.ValueRW.Position;
        ref var characterRotation = ref aspect.CharacterAspect.LocalTransform.ValueRW.Rotation;

        var characterRigidTransform = new RigidTransform(characterRotation, characterPosition);
        var swimmingDetectionPointWorldPosition =
            math.transform(characterRigidTransform, character.LocalSwimmingDetectionPoint);
        var waterDetectionFilter = new CollisionFilter
        {
            BelongsTo = physicsCollider.ColliderPtr->GetCollisionFilter().BelongsTo,
            CollidesWith = character.WaterPhysicsCategory.Value
        };

        var pointInput = new PointDistanceInput
        {
            Filter = waterDetectionFilter,
            MaxDistance = character.WaterDetectionDistance,
            Position = swimmingDetectionPointWorldPosition
        };

        if (baseContext.PhysicsWorld.CalculateDistance(pointInput, out var closestHit))
        {
            directionToWaterSurface =
                closestHit.SurfaceNormal; // always goes in the direction of decolliding from the target collider
            waterSurfaceDistance = closestHit.Distance; // positive means above surface
            return true;
        }

        return false;
    }
}