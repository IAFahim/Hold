using Unity.Entities;
using Unity.CharacterController;
using Unity.Mathematics;

public struct SlidingState : IPlatformerCharacterState
{
    public void OnStateEnter(CharacterState previousState, ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        ref var capsuleGeometry = ref aspect.CapsuleGeometry.ValueRO.BlobAssetRef.Value;
        aspect.SetCapsuleGeometry(capsuleGeometry.crouching.ToCapsuleGeometry());
    }

    public void OnStateExit(CharacterState nextState, ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        // The transition logic handles standing up, so we don't need to force a geometry change here.
        // The next state's OnStateEnter will set the appropriate geometry.
    }

    public void OnStatePhysicsUpdate(ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        var deltaTime = baseContext.Time.DeltaTime;
        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var character = ref aspect.Character.ValueRW;
        ref var characterControl = ref aspect.CharacterControl.ValueRW;
        var customGravity = aspect.CustomGravity.ValueRO;

        aspect.HandlePhysicsUpdatePhase1(ref context, ref baseContext, true, true);

        if (characterBody.IsGrounded)
        {
            // Apply slope-based acceleration/deceleration
            var gravityOnPlane = MathUtilities.ProjectOnPlane(customGravity.Gravity, characterBody.GroundHit.Normal);
            characterBody.RelativeVelocity += gravityOnPlane * deltaTime;

            // Apply friction
            characterBody.RelativeVelocity *= (1f - (character.SlideFriction * deltaTime));

            // Apply steering
            if (math.lengthsq(characterBody.RelativeVelocity) > 0.1f)
            {
                var forwardDirection = math.normalizesafe(characterBody.RelativeVelocity);
                var steeringVector = MathUtilities.ProjectOnPlane(characterControl.MoveVector, forwardDirection);
                CharacterControlUtilities.AccelerateVelocity(ref characterBody.RelativeVelocity, steeringVector * character.SlideSteeringSharpness, deltaTime);
            }
        }
        else
        {
            // If we are not grounded, behave like in air
            CharacterControlUtilities.AccelerateVelocity(ref characterBody.RelativeVelocity, customGravity.Gravity, deltaTime);
        }

        aspect.HandlePhysicsUpdatePhase2(ref context, ref baseContext, true, true, true, true, true);

        DetectTransitions(ref context, ref baseContext, in aspect);
    }

    public void OnStateVariableUpdate(ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        var deltaTime = baseContext.Time.DeltaTime;
        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var character = ref aspect.Character.ValueRW;
        ref var characterRotation = ref aspect.CharacterAspect.LocalTransform.ValueRW.Rotation;
        var customGravity = aspect.CustomGravity.ValueRO;

        // Orient character towards velocity
        if (math.lengthsq(characterBody.RelativeVelocity) > 0f)
        {
            CharacterControlUtilities.SlerpRotationTowardsDirectionAroundUp(ref characterRotation, deltaTime, math.normalizesafe(characterBody.RelativeVelocity), MathUtilities.GetUpFromRotation(characterRotation), character.GroundedRotationSharpness);
        }

        // Orient character up with ground normal
        character.IsOnStickySurface = PhysicsUtilities.HasPhysicsTag(in baseContext.PhysicsWorld, characterBody.GroundHit.RigidBodyIndex, character.StickySurfaceTag);
        if (character.IsOnStickySurface)
        {
            CharacterControlUtilities.SlerpCharacterUpTowardsDirection(ref characterRotation, deltaTime, characterBody.GroundHit.Normal, character.UpOrientationAdaptationSharpness);
        }
        else
        {
            CharacterControlUtilities.SlerpCharacterUpTowardsDirection(ref characterRotation, deltaTime, math.normalizesafe(-customGravity.Gravity), character.UpOrientationAdaptationSharpness);
        }
    }

    public void GetCameraParameters(in PlatformerCharacterComponent character, out Entity cameraTarget, out bool calculateUpFromGravity)
    {
        cameraTarget = character.CrouchingCameraTargetEntity;
        calculateUpFromGravity = !character.IsOnStickySurface;
    }

    public void GetMoveVectorFromPlayerInput(in PlatformerPlayerInputs inputs, quaternion cameraRotation, out float3 moveVector)
    {
        PlatformerCharacterAspect.GetCommonMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
    }

    public bool DetectTransitions(ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var characterControl = ref aspect.CharacterControl.ValueRW;
        ref var stateMachine = ref aspect.StateMachine.ValueRW;
        ref var character = ref aspect.Character.ValueRW;

        // Transition to air move if not grounded
        if (!characterBody.IsGrounded)
        {
            stateMachine.TransitionToState(CharacterState.AirMove, ref context, ref baseContext, in aspect);
            return true;
        }

        // Transition to crouch if speed is too low
        if (math.lengthsq(characterBody.RelativeVelocity) < (character.SlideSpeedToExit * character.SlideSpeedToExit))
        {
            stateMachine.TransitionToState(CharacterState.Crouched, ref context, ref baseContext, in aspect);
            return true;
        }

        // Transition to ground move if crouch/roll is released and can stand up
        if (!characterControl.IsRollHeld() && aspect.CanStandUp(ref context, ref baseContext))
        {
            stateMachine.TransitionToState(CharacterState.GroundMove, ref context, ref baseContext, in aspect);
            return true;
        }

        // Transition to air move on jump
        if (characterControl.IsJumpPressed())
        {
            CharacterControlUtilities.StandardJump(ref characterBody, characterBody.GroundingUp * character.GroundJumpSpeed, true, characterBody.GroundingUp);
            stateMachine.TransitionToState(CharacterState.AirMove, ref context, ref baseContext, in aspect);
            return true;
        }

        return aspect.DetectGlobalTransitions(ref context, ref baseContext);
    }
}