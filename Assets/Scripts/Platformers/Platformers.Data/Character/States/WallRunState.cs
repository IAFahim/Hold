using Unity.Entities;
using Unity.CharacterController;
using Unity.Mathematics;
using Unity.Physics;

public struct WallRunState : IPlatformerCharacterState
{
    public void OnStateEnter(CharacterState previousState, ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        ref var character = ref aspect.Character.ValueRW;
        aspect.SetCapsuleGeometry(character.StandingGeometry.ToCapsuleGeometry());
    }

    public void OnStateExit(CharacterState nextState, ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
    }

    public void OnStatePhysicsUpdate(ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        var deltaTime = baseContext.Time.DeltaTime;
        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var character = ref aspect.Character.ValueRW;
        ref var characterControl = ref aspect.CharacterControl.ValueRW;
        var customGravity = aspect.CustomGravity.ValueRO;

        aspect.HandlePhysicsUpdatePhase1(ref context, ref baseContext, true, true);

        // Detect if still moving against ungrounded surface
        if (aspect.CharacterAspect.MovementWouldHitNonGroundedObstruction(in aspect, ref context, ref baseContext,
                -character.LastKnownWallNormal * character.WallRunDetectionDistance, out var detectedHit))
        {
            character.HasDetectedMoveAgainstWall = true;
            character.LastKnownWallNormal = detectedHit.SurfaceNormal;
        }
        else
        {
            character.LastKnownWallNormal = default;
        }

        if (character.HasDetectedMoveAgainstWall)
        {
            var constrainedMoveDirection =
                math.normalizesafe(math.cross(character.LastKnownWallNormal, characterBody.GroundingUp));

            var moveVectorOnPlane =
                math.normalizesafe(MathUtilities.ProjectOnPlane(characterControl.MoveVector,
                    characterBody.GroundingUp)) * math.length(characterControl.MoveVector);
            var acceleration = moveVectorOnPlane * character.WallRunAcceleration;
            acceleration = math.projectsafe(acceleration, constrainedMoveDirection);
            CharacterControlUtilities.StandardAirMove(ref characterBody.RelativeVelocity, acceleration,
                character.WallRunMaxSpeed, characterBody.GroundingUp, deltaTime, false);

            // Jumping
            if (character.HasDetectedMoveAgainstWall && characterControl.IsJumpPressed())
            {
                var jumpDirection = math.normalizesafe(math.lerp(characterBody.GroundingUp,
                    character.LastKnownWallNormal, character.WallRunJumpRatioFromCharacterUp));
                CharacterControlUtilities.StandardJump(ref characterBody, jumpDirection * character.WallRunJumpSpeed,
                    true, jumpDirection);
            }

            if (characterControl.IsJumpHeld() && character.HeldJumpTimeCounter < character.MaxHeldJumpTime)
                characterBody.RelativeVelocity +=
                    characterBody.GroundingUp * character.JumpHeldAcceleration * deltaTime;
        }

        // Gravity
        CharacterControlUtilities.AccelerateVelocity(ref characterBody.RelativeVelocity,
            customGravity.Gravity * character.WallRunGravityFactor, deltaTime);

        // Drag
        CharacterControlUtilities.ApplyDragToVelocity(ref characterBody.RelativeVelocity, deltaTime,
            character.WallRunDrag);

        aspect.HandlePhysicsUpdatePhase2(ref context, ref baseContext, false, true, true, true, true);

        DetectTransitions(ref context, ref baseContext, in aspect);
    }

    public void OnStateVariableUpdate(ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        var deltaTime = baseContext.Time.DeltaTime;
        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var character = ref aspect.Character.ValueRW;
        ref var characterRotation = ref aspect.CharacterAspect.LocalTransform.ValueRW.Rotation;
        var customGravity = aspect.CustomGravity.ValueRO;

        // Orientation
        if (character.HasDetectedMoveAgainstWall)
        {
            var rotationDirection =
                math.normalizesafe(math.cross(character.LastKnownWallNormal, characterBody.GroundingUp));
            if (math.dot(rotationDirection, characterBody.RelativeVelocity) < 0f) rotationDirection *= -1f;
            CharacterControlUtilities.SlerpRotationTowardsDirectionAroundUp(ref characterRotation, deltaTime,
                rotationDirection, characterBody.GroundingUp, character.GroundedRotationSharpness);
        }

        CharacterControlUtilities.SlerpCharacterUpTowardsDirection(ref characterRotation, deltaTime,
            math.normalizesafe(-customGravity.Gravity), character.UpOrientationAdaptationSharpness);
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
        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var character = ref aspect.Character.ValueRW;
        ref var characterControl = ref aspect.CharacterControl.ValueRW;
        ref var stateMachine = ref aspect.StateMachine.ValueRW;

        if (characterControl.IsRollHeld())
        {
            stateMachine.TransitionToState(CharacterState.Rolling, ref context, ref baseContext, in aspect);
            return true;
        }

        if (characterControl.IsDashPressed())
        {
            stateMachine.TransitionToState(CharacterState.Dashing, ref context, ref baseContext, in aspect);
            return true;
        }

        if (characterBody.IsGrounded)
        {
            stateMachine.TransitionToState(CharacterState.GroundMove, ref context, ref baseContext, in aspect);
            return true;
        }

        if (!character.HasDetectedMoveAgainstWall)
        {
            stateMachine.TransitionToState(CharacterState.AirMove, ref context, ref baseContext, in aspect);
            return true;
        }

        return aspect.DetectGlobalTransitions(ref context, ref baseContext);
    }
}