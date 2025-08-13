using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;
using UnityEngine;
using Unity.CharacterController;
using Unity.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(PhysicsShapeAuthoring))]
public class PlatformerCharacterAuthoring : MonoBehaviour
{
    public AuthoringKinematicCharacterProperties CharacterProperties =
        AuthoringKinematicCharacterProperties.GetDefault();

    public PlatformerCharacterComponent Character = default;

    [Header("Geometry")] [SerializeField]
    private CharacterCapsuleGeometry characterCapsuleGeometry = CharacterCapsuleGeometryExt.Default();

    [Header("References")] public GameObject MeshPrefab;
    public GameObject DefaultCameraTarget;
    public GameObject SwimmingCameraTarget;
    public GameObject ClimbingCameraTarget;
    public GameObject CrouchingCameraTarget;
    public GameObject MeshRoot;
    public GameObject RollballMesh;
    public GameObject RopePrefab;
    public GameObject SwimmingDetectionPoint;
    public GameObject LedgeDetectionPoint;

    [Header("Carrying")] public bool UseCarrying = true;
    public CarryingComponent carryingComponent = CarryingComponentExt.Default();
    public float CarryingCapacity = 1f;
    public float CurrentWeight = 0;
    [Range(0f, 1f)] public float CarryingMinSpeedAtMaxLoad = 0.2f;

    [Header("Debug")] public bool DebugStandingGeometry;
    public bool DebugCrouchingGeometry;
    public bool DebugRollingGeometry;
    public bool DebugClimbingGeometry;
    public bool DebugSwimmingGeometry;

    public class Baker : Baker<PlatformerCharacterAuthoring>
    {
        public override void Bake(PlatformerCharacterAuthoring authoring)
        {
            KinematicCharacterUtilities.BakeCharacter(this, authoring, authoring.CharacterProperties);

            authoring.Character.DefaultCameraTargetEntity =
                GetEntity(authoring.DefaultCameraTarget, TransformUsageFlags.Dynamic);
            authoring.Character.SwimmingCameraTargetEntity =
                GetEntity(authoring.SwimmingCameraTarget, TransformUsageFlags.Dynamic);
            authoring.Character.ClimbingCameraTargetEntity =
                GetEntity(authoring.ClimbingCameraTarget, TransformUsageFlags.Dynamic);
            authoring.Character.CrouchingCameraTargetEntity =
                GetEntity(authoring.CrouchingCameraTarget, TransformUsageFlags.Dynamic);
            authoring.Character.MeshRootEntity = GetEntity(authoring.MeshRoot, TransformUsageFlags.Dynamic);
            authoring.Character.RollballMeshEntity = GetEntity(authoring.RollballMesh, TransformUsageFlags.Dynamic);
            authoring.Character.RopePrefabEntity = GetEntity(authoring.RopePrefab, TransformUsageFlags.Dynamic);
            authoring.Character.LocalSwimmingDetectionPoint = authoring.SwimmingDetectionPoint.transform.localPosition;
            authoring.Character.LocalLedgeDetectionPoint = authoring.LedgeDetectionPoint.transform.localPosition;

            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, authoring.Character);
            AddComponent(entity, new CapsuleGeometryBlobComponent
            {
                BlobAssetRef = authoring.characterCapsuleGeometry.CreateBlob()
            });
            AddComponent(entity, new PlatformerCharacterControl());
            AddComponent(entity, new PlatformerCharacterStateMachine());
            AddComponentObject(entity, new PlatformerCharacterHybridData { MeshPrefab = authoring.MeshPrefab });

            if (authoring.UseCarrying)
            {
                AddComponent(entity, new CarryingComponent
                {
                    capacity = authoring.CarryingCapacity,
                    minSpeedAtMaxLoad = authoring.CarryingMinSpeedAtMaxLoad,
                    currentWeight = authoring.CurrentWeight
                });
            }
        }
    }


    public static class CarryingComponentExt
    {
        public static CarryingComponent Default()
        {
            return new CarryingComponent
            {
                capacity = 1,
                currentWeight = 0,
                minSpeedAtMaxLoad = .2f
            };
        }
    }


    private void OnDrawGizmosSelected()
    {
        if (DebugStandingGeometry)
        {
            Gizmos.color = Color.cyan;
            DrawCapsuleGizmo(characterCapsuleGeometry.standing);
        }

        if (DebugCrouchingGeometry)
        {
            Gizmos.color = Color.cyan;
            DrawCapsuleGizmo(characterCapsuleGeometry.crouching);
        }

        if (DebugRollingGeometry)
        {
            Gizmos.color = Color.cyan;
            DrawCapsuleGizmo(characterCapsuleGeometry.rolling);
        }

        if (DebugClimbingGeometry)
        {
            Gizmos.color = Color.cyan;
            DrawCapsuleGizmo(characterCapsuleGeometry.climbing);
        }

        if (DebugSwimmingGeometry)
        {
            Gizmos.color = Color.cyan;
            DrawCapsuleGizmo(characterCapsuleGeometry.swimming);
        }
    }

    private void DrawCapsuleGizmo(CapsuleGeometryDefinition capsuleGeo)
    {
        var characterTransform = new RigidTransform(transform.rotation, transform.position);
        float3 characterUp = transform.up;
        float3 characterFwd = transform.forward;
        float3 characterRight = transform.right;
        var capsuleCenter = math.transform(characterTransform, capsuleGeo.Center);
        var halfHeight = capsuleGeo.Height * 0.5f;

        var bottomHemiCenter = capsuleCenter - characterUp * (halfHeight - capsuleGeo.Radius);
        var topHemiCenter = capsuleCenter + characterUp * (halfHeight - capsuleGeo.Radius);

        Gizmos.DrawWireSphere(bottomHemiCenter, capsuleGeo.Radius);
        Gizmos.DrawWireSphere(topHemiCenter, capsuleGeo.Radius);

        Gizmos.DrawLine(bottomHemiCenter + characterFwd * capsuleGeo.Radius,
            topHemiCenter + characterFwd * capsuleGeo.Radius);
        Gizmos.DrawLine(bottomHemiCenter - characterFwd * capsuleGeo.Radius,
            topHemiCenter - characterFwd * capsuleGeo.Radius);
        Gizmos.DrawLine(bottomHemiCenter + characterRight * capsuleGeo.Radius,
            topHemiCenter + characterRight * capsuleGeo.Radius);
        Gizmos.DrawLine(bottomHemiCenter - characterRight * capsuleGeo.Radius,
            topHemiCenter - characterRight * capsuleGeo.Radius);
    }
}

public static class CharacterCapsuleGeometryExt
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CharacterCapsuleGeometry Default()
    {
        return new CharacterCapsuleGeometry
        {
            standing = new CapsuleGeometryDefinition { Radius = 0.3f, Height = 1.4f, Center = new float3(0, 0.7f, 0) },
            crouching = new CapsuleGeometryDefinition { Radius = 0.3f, Height = .9f, Center = new float3(0, 0.45f, 0) },
            rolling = new CapsuleGeometryDefinition { Radius = 0.3f, Height = 0.6f, Center = new float3(0, 0.3f, 0) },
            climbing = new CapsuleGeometryDefinition { Radius = 1f, Height = 2f, Center = new float3(0, 0.7f, 0) },
            swimming = new CapsuleGeometryDefinition { Radius = 0.3f, Height = 1.4f, Center = new float3(0, 0.7f, 0) }
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BlobAssetReference<CharacterCapsuleGeometry> CreateBlob(in this CharacterCapsuleGeometry geometry)
    {
        using var builder = new BlobBuilder(Allocator.Temp);
        ref var root = ref builder.ConstructRoot<CharacterCapsuleGeometry>();
        root = geometry;
        var blobRef = builder.CreateBlobAssetReference<CharacterCapsuleGeometry>(Allocator.Persistent);
        return blobRef;
    }
}