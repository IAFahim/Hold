using Unity.Entities;
using UnityEngine;

[System.Serializable]
public class CarryingComponentAuthoring : MonoBehaviour
{
    [Header("Capacity Settings")]
    [SerializeField, Range(1f, 1000f)]
    [Tooltip("Maximum weight this entity can carry")]
    public float capacity = 50f;
    
    [SerializeField, Range(0f, 100f)]
    [Tooltip("Current weight being carried")]
    public float startingWeight = 0f;
    
    [Header("Movement Speed")]
    [SerializeField, Range(0.1f, 1f)]
    [Tooltip("Minimum speed multiplier when fully loaded (0.3 = 30% of normal speed)")]
    public float minSpeedAtMaxLoad = 0.3f;
    
    [SerializeField, Range(0.5f, 3f)]
    [Tooltip("Speed reduction curve sharpness (1=linear, 2=quadratic, higher=more dramatic)")]
    public float speedCurveExponent = 1.8f;
    
    [Header("Animation")]
    [SerializeField, Range(0.5f, 2f)]
    [Tooltip("Base animation playback speed")]
    public float baseAnimationSpeed = 1f;
    
    [SerializeField, Range(0.2f, 1f)]
    [Tooltip("Minimum animation speed when fully loaded")]
    public float minAnimationSpeedAtMaxLoad = 0.65f;
    
    [SerializeField, Range(0.8f, 2.5f)]
    [Tooltip("Animation speed reduction curve")]
    public float animationSpeedCurve = 1.3f;
    
    [Header("Stamina & Energy")]
    [SerializeField, Range(1f, 5f)]
    [Tooltip("Base stamina drain multiplier from carrying weight")]
    public float staminaDrainMultiplier = 1.5f;
    
    [SerializeField, Range(1.5f, 8f)]
    [Tooltip("Maximum stamina drain when fully loaded")]
    public float maxStaminaDrainAtFullLoad = 4f;
    
    [Header("Balance & Movement Feel")]
    [SerializeField, Range(0f, 0.8f)]
    [Tooltip("How much balance/stability is reduced by weight (0.5 = 50% less stable when full)")]
    public float balanceReduction = 0.4f;
    
    [SerializeField, Range(0f, 2f)]
    [Tooltip("Movement sway amplitude when heavily loaded")]
    public float swayAmplitude = 0.8f;
    
    [Header("Audio Effects")]
    [SerializeField, Range(1f, 3f)]
    [Tooltip("Footstep volume multiplier at full load")]
    public float footstepVolumeMultiplier = 2.2f;
    
    [SerializeField, Range(1f, 4f)]
    [Tooltip("Breathing intensity at full load")]
    public float breathingIntensity = 2.8f;
    
    [Header("Preset Configurations")]
    [SerializeField] public CarryingPreset preset = CarryingPreset.Balanced;
    
    public enum CarryingPreset
    {
        Custom,
        LightAndAgile,      // Courier, scout
        Balanced,           // Standard soldier
        HeavyLifter,        // Pack mule, strong character
        Realistic,          // More punishing, simulation-like
        Arcade             // Forgiving, action-game feel
    }
    
    private void OnValidate()
    {
        if (preset != CarryingPreset.Custom)
        {
            ApplyPreset(preset);
        }
        
        // Ensure logical constraints
        minSpeedAtMaxLoad = Mathf.Clamp(minSpeedAtMaxLoad, 0.1f, 1f);
        minAnimationSpeedAtMaxLoad = Mathf.Clamp(minAnimationSpeedAtMaxLoad, 0.2f, 1f);
        startingWeight = Mathf.Clamp(startingWeight, 0f, capacity);
    }
    
    private void ApplyPreset(CarryingPreset presetType)
    {
        switch (presetType)
        {
            case CarryingPreset.LightAndAgile:
                capacity = 25f;
                minSpeedAtMaxLoad = 0.6f;
                speedCurveExponent = 1.3f;
                minAnimationSpeedAtMaxLoad = 0.8f;
                animationSpeedCurve = 1.1f;
                staminaDrainMultiplier = 1.2f;
                maxStaminaDrainAtFullLoad = 2.5f;
                balanceReduction = 0.2f;
                swayAmplitude = 0.4f;
                footstepVolumeMultiplier = 1.6f;
                breathingIntensity = 1.8f;
                break;
                
            case CarryingPreset.Balanced:
                capacity = 50f;
                minSpeedAtMaxLoad = 0.3f;
                speedCurveExponent = 1.8f;
                minAnimationSpeedAtMaxLoad = 0.65f;
                animationSpeedCurve = 1.3f;
                staminaDrainMultiplier = 1.5f;
                maxStaminaDrainAtFullLoad = 4f;
                balanceReduction = 0.4f;
                swayAmplitude = 0.8f;
                footstepVolumeMultiplier = 2.2f;
                breathingIntensity = 2.8f;
                break;
                
            case CarryingPreset.HeavyLifter:
                capacity = 100f;
                minSpeedAtMaxLoad = 0.15f;
                speedCurveExponent = 2.2f;
                minAnimationSpeedAtMaxLoad = 0.5f;
                animationSpeedCurve = 1.6f;
                staminaDrainMultiplier = 2f;
                maxStaminaDrainAtFullLoad = 6f;
                balanceReduction = 0.6f;
                swayAmplitude = 1.2f;
                footstepVolumeMultiplier = 3f;
                breathingIntensity = 4f;
                break;
                
            case CarryingPreset.Realistic:
                capacity = 35f;
                minSpeedAtMaxLoad = 0.2f;
                speedCurveExponent = 2.5f;
                minAnimationSpeedAtMaxLoad = 0.4f;
                animationSpeedCurve = 2f;
                staminaDrainMultiplier = 2.5f;
                maxStaminaDrainAtFullLoad = 8f;
                balanceReduction = 0.7f;
                swayAmplitude = 1.5f;
                footstepVolumeMultiplier = 2.8f;
                breathingIntensity = 4f;
                break;
                
            case CarryingPreset.Arcade:
                capacity = 75f;
                minSpeedAtMaxLoad = 0.7f;
                speedCurveExponent = 1.2f;
                minAnimationSpeedAtMaxLoad = 0.85f;
                animationSpeedCurve = 1.1f;
                staminaDrainMultiplier = 1.1f;
                maxStaminaDrainAtFullLoad = 2f;
                balanceReduction = 0.2f;
                swayAmplitude = 0.3f;
                footstepVolumeMultiplier = 1.4f;
                breathingIntensity = 1.5f;
                break;
        }
    }
    
    [Header("Debug Visualization")]
    [SerializeField] private bool showDebugInfo = true;
    
    private void OnDrawGizmosSelected()
    {
        if (!showDebugInfo) return;
        
        var component = new CarryingComponent
        {
            currentWeight = startingWeight,
            capacity = capacity,
            minSpeedAtMaxLoad = minSpeedAtMaxLoad,
            speedCurveExponent = speedCurveExponent,
            minAnimationSpeedAtMaxLoad = minAnimationSpeedAtMaxLoad,
            animationSpeedCurve = animationSpeedCurve,
            staminaDrainMultiplier = staminaDrainMultiplier,
            maxStaminaDrainAtFullLoad = maxStaminaDrainAtFullLoad,
            balanceReduction = balanceReduction,
            swayAmplitude = swayAmplitude,
            footstepVolumeMultiplier = footstepVolumeMultiplier,
            breathingIntensity = breathingIntensity
        };
        
        // Draw capacity visualization
        Gizmos.color = Color.green;
        float loadRatio = component.GetLoadRatio();
        Vector3 pos = transform.position + Vector3.up * 2f;
        
        // Capacity bar
        Gizmos.DrawWireCube(pos, new Vector3(2f, 0.2f, 0.1f));
        Gizmos.color = Color.Lerp(Color.green, Color.red, loadRatio);
        Gizmos.DrawCube(pos - Vector3.right * (1f - loadRatio), 
                       new Vector3(2f * loadRatio, 0.18f, 0.08f));
        
        // Speed indicator
        float speedMult = component.ComputeSpeedMultiplier();
        Gizmos.color = Color.Lerp(Color.red, Color.green, speedMult);
        Gizmos.DrawWireSphere(pos + Vector3.up * 0.5f, 0.2f * speedMult);
    }
}

public class CarryingComponentBaker : Baker<CarryingComponentAuthoring>
{
    public override void Bake(CarryingComponentAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        
        AddComponent(entity, new CarryingComponent
        {
            currentWeight = authoring.startingWeight,
            capacity = authoring.capacity,
            minSpeedAtMaxLoad = authoring.minSpeedAtMaxLoad,
            speedCurveExponent = authoring.speedCurveExponent,
            minAnimationSpeedAtMaxLoad = authoring.minAnimationSpeedAtMaxLoad,
            animationSpeedCurve = authoring.animationSpeedCurve,
            staminaDrainMultiplier = authoring.staminaDrainMultiplier,
            maxStaminaDrainAtFullLoad = authoring.maxStaminaDrainAtFullLoad,
            balanceReduction = authoring.balanceReduction,
            swayAmplitude = authoring.swayAmplitude,
            footstepVolumeMultiplier = authoring.footstepVolumeMultiplier,
            breathingIntensity = authoring.breathingIntensity
        });
    }
}