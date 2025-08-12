using Unity.Entities;
using Unity.CharacterController;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public struct LedgeGrabState : IPlatformerCharacterState
{
    private bool DetectedMustExitLedge;
    private float3 ForwardHitNormal;

    private const float collisionOffset = 0.02f;

    public void OnStateEnter(CharacterState previousState, ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var characterProperties = ref aspect.CharacterAspect.CharacterProperties.ValueRW;

        ref var capsuleGeometry = ref aspect.CapsuleGeometry.ValueRO.BlobAssetRef.Value;
        aspect.SetCapsuleGeometry(capsuleGeometry.standing.ToCapsuleGeometry());

        characterProperties.EvaluateGrounding = false;
        characterProperties.DetectMovementCollisions = false;
        characterProperties.DecollideFromOverlaps = false;

        characterBody.RelativeVelocity = float3.zero;
        characterBody.IsGrounded = false;
    }

    public void OnStateExit(CharacterState nextState, ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var characterProperties = ref aspect.CharacterAspect.CharacterProperties.ValueRW;

        if (nextState != CharacterState.LedgeStandingUp)
        {
            characterProperties.EvaluateGrounding = true;
            characterProperties.DetectMovementCollisions = true;
            characterProperties.DecollideFromOverlaps = true;

            aspect.CharacterAspect.SetOrUpdateParentBody(ref baseContext, ref characterBody, default, default);
        }

        characterBody.RelativeVelocity = float3.zero;
    }

    public void OnStatePhysicsUpdate(ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        var deltaTime = baseContext.Time.DeltaTime;
        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var character = ref aspect.Character.ValueRW;
        ref var characterControl = ref aspect.CharacterControl.ValueRW;
        ref var characterPosition = ref aspect.CharacterAspect.LocalTransform.ValueRW.Position;
        ref var characterRotation = ref aspect.CharacterAspect.LocalTransform.ValueRW.Rotation;

        aspect.HandlePhysicsUpdatePhase1(ref context, ref baseContext, true, false);

        DetectedMustExitLedge = false;
        characterBody.RelativeVelocity = float3.zero;

        LedgeDetection(
            ref context,
            ref baseContext,
            in aspect,
            characterPosition,
            characterRotation,
            out var ledgeIsValid,
            out var surfaceHit,
            out var forwardHit,
            out var characterTranslationAtLedgeSurface,
            out var wouldBeGroundedOnLedgeSurfaceHit,
            out var forwardHitDistance,
            out var isObstructedAtSurface,
            out var isObstructedAtCurrentPosition,
            out var upOffsetToPlaceLedgeDetectionPointAtLedgeLevel);

        if (ledgeIsValid && !isObstructedAtSurface)
        {
            ForwardHitNormal = forwardHit.SurfaceNormal;

            // Stick to wall
            var characterForward = MathUtilities.GetForwardFromRotation(characterRotation);
            characterPosition += characterForward * (forwardHitDistance - collisionOffset);

            // Adjust to ledge height
            characterPosition += characterBody.GroundingUp *
                                 (upOffsetToPlaceLedgeDetectionPointAtLedgeLevel - collisionOffset);

            if (math.lengthsq(characterControl.MoveVector) > 0f)
            {
                // Move input
                var ledgeDirection = math.normalizesafe(math.cross(surfaceHit.SurfaceNormal, forwardHit.SurfaceNormal));
                var moveInputOnLedgeDirection = math.projectsafe(characterControl.MoveVector, ledgeDirection);

                // Check for move obstructions
                var targetTranslationAfterMove =
                    characterPosition + moveInputOnLedgeDirection * character.LedgeMoveSpeed * deltaTime;
                LedgeDetection(
                    ref context,
                    ref baseContext,
                    in aspect,
                    targetTranslationAfterMove,
                    characterRotation,
                    out var afterMoveLedgeIsValid,
                    out var afterMoveSurfaceHit,
                    out var afterMoveForwardHit,
                    out var afterMoveCharacterTranslationAtLedgeSurface,
                    out var afterMoveWouldBeGroundedOnLedgeSurfaceHit,
                    out var afterMoveForwardHitDistance,
                    out var afterMoveIsObstructedAtSurface,
                    out var afterMoveIsObstructedAtCurrentPosition,
                    out var afterMoveUpOffsetToPlaceLedgeDetectionPointAtLedgeLevel);

                if (afterMoveLedgeIsValid && !afterMoveIsObstructedAtSurface)
                {
                    characterBody.RelativeVelocity = moveInputOnLedgeDirection * character.LedgeMoveSpeed;

                    // Apply velocity to position
                    characterPosition += characterBody.RelativeVelocity * baseContext.Time.DeltaTime;
                }
            }

            aspect.CharacterAspect.SetOrUpdateParentBody(ref baseContext, ref characterBody, forwardHit.Entity,
                forwardHit.Position);
        }
        else
        {
            DetectedMustExitLedge = true;
        }

        // Detect letting go of ledge
        if (characterControl.IsCrouchPressed() || characterControl.IsDashPressed())
            character.LedgeGrabBlockCounter = 0.3f;

        aspect.HandlePhysicsUpdatePhase2(ref context, ref baseContext, false, false, false, false, true);

        DetectTransitions(ref context, ref baseContext, in aspect);
    }

    public void OnStateVariableUpdate(ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        var deltaTime = baseContext.Time.DeltaTime;
        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var character = ref aspect.Character.ValueRW;
        ref var characterRotation = ref aspect.CharacterAspect.LocalTransform.ValueRW.Rotation;

        // Adjust rotation to face current ledge wall
        var targetRotation = quaternion.LookRotationSafe(
            math.normalizesafe(MathUtilities.ProjectOnPlane(-ForwardHitNormal, characterBody.GroundingUp)),
            characterBody.GroundingUp);
        characterRotation = math.slerp(characterRotation, targetRotation,
            MathUtilities.GetSharpnessInterpolant(character.LedgeRotationSharpness, deltaTime));
    }

    public void GetCameraParameters(in PlatformerCharacterComponent character, out Entity cameraTarget,
        out bool calculateUpFromGravity)
    {
        cameraTarget = character.DefaultCameraTargetEntity;
        calculateUpFromGravity = true;
    }

    public void GetMoveVectorFromPlayerInput(in PlatformerPlayerInputs inputs, quaternion cameraRotation,
        out float3 moveVector)
    {
        PlatformerCharacterAspect.GetCommonMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
    }

    public bool DetectTransitions(ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        ref var character = ref aspect.Character.ValueRW;
        ref var characterControl = ref aspect.CharacterControl.ValueRW;
        ref var characterPosition = ref aspect.CharacterAspect.LocalTransform.ValueRW.Position;
        ref var characterRotation = ref aspect.CharacterAspect.LocalTransform.ValueRW.Rotation;
        ref var stateMachine = ref aspect.StateMachine.ValueRW;

        if (IsLedgeGrabBlocked(in character) || DetectedMustExitLedge)
        {
            stateMachine.TransitionToState(CharacterState.AirMove, ref context, ref baseContext, in aspect);
            return true;
        }

        if (characterControl.IsJumpPressed())
        {
            LedgeDetection(
                ref context,
                ref baseContext,
                in aspect,
                characterPosition,
                characterRotation,
                out var ledgeIsValid,
                out var surfaceHit,
                out var forwardHit,
                out var characterTranslationAtLedgeSurface,
                out var wouldBeGroundedOnLedgeSurfaceHit,
                out var forwardHitDistance,
                out var isObstructedAtSurface,
                out var isObstructedAtCurrentPosition,
                out var upOffsetToPlaceLedgeDetectionPointAtLedgeLevel);

            if (ledgeIsValid && !isObstructedAtSurface && wouldBeGroundedOnLedgeSurfaceHit)
            {
                stateMachine.LedgeStandingUpState.StandingPoint = surfaceHit.Position;
                stateMachine.TransitionToState(CharacterState.LedgeStandingUp, ref context, ref baseContext, in aspect);
                return true;
            }
        }

        return aspect.DetectGlobalTransitions(ref context, ref baseContext);
    }

    public static bool IsLedgeGrabBlocked(in PlatformerCharacterComponent character)
    {
        return character.LedgeGrabBlockCounter > 0f;
    }

    public static bool CanGrabLedge(ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect, out Entity ledgeEntity,
        out ColliderCastHit ledgeSurfaceHit)
    {
        ledgeEntity = Entity.Null;
        ledgeSurfaceHit = default;

        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var character = ref aspect.Character.ValueRW;
        ref var characterPosition = ref aspect.CharacterAspect.LocalTransform.ValueRW.Position;
        ref var characterRotation = ref aspect.CharacterAspect.LocalTransform.ValueRW.Rotation;

        if (IsLedgeGrabBlocked(in character)) return false;

        LedgeDetection(
            ref context,
            ref baseContext,
            in aspect,
            characterPosition,
            characterRotation,
            out var ledgeIsValid,
            out ledgeSurfaceHit,
            out var forwardHit,
            out var characterTranslationAtLedgeSurface,
            out var wouldBeGroundedOnLedgeSurfaceHit,
            out var forwardHitDistance,
            out var isObstructedAtSurface,
            out var isObstructedAtCurrentPosition,
            out var upOffsetToPlaceLedgeDetectionPointAtLedgeLevel);

        // Prevent detecting valid grab if going up
        // if (math.dot(characterBody.RelativeVelocity, ledgeSurfaceHit.SurfaceNormal) > 0f) ledgeIsValid = false;

        if (ledgeIsValid) ledgeEntity = ledgeSurfaceHit.Entity;

        return ledgeIsValid && !isObstructedAtSurface;
    }

    public static void LedgeDetection(
        ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        in PlatformerCharacterAspect aspect,
        float3 atCharacterTranslation,
        quaternion atCharacterRotation,
        out bool ledgeIsValid,
        out ColliderCastHit surfaceHit,
        out ColliderCastHit forwardHit,
        out float3 characterTranslationAtLedgeSurface,
        out bool wouldBeGroundedOnLedgeSurfaceHit,
        out float forwardHitDistance,
        out bool isObstructedAtSurface,
        out bool isObstructedAtCurrentPosition,
        out float upOffsetToPlaceLedgeDetectionPointAtLedgeLevel)
    {
        const float ledgeProbingToleranceOffset = 0.04f;

        ledgeIsValid = false;
        surfaceHit = default;
        forwardHit = default;
        characterTranslationAtLedgeSurface = default;
        wouldBeGroundedOnLedgeSurfaceHit = false;
        forwardHitDistance = -1f;
        isObstructedAtSurface = false;
        isObstructedAtCurrentPosition = false;
        upOffsetToPlaceLedgeDetectionPointAtLedgeLevel = -1f;

        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var characterProperties = ref aspect.CharacterAspect.CharacterProperties.ValueRW;
        ref var character = ref aspect.Character.ValueRW;
        var characterScale = aspect.CharacterAspect.LocalTransform.ValueRO.Scale;

        var currentCharacterForward = MathUtilities.GetForwardFromRotation(atCharacterRotation);
        var currentCharacterRight = MathUtilities.GetRightFromRotation(atCharacterRotation);
        var currentCharacterRigidTransform = math.RigidTransform(atCharacterRotation, atCharacterTranslation);
        var worldSpaceLedgeDetectionPoint =
            math.transform(currentCharacterRigidTransform, character.LocalLedgeDetectionPoint);
        var forwardDepthOfLedgeDetectionPoint =
            math.length(math.projectsafe(worldSpaceLedgeDetectionPoint - atCharacterTranslation,
                currentCharacterForward));

        // Forward detection against the ledge wall
        var forwardHitDetected = false;
        if (aspect.CharacterAspect.CastColliderClosestCollisions(
                in aspect,
                ref context,
                ref baseContext,
                atCharacterTranslation,
                atCharacterRotation,
                characterScale,
                currentCharacterForward,
                forwardDepthOfLedgeDetectionPoint,
                false,
                characterProperties.ShouldIgnoreDynamicBodies(),
                out forwardHit,
                out forwardHitDistance))
        {
            forwardHitDetected = true;

            if (aspect.CharacterAspect.CalculateDistanceClosestCollisions(
                    in aspect,
                    ref context,
                    ref baseContext,
                    atCharacterTranslation,
                    atCharacterRotation,
                    characterScale,
                    0f,
                    characterProperties.ShouldIgnoreDynamicBodies(),
                    out var closestOverlapHit))
                if (closestOverlapHit.Distance <= 0f)
                    isObstructedAtCurrentPosition = true;
        }

        // Cancel rest of detection if no forward hit detected
        if (!forwardHitDetected) return;

        // Cancel rest of detection if currently obstructed
        if (isObstructedAtCurrentPosition) return;

        // Raycast downward at detectionPoint to find a surface hit
        var surfaceRaycastHitDetected = false;
        var startPointOfSurfaceDetectionRaycast = worldSpaceLedgeDetectionPoint +
                                                  characterBody.GroundingUp * character.LedgeSurfaceProbingHeight;
        var surfaceRaycastLength = character.LedgeSurfaceProbingHeight + ledgeProbingToleranceOffset;
        if (aspect.CharacterAspect.RaycastClosestCollisions(
                in aspect,
                ref context,
                ref baseContext,
                startPointOfSurfaceDetectionRaycast,
                -characterBody.GroundingUp,
                surfaceRaycastLength,
                characterProperties.ShouldIgnoreDynamicBodies(),
                out var surfaceRaycastHit,
                out var surfaceRaycastHitDistance))
            if (surfaceRaycastHit.Fraction > 0f)
                surfaceRaycastHitDetected = true;

        // If no ray hit found, do more raycast tests on the sides
        if (!surfaceRaycastHitDetected)
        {
            var rightStartPointOfSurfaceDetectionRaycast = startPointOfSurfaceDetectionRaycast +
                                                           currentCharacterRight * character.LedgeSideProbingLength;
            if (aspect.CharacterAspect.RaycastClosestCollisions(
                    in aspect,
                    ref context,
                    ref baseContext,
                    rightStartPointOfSurfaceDetectionRaycast,
                    -characterBody.GroundingUp,
                    surfaceRaycastLength,
                    characterProperties.ShouldIgnoreDynamicBodies(),
                    out surfaceRaycastHit,
                    out surfaceRaycastHitDistance))
                if (surfaceRaycastHit.Fraction > 0f)
                    surfaceRaycastHitDetected = true;
        }

        if (!surfaceRaycastHitDetected)
        {
            var leftStartPointOfSurfaceDetectionRaycast = startPointOfSurfaceDetectionRaycast -
                                                          currentCharacterRight * character.LedgeSideProbingLength;
            if (aspect.CharacterAspect.RaycastClosestCollisions(
                    in aspect,
                    ref context,
                    ref baseContext,
                    leftStartPointOfSurfaceDetectionRaycast,
                    -characterBody.GroundingUp,
                    surfaceRaycastLength,
                    characterProperties.ShouldIgnoreDynamicBodies(),
                    out surfaceRaycastHit,
                    out surfaceRaycastHitDistance))
                if (surfaceRaycastHit.Fraction > 0f)
                    surfaceRaycastHitDetected = true;
        }

        // Cancel rest of detection if no surface raycast hit detected
        if (!surfaceRaycastHitDetected) return;

        // Cancel rest of detection if surface hit is dynamic
        if (PhysicsUtilities.IsBodyDynamic(baseContext.PhysicsWorld, surfaceRaycastHit.RigidBodyIndex)) return;

        ledgeIsValid = true;

        upOffsetToPlaceLedgeDetectionPointAtLedgeLevel = surfaceRaycastLength - surfaceRaycastHitDistance;

        // Note: this assumes that our transform pivot is at the base of our capsule collider
        var startPointOfSurfaceObstructionDetectionCast = surfaceRaycastHit.Position +
                                                          characterBody.GroundingUp *
                                                          character.LedgeSurfaceObstructionProbingHeight;

        // Check obstructions at surface hit point
        if (aspect.CharacterAspect.CastColliderClosestCollisions(
                in aspect,
                ref context,
                ref baseContext,
                startPointOfSurfaceObstructionDetectionCast,
                atCharacterRotation,
                characterScale,
                -characterBody.GroundingUp,
                character.LedgeSurfaceObstructionProbingHeight + ledgeProbingToleranceOffset,
                false,
                characterProperties.ShouldIgnoreDynamicBodies(),
                out surfaceHit,
                out var closestSurfaceObstructionHitDistance))
            if (surfaceHit.Fraction <= 0f)
                isObstructedAtSurface = true;

        // Cancel rest of detection if obstruction at surface
        if (isObstructedAtSurface) return;

        // Cancel rest of detection if found no surface hit
        if (surfaceHit.Entity == Entity.Null) return;

        characterTranslationAtLedgeSurface = startPointOfSurfaceObstructionDetectionCast +
                                             -characterBody.GroundingUp * closestSurfaceObstructionHitDistance;

        wouldBeGroundedOnLedgeSurfaceHit =
            aspect.IsGroundedOnHit(ref context, ref baseContext, new BasicHit(surfaceHit), 0);
    }
}