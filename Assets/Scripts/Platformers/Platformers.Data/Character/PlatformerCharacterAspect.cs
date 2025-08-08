using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.CharacterController;
using CapsuleCollider = Unity.Physics.CapsuleCollider;

public struct PlatformerCharacterUpdateContext
{
    public int ChunkIndex;
    public EntityCommandBuffer.ParallelWriter EndFrameECB;
    [ReadOnly] public ComponentLookup<CharacterFrictionModifier> CharacterFrictionModifierLookup;
    [ReadOnly] public BufferLookup<LinkedEntityGroup> LinkedEntityGroupLookup;

    public void SetChunkIndex(int chunkIndex)
    {
        ChunkIndex = chunkIndex;
    }

    public void OnSystemCreate(ref SystemState state)
    {
        CharacterFrictionModifierLookup = state.GetComponentLookup<CharacterFrictionModifier>(true);
        LinkedEntityGroupLookup = state.GetBufferLookup<LinkedEntityGroup>(true);
    }

    public void OnSystemUpdate(ref SystemState state, EntityCommandBuffer endFrameECB)
    {
        EndFrameECB = endFrameECB.AsParallelWriter();
        CharacterFrictionModifierLookup.Update(ref state);
        LinkedEntityGroupLookup.Update(ref state);
    }
}

public readonly partial struct PlatformerCharacterAspect : IAspect,
    IKinematicCharacterProcessor<PlatformerCharacterUpdateContext>
{
    public readonly KinematicCharacterAspect CharacterAspect;
    public readonly RefRW<PlatformerCharacterComponent> Character;
    public readonly RefRW<PlatformerCharacterControl> CharacterControl;
    public readonly RefRW<PlatformerCharacterStateMachine> StateMachine;
    public readonly RefRW<CustomGravity> CustomGravity;
    public readonly RefRO<CapsuleGeometryBlobComponent> CapsuleGeometry;

    public void PhysicsUpdate(ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext)
    {
        ref var characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        ref var character = ref Character.ValueRW;
        ref var characterControl = ref CharacterControl.ValueRW;
        ref var stateMachine = ref StateMachine.ValueRW;

        // Common pre-update logic across states
        {
            // Handle initial state transition
            if (stateMachine.CurrentState == CharacterState.Uninitialized)
                stateMachine.TransitionToState(CharacterState.AirMove, ref context, ref baseContext, in this);

            if (characterControl.IsJumpHeld())
            {
                character.HeldJumpTimeCounter += baseContext.Time.DeltaTime;
            }
            else
            {
                character.HeldJumpTimeCounter = 0f;
                character.AllowHeldJumpInAir = false;
            }

            if (characterControl.IsJumpPressed()) character.LastTimeJumpPressed = (float)baseContext.Time.ElapsedTime;

            character.HasDetectedMoveAgainstWall = false;
            if (characterBody.IsGrounded)
            {
                character.LastTimeWasGrounded = (float)baseContext.Time.ElapsedTime;

                character.CurrentUngroundedJumps = 0;
                character.AllowJumpAfterBecameUngrounded = true;
                character.AllowHeldJumpInAir = true;
            }

            if (character.LedgeGrabBlockCounter > 0f) character.LedgeGrabBlockCounter -= baseContext.Time.DeltaTime;
        }

        stateMachine.OnStatePhysicsUpdate(stateMachine.CurrentState, ref context, ref baseContext, in this);

        // Common post-update logic across states
        {
            character.JumpPressedBeforeBecameGrounded = false;
        }
    }

    public void VariableUpdate(ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext)
    {
        ref var characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        ref var stateMachine = ref StateMachine.ValueRW;
        ref var characterRotation = ref CharacterAspect.LocalTransform.ValueRW.Rotation;

        KinematicCharacterUtilities.AddVariableRateRotationFromFixedRateRotation(ref characterRotation,
            characterBody.RotationFromParent, baseContext.Time.DeltaTime, characterBody.LastPhysicsUpdateDeltaTime);
        stateMachine.OnStateVariableUpdate(stateMachine.CurrentState, ref context, ref baseContext, in this);
    }

    public bool DetectGlobalTransitions(ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext)
    {
        ref var stateMachine = ref StateMachine.ValueRW;
        ref var characterControl = ref CharacterControl.ValueRW;

        if (stateMachine.CurrentState != CharacterState.Swimming &&
            stateMachine.CurrentState != CharacterState.FlyingNoCollisions)
            if (SwimmingState.DetectWaterZones(ref context, ref baseContext, in this, out var tmpDirection,
                    out var tmpDistance))
                if (tmpDistance < 0f)
                {
                    stateMachine.TransitionToState(CharacterState.Swimming, ref context, ref baseContext, in this);
                    return true;
                }

        if (characterControl.IsFlyNoCollisionsPressed())
        {
            if (stateMachine.CurrentState == CharacterState.FlyingNoCollisions)
            {
                stateMachine.TransitionToState(CharacterState.AirMove, ref context, ref baseContext, in this);
                return true;
            }
            else
            {
                stateMachine.TransitionToState(CharacterState.FlyingNoCollisions, ref context, ref baseContext,
                    in this);
                return true;
            }
        }

        return false;
    }

    public void HandlePhysicsUpdatePhase1(
        ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        bool allowParentHandling,
        bool allowGroundingDetection)
    {
        ref var characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        ref var characterPosition = ref CharacterAspect.LocalTransform.ValueRW.Position;

        CharacterAspect.Update_Initialize(in this, ref context, ref baseContext, ref characterBody,
            baseContext.Time.DeltaTime);
        if (allowParentHandling)
            CharacterAspect.Update_ParentMovement(in this, ref context, ref baseContext, ref characterBody,
                ref characterPosition, characterBody.WasGroundedBeforeCharacterUpdate);
        if (allowGroundingDetection)
            CharacterAspect.Update_Grounding(in this, ref context, ref baseContext, ref characterBody,
                ref characterPosition);
    }

    public void HandlePhysicsUpdatePhase2(
        ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        bool allowPreventGroundingFromFutureSlopeChange,
        bool allowGroundingPushing,
        bool allowMovementAndDecollisions,
        bool allowMovingPlatformDetection,
        bool allowParentHandling)
    {
        ref var character = ref Character.ValueRW;
        ref var characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        ref var characterPosition = ref CharacterAspect.LocalTransform.ValueRW.Position;
        var customGravity = CustomGravity.ValueRO;

        if (allowPreventGroundingFromFutureSlopeChange)
            CharacterAspect.Update_PreventGroundingFromFutureSlopeChange(in this, ref context, ref baseContext,
                ref characterBody, in character.StepAndSlopeHandling);
        if (allowGroundingPushing)
            CharacterAspect.Update_GroundPushing(in this, ref context, ref baseContext, customGravity.Gravity);
        if (allowMovementAndDecollisions)
            CharacterAspect.Update_MovementAndDecollisions(in this, ref context, ref baseContext, ref characterBody,
                ref characterPosition);
        if (allowMovingPlatformDetection)
            CharacterAspect.Update_MovingPlatformDetection(ref baseContext, ref characterBody);
        if (allowParentHandling) CharacterAspect.Update_ParentMomentum(ref baseContext, ref characterBody);
        CharacterAspect.Update_ProcessStatefulCharacterHits();
    }

    public unsafe void SetCapsuleGeometry(CapsuleGeometry capsuleGeometry)
    {
        ref var physicsCollider = ref CharacterAspect.PhysicsCollider.ValueRW;

        var capsuleCollider = (CapsuleCollider*)physicsCollider.ColliderPtr;
        capsuleCollider->Geometry = capsuleGeometry;
    }

    public float3 GetGeometryCenter(CapsuleGeometryDefinition geometry)
    {
        var characterPosition = CharacterAspect.LocalTransform.ValueRW.Position;
        var characterRotation = CharacterAspect.LocalTransform.ValueRW.Rotation;

        var characterTransform = new RigidTransform(characterRotation, characterPosition);
        var geometryCenter = math.transform(characterTransform, geometry.Center);
        return geometryCenter;
    }

    public unsafe bool CanStandUp(ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext)
    {
        ref var physicsCollider = ref CharacterAspect.PhysicsCollider.ValueRW;
        ref var character = ref Character.ValueRW;
        ref var characterPosition = ref CharacterAspect.LocalTransform.ValueRW.Position;
        ref var characterRotation = ref CharacterAspect.LocalTransform.ValueRW.Rotation;
        var characterScale = CharacterAspect.LocalTransform.ValueRO.Scale;
        ref var characterProperties = ref CharacterAspect.CharacterProperties.ValueRW;
        ref var capsuleGeometry = ref CapsuleGeometry.ValueRO.BlobAssetRef.Value;

        // Overlap test with standing geometry to see if we have space to stand
        var capsuleCollider = (CapsuleCollider*)physicsCollider.ColliderPtr;

        var initialGeometry = capsuleCollider->Geometry;
        capsuleCollider->Geometry = capsuleGeometry.standing.ToCapsuleGeometry();

        bool isObstructed = CharacterAspect.CalculateDistanceClosestCollisions(
            in this,
            ref context,
            ref baseContext,
            characterPosition,
            characterRotation,
            characterScale,
            0f,
            characterProperties.ShouldIgnoreDynamicBodies(),
            out _);

        capsuleCollider->Geometry = initialGeometry;

        return !isObstructed;
    }

    public static bool CanBeAffectedByWindZone(CharacterState currentCharacterState)
    {
        if (currentCharacterState == CharacterState.GroundMove ||
            currentCharacterState == CharacterState.AirMove ||
            currentCharacterState == CharacterState.Crouched ||
            currentCharacterState == CharacterState.Rolling)
            return true;

        return false;
    }

    public static void GetCommonMoveVectorFromPlayerInput(in PlatformerPlayerInputs inputs, quaternion cameraRotation,
        out float3 moveVector)
    {
        moveVector = math.mul(cameraRotation, math.right()) * inputs.Move.x +
                     math.mul(cameraRotation, math.forward()) * inputs.Move.y;
    }

    #region Character Processor Callbacks

    public void UpdateGroundingUp(
        ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext)
    {
        ref var characterBody = ref CharacterAspect.CharacterBody.ValueRW;

        CharacterAspect.Default_UpdateGroundingUp(ref characterBody);
    }

    public bool CanCollideWithHit(
        ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        in BasicHit hit)
    {
        return PhysicsUtilities.IsCollidable(hit.Material);
    }

    public bool IsGroundedOnHit(
        ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        in BasicHit hit,
        int groundingEvaluationType)
    {
        var characterComponent = Character.ValueRO;

        return CharacterAspect.Default_IsGroundedOnHit(
            in this,
            ref context,
            ref baseContext,
            in hit,
            in characterComponent.StepAndSlopeHandling,
            groundingEvaluationType);
    }

    public void OnMovementHit(
        ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        ref KinematicCharacterHit hit,
        ref float3 remainingMovementDirection,
        ref float remainingMovementLength,
        float3 originalVelocityDirection,
        float hitDistance)
    {
        ref var characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        ref var characterPosition = ref CharacterAspect.LocalTransform.ValueRW.Position;
        var characterComponent = Character.ValueRO;

        CharacterAspect.Default_OnMovementHit(
            in this,
            ref context,
            ref baseContext,
            ref characterBody,
            ref characterPosition,
            ref hit,
            ref remainingMovementDirection,
            ref remainingMovementLength,
            originalVelocityDirection,
            hitDistance,
            characterComponent.StepAndSlopeHandling.StepHandling,
            characterComponent.StepAndSlopeHandling.MaxStepHeight,
            characterComponent.StepAndSlopeHandling.CharacterWidthForStepGroundingCheck);
    }

    public void OverrideDynamicHitMasses(
        ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        ref PhysicsMass characterMass,
        ref PhysicsMass otherMass,
        BasicHit hit)
    {
    }

    public void ProjectVelocityOnHits(
        ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        ref float3 velocity,
        ref bool characterIsGrounded,
        ref BasicHit characterGroundHit,
        in DynamicBuffer<KinematicVelocityProjectionHit> velocityProjectionHits,
        float3 originalVelocityDirection)
    {
        var characterComponent = Character.ValueRO;

        CharacterAspect.Default_ProjectVelocityOnHits(
            ref velocity,
            ref characterIsGrounded,
            ref characterGroundHit,
            in velocityProjectionHits,
            originalVelocityDirection,
            characterComponent.StepAndSlopeHandling.ConstrainVelocityToGroundPlane);
    }

    #endregion
}