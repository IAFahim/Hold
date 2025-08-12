using Unity.Entities;
using Unity.CharacterController;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public struct RopeSwingState : IPlatformerCharacterState
{
    public float3 AnchorPoint;

    public void OnStateEnter(CharacterState previousState, ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        var entity = aspect.CharacterAspect.Entity;
        ref var characterProperties = ref aspect.CharacterAspect.CharacterProperties.ValueRW;
        ref var character = ref aspect.Character.ValueRW;
        ref var capsuleGeometry = ref aspect.CapsuleGeometry.ValueRO.BlobAssetRef.Value;
        aspect.SetCapsuleGeometry(capsuleGeometry.standing.ToCapsuleGeometry());

        characterProperties.EvaluateGrounding = false;

        // Spawn rope
        var ropeInstanceEntity = context.EndFrameECB.Instantiate(context.ChunkIndex, character.RopePrefabEntity);
        context.EndFrameECB.AddComponent(context.ChunkIndex, ropeInstanceEntity,
            new CharacterRope { OwningCharacterEntity = entity });
    }

    public void OnStateExit(CharacterState nextState, ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        ref var characterProperties = ref aspect.CharacterAspect.CharacterProperties.ValueRW;

        characterProperties.EvaluateGrounding = true;
        // Note: rope despawning is handled by the rope system itself
    }

    public void OnStatePhysicsUpdate(ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        var deltaTime = baseContext.Time.DeltaTime;
        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var character = ref aspect.Character.ValueRW;
        ref var characterControl = ref aspect.CharacterControl.ValueRW;
        ref var characterPosition = ref aspect.CharacterAspect.LocalTransform.ValueRW.Position;
        var customGravity = aspect.CustomGravity.ValueRO;
        var characterRotation = aspect.CharacterAspect.LocalTransform.ValueRW.Rotation;

        aspect.HandlePhysicsUpdatePhase1(ref context, ref baseContext, false, false);

        // Move
        var moveVectorOnPlane =
            math.normalizesafe(MathUtilities.ProjectOnPlane(characterControl.MoveVector, characterBody.GroundingUp)) *
            math.length(characterControl.MoveVector);
        var acceleration = moveVectorOnPlane * character.RopeSwingAcceleration;
        var speedMultiplier = aspect.Carrying.ValueRO.ComputeSpeedMultiplier();
        var ropeMax = character.RopeSwingMaxSpeed * speedMultiplier;
        CharacterControlUtilities.StandardAirMove(ref characterBody.RelativeVelocity, acceleration,
            ropeMax, characterBody.GroundingUp, deltaTime, false);

        // Gravity
        CharacterControlUtilities.AccelerateVelocity(ref characterBody.RelativeVelocity, customGravity.Gravity,
            deltaTime);

        // Drag
        CharacterControlUtilities.ApplyDragToVelocity(ref characterBody.RelativeVelocity, deltaTime,
            character.RopeSwingDrag);

        // Rope constraint
        var characterTransform = new RigidTransform(characterRotation, characterPosition);
        ConstrainToRope(ref characterPosition, ref characterBody.RelativeVelocity, character.RopeLength, AnchorPoint,
            math.transform(characterTransform, character.LocalRopeAnchorPoint));

        aspect.HandlePhysicsUpdatePhase2(ref context, ref baseContext, false, false, true, false, false);

        DetectTransitions(ref context, ref baseContext, in aspect);
    }

    public void OnStateVariableUpdate(ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        var deltaTime = baseContext.Time.DeltaTime;
        ref var character = ref aspect.Character.ValueRW;
        ref var characterControl = ref aspect.CharacterControl.ValueRW;
        ref var characterPosition = ref aspect.CharacterAspect.LocalTransform.ValueRW.Position;
        ref var characterRotation = ref aspect.CharacterAspect.LocalTransform.ValueRW.Rotation;

        if (math.lengthsq(characterControl.MoveVector) > 0f)
            CharacterControlUtilities.SlerpRotationTowardsDirectionAroundUp(ref characterRotation, deltaTime,
                math.normalizesafe(characterControl.MoveVector), MathUtilities.GetUpFromRotation(characterRotation),
                character.AirRotationSharpness);
        CharacterControlUtilities.SlerpCharacterUpTowardsDirection(ref characterRotation, deltaTime,
            math.normalizesafe(AnchorPoint - characterPosition), character.UpOrientationAdaptationSharpness);
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
        ref var characterControl = ref aspect.CharacterControl.ValueRW;
        ref var stateMachine = ref aspect.StateMachine.ValueRW;

        if (characterControl.IsJumpPressed() || characterControl.IsDashPressed())
        {
            stateMachine.TransitionToState(CharacterState.AirMove, ref context, ref baseContext, in aspect);
            return true;
        }

        return aspect.DetectGlobalTransitions(ref context, ref baseContext);
    }

    public static bool DetectRopePoints(in PhysicsWorld physicsWorld, in PlatformerCharacterAspect aspect,
        out float3 point)
    {
        point = default;

        ref var character = ref aspect.Character.ValueRW;
        ref var characterPosition = ref aspect.CharacterAspect.LocalTransform.ValueRW.Position;
        var characterRotation = aspect.CharacterAspect.LocalTransform.ValueRW.Rotation;

        var characterTransform = new RigidTransform(characterRotation, characterPosition);
        var ropeDetectionPoint = math.transform(characterTransform, character.LocalRopeAnchorPoint);

        var ropeAnchorDetectionFilter = CollisionFilter.Default;
        ropeAnchorDetectionFilter.CollidesWith = character.RopeAnchorCategory.Value;

        var pointInput = new PointDistanceInput
        {
            Filter = ropeAnchorDetectionFilter,
            MaxDistance = character.RopeLength,
            Position = ropeDetectionPoint
        };

        if (physicsWorld.CalculateDistance(pointInput, out var closestHit))
        {
            point = closestHit.Position;
            return true;
        }

        return false;
    }

    public static void ConstrainToRope(
        ref float3 translation,
        ref float3 velocity,
        float ropeLength,
        float3 ropeAnchorPoint,
        float3 ropeAnchorPointOnCharacter)
    {
        var characterToRopeVector = ropeAnchorPoint - ropeAnchorPointOnCharacter;
        var ropeNormal = math.normalizesafe(characterToRopeVector);

        if (math.length(characterToRopeVector) >= ropeLength)
        {
            var targetAnchorPointOnCharacter =
                ropeAnchorPoint - MathUtilities.ClampToMaxLength(characterToRopeVector, ropeLength);
            translation += targetAnchorPointOnCharacter - ropeAnchorPointOnCharacter;

            if (math.dot(velocity, ropeNormal) < 0f) velocity = MathUtilities.ProjectOnPlane(velocity, ropeNormal);
        }
    }
}