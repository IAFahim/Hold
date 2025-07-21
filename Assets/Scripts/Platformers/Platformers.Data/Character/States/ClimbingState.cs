using Unity.Entities;
using Unity.CharacterController;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Transforms;

public struct ClimbingState : IPlatformerCharacterState
{
    public float3 LastKnownClimbNormal;

    private bool _foundValidClimbSurface;

    public void OnStateEnter(CharacterState previousState, ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        ref var character = ref aspect.Character.ValueRW;
        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var characterProperties = ref aspect.CharacterAspect.CharacterProperties.ValueRW;
        ref var characterRotation = ref aspect.CharacterAspect.LocalTransform.ValueRW.Rotation;

        aspect.SetCapsuleGeometry(character.ClimbingGeometry.ToCapsuleGeometry());

        characterProperties.EvaluateGrounding = false;
        characterProperties.DetectMovementCollisions = false;
        characterProperties.DecollideFromOverlaps = false;
        characterBody.IsGrounded = false;

        LastKnownClimbNormal = -MathUtilities.GetForwardFromRotation(characterRotation);
    }

    public void OnStateExit(CharacterState nextState, ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var characterProperties = ref aspect.CharacterAspect.CharacterProperties.ValueRW;

        aspect.CharacterAspect.SetOrUpdateParentBody(ref baseContext, ref characterBody, default, default);
        characterProperties.EvaluateGrounding = true;
        characterProperties.DetectMovementCollisions = true;
        characterProperties.DecollideFromOverlaps = true;
    }

    public void OnStatePhysicsUpdate(ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        var deltaTime = baseContext.Time.DeltaTime;
        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var character = ref aspect.Character.ValueRW;
        ref var characterControl = ref aspect.CharacterControl.ValueRW;
        ref var characterPosition = ref aspect.CharacterAspect.LocalTransform.ValueRW.Position;

        aspect.HandlePhysicsUpdatePhase1(ref context, ref baseContext, true, false);

        // Quad climbing surface detection raycasts
        _foundValidClimbSurface = false;
        if (ClimbingDetection(ref context, ref baseContext, in aspect, true, out LastKnownClimbNormal,
                out var closestClimbableHit, out var closestUnclimbableHit))
        {
            _foundValidClimbSurface = true;

            // Adjust distance of character to surface
            characterPosition += -closestClimbableHit.Distance * closestClimbableHit.SurfaceNormal;
            characterPosition += (character.ClimbingGeometry.Radius - character.ClimbingDistanceFromSurface) *
                                 -closestClimbableHit.SurfaceNormal;

            // decollide from most penetrating non-climbable hit
            if (closestUnclimbableHit.Entity != Entity.Null)
                characterPosition += -closestUnclimbableHit.Distance * closestUnclimbableHit.SurfaceNormal;

            // Move
            var climbMoveVector =
                math.normalizesafe(MathUtilities.ProjectOnPlane(characterControl.MoveVector, LastKnownClimbNormal)) *
                math.length(characterControl.MoveVector);
            var targetVelocity = climbMoveVector * character.ClimbingSpeed;
            CharacterControlUtilities.InterpolateVelocityTowardsTarget(ref characterBody.RelativeVelocity,
                targetVelocity, deltaTime, character.ClimbingMovementSharpness);
            characterBody.RelativeVelocity =
                MathUtilities.ProjectOnPlane(characterBody.RelativeVelocity, LastKnownClimbNormal);

            // Project velocity on non-climbable obstacles
            if (aspect.CharacterAspect.VelocityProjectionHits.Length > 0)
            {
                var tmpCharacterGrounded = false;
                BasicHit tmpCharacterGroundHit = default;
                aspect.ProjectVelocityOnHits(
                    ref context,
                    ref baseContext,
                    ref characterBody.RelativeVelocity,
                    ref tmpCharacterGrounded,
                    ref tmpCharacterGroundHit,
                    in aspect.CharacterAspect.VelocityProjectionHits,
                    math.normalizesafe(characterBody.RelativeVelocity));
            }

            // Apply velocity to position
            characterPosition += characterBody.RelativeVelocity * baseContext.Time.DeltaTime;

            aspect.CharacterAspect.SetOrUpdateParentBody(ref baseContext, ref characterBody, closestClimbableHit.Entity,
                closestClimbableHit.Position);
        }
        else
        {
            aspect.CharacterAspect.SetOrUpdateParentBody(ref baseContext, ref characterBody, default, default);
        }

        aspect.HandlePhysicsUpdatePhase2(ref context, ref baseContext, false, false, false, false, true);

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

        var geometryCenter = GetGeometryCenter(in aspect);

        // Rotate
        var targetCharacterUp = characterBody.GroundingUp;
        if (math.lengthsq(characterControl.MoveVector) > 0f)
            targetCharacterUp =
                math.normalizesafe(MathUtilities.ProjectOnPlane(characterControl.MoveVector, LastKnownClimbNormal));
        var targetRotation = quaternion.LookRotationSafe(-LastKnownClimbNormal, targetCharacterUp);
        var smoothedRotation = math.slerp(characterRotation, targetRotation,
            MathUtilities.GetSharpnessInterpolant(character.ClimbingRotationSharpness, deltaTime));
        MathUtilities.SetRotationAroundPoint(ref characterRotation, ref characterPosition, geometryCenter,
            smoothedRotation);
    }

    public void GetCameraParameters(in PlatformerCharacterComponent character, out Entity cameraTarget,
        out bool calculateUpFromGravity)
    {
        cameraTarget = character.ClimbingCameraTargetEntity;
        calculateUpFromGravity = true;
    }

    public void GetMoveVectorFromPlayerInput(in PlatformerPlayerInputs inputs, quaternion cameraRotation,
        out float3 moveVector)
    {
        var cameraFwd = math.mul(cameraRotation, math.forward());
        var cameraRight = math.mul(cameraRotation, math.right());
        var cameraUp = math.mul(cameraRotation, math.up());

        // Only use input if the camera is pointing towards the normal
        if (math.dot(LastKnownClimbNormal, cameraFwd) < -0.05f)
            moveVector = cameraRight * inputs.Move.x + cameraUp * inputs.Move.y;
        else
            moveVector = cameraRight * inputs.Move.x + cameraFwd * inputs.Move.y;
    }

    public bool DetectTransitions(ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        ref var characterControl = ref aspect.CharacterControl.ValueRW;
        ref var stateMachine = ref aspect.StateMachine.ValueRW;

        if (!_foundValidClimbSurface || characterControl.IsJumpPressed() || characterControl.IsDashPressed() ||
            characterControl.IsClimbPressed())
        {
            stateMachine.TransitionToState(CharacterState.AirMove, ref context, ref baseContext, in aspect);
            return true;
        }

        return aspect.DetectGlobalTransitions(ref context, ref baseContext);
    }

    public static float3 GetGeometryCenter(in PlatformerCharacterAspect aspect)
    {
        var characterPosition = aspect.CharacterAspect.LocalTransform.ValueRW.Position;
        var characterRotation = aspect.CharacterAspect.LocalTransform.ValueRW.Rotation;
        var character = aspect.Character.ValueRW;

        var characterTransform = new RigidTransform(characterRotation, characterPosition);
        var geometryCenter = math.transform(characterTransform, math.up() * character.ClimbingGeometry.Height * 0.5f);
        return geometryCenter;
    }

    public static bool CanStartClimbing(ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        ref var character = ref aspect.Character.ValueRW;

        aspect.SetCapsuleGeometry(character.ClimbingGeometry.ToCapsuleGeometry());
        var canStart = ClimbingDetection(ref context, ref baseContext, in aspect, false, out var _, out var _,
            out var _);
        aspect.SetCapsuleGeometry(character.StandingGeometry.ToCapsuleGeometry());

        return canStart;
    }

    public static bool ClimbingDetection(
        ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        in PlatformerCharacterAspect aspect,
        bool addUnclimbableHitsAsVelocityProjectionHits,
        out float3 avgClimbingSurfaceNormal,
        out DistanceHit closestClimbableHit,
        out DistanceHit closestUnclimbableHit)
    {
        var climbableNormalsCounter = 0;
        avgClimbingSurfaceNormal = default;
        closestClimbableHit = default;
        closestUnclimbableHit = default;

        ref var characterProperties = ref aspect.CharacterAspect.CharacterProperties.ValueRW;
        ref var character = ref aspect.Character.ValueRW;
        ref var characterPosition = ref aspect.CharacterAspect.LocalTransform.ValueRW.Position;
        ref var characterRotation = ref aspect.CharacterAspect.LocalTransform.ValueRW.Rotation;
        var characterScale = aspect.CharacterAspect.LocalTransform.ValueRO.Scale;

        aspect.CharacterAspect.CalculateDistanceAllCollisions(
            in aspect,
            ref context,
            ref baseContext,
            characterPosition,
            characterRotation,
            characterScale,
            0f,
            characterProperties.ShouldIgnoreDynamicBodies(),
            out baseContext.TmpDistanceHits);

        if (baseContext.TmpDistanceHits.Length > 0)
        {
            closestClimbableHit.Fraction = float.MaxValue;
            closestUnclimbableHit.Fraction = float.MaxValue;

            for (var i = 0; i < baseContext.TmpDistanceHits.Length; i++)
            {
                var tmpHit = baseContext.TmpDistanceHits[i];

                var faceNormal = tmpHit.SurfaceNormal;

                // This is necessary for cases where the detected hit is the edge of a triangle/plane
                if (PhysicsUtilities.GetHitFaceNormal(baseContext.PhysicsWorld.Bodies[tmpHit.RigidBodyIndex],
                        tmpHit.ColliderKey, out var tmpFaceNormal)) faceNormal = tmpFaceNormal;

                // Ignore back faces
                if (math.dot(faceNormal, tmpHit.SurfaceNormal) >
                    KinematicCharacterAspect.Constants.DotProductSimilarityEpsilon)
                {
                    var isClimbable = false;
                    if (character.ClimbableTag.Value > CustomPhysicsBodyTags.Nothing.Value)
                        if ((baseContext.PhysicsWorld.Bodies[tmpHit.RigidBodyIndex].CustomTags &
                             character.ClimbableTag.Value) > 0)
                            isClimbable = true;

                    // Add virtual velocityProjection hit in direction of unclimbable hit
                    if (isClimbable)
                    {
                        if (tmpHit.Fraction < closestClimbableHit.Fraction) closestClimbableHit = tmpHit;

                        avgClimbingSurfaceNormal += faceNormal;
                        climbableNormalsCounter++;
                    }
                    else
                    {
                        if (tmpHit.Fraction < closestUnclimbableHit.Fraction) closestUnclimbableHit = tmpHit;

                        if (addUnclimbableHitsAsVelocityProjectionHits)
                        {
                            var velProjHit = new KinematicVelocityProjectionHit(new BasicHit(tmpHit), false);
                            aspect.CharacterAspect.VelocityProjectionHits.Add(velProjHit);
                        }
                    }
                }
            }

            if (climbableNormalsCounter > 0)
            {
                avgClimbingSurfaceNormal = avgClimbingSurfaceNormal / climbableNormalsCounter;

                return true;
            }

            return false;
        }

        return false;
    }
}