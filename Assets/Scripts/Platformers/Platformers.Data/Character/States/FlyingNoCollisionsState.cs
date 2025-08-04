using Unity.Entities;
using Unity.CharacterController;
using Unity.Mathematics;

public struct FlyingNoCollisionsState : IPlatformerCharacterState
{
    public void OnStateEnter(CharacterState previousState, ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var characterProperties = ref aspect.CharacterAspect.CharacterProperties.ValueRW;
        ref var characterCollider = ref aspect.CharacterAspect.PhysicsCollider.ValueRW;
        ref var character = ref aspect.Character.ValueRW;

        ref var capsuleGeometry = ref aspect.CapsuleGeometry.ValueRO.BlobAssetRef.Value;
        aspect.SetCapsuleGeometry(capsuleGeometry.standing.ToCapsuleGeometry());

        KinematicCharacterUtilities.SetCollisionDetectionActive(false, ref characterProperties, ref characterCollider);
        characterBody.IsGrounded = false;
    }

    public void OnStateExit(CharacterState nextState, ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        ref var characterProperties = ref aspect.CharacterAspect.CharacterProperties.ValueRW;
        ref var characterCollider = ref aspect.CharacterAspect.PhysicsCollider.ValueRW;

        KinematicCharacterUtilities.SetCollisionDetectionActive(true, ref characterProperties, ref characterCollider);
    }

    public void OnStatePhysicsUpdate(ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        var deltaTime = baseContext.Time.DeltaTime;
        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var character = ref aspect.Character.ValueRW;
        ref var characterControl = ref aspect.CharacterControl.ValueRW;
        ref var characterPosition = ref aspect.CharacterAspect.LocalTransform.ValueRW.Position;

        aspect.CharacterAspect.Update_Initialize(in aspect, ref context, ref baseContext, ref characterBody, deltaTime);

        // Movement
        var targetVelocity = characterControl.MoveVector * character.FlyingMaxSpeed;
        CharacterControlUtilities.InterpolateVelocityTowardsTarget(ref characterBody.RelativeVelocity, targetVelocity,
            deltaTime, character.FlyingMovementSharpness);
        characterPosition += characterBody.RelativeVelocity * deltaTime;

        aspect.DetectGlobalTransitions(ref context, ref baseContext);
    }

    public void OnStateVariableUpdate(ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        ref var characterRotation = ref aspect.CharacterAspect.LocalTransform.ValueRW.Rotation;

        characterRotation = quaternion.identity;
    }

    public void GetCameraParameters(in PlatformerCharacterComponent character, out Entity cameraTarget,
        out bool calculateUpFromGravity)
    {
        cameraTarget = character.DefaultCameraTargetEntity;
        calculateUpFromGravity = false;
    }

    public void GetMoveVectorFromPlayerInput(in PlatformerPlayerInputs inputs, quaternion cameraRotation,
        out float3 moveVector)
    {
        PlatformerCharacterAspect.GetCommonMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
        var verticalInput = (inputs.JumpHeld ? 1f : 0f) + (inputs.RollHeld ? -1f : 0f);
        moveVector =
            MathUtilities.ClampToMaxLength(moveVector + math.mul(cameraRotation, math.up()) * verticalInput, 1f);
    }
}