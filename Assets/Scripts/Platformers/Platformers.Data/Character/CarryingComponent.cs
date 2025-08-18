using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
[Serializable]
public struct CarryingComponent : IComponentData
{
    public float currentWeight;
    public float capacity;
    
    // Speed modifiers
    public float minSpeedAtMaxLoad; // 0..1, minimum multiplier at full capacity
    public float speedCurveExponent; // How sharp the speed reduction curve is (1 = linear, >1 = exponential)
    
    public float minAnimationSpeedAtMaxLoad; // Minimum animation speed when fully loaded
    public float animationSpeedCurve; // Animation speed reduction curve
    
    // Stamina/Energy system
    public float staminaDrainMultiplier; // How much faster stamina drains with weight
    public float maxStaminaDrainAtFullLoad; // Maximum stamina drain multiplier at full capacity
    
    // Balance/Sway effects
    public float balanceReduction; // How much balance is affected by weight (0-1)
    public float swayAmplitude; // Movement sway when heavily loaded
    
    // Sound effects
    public float footstepVolumeMultiplier; // Louder footsteps with more weight
    public float breathingIntensity; // Breathing gets heavier with weight

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float ComputeSpeedMultiplier()
    {
        if (capacity <= 0f) return 1f;
        
        float loadRatio = math.saturate(currentWeight / capacity);
        
        // Use exponential curve for more natural speed reduction
        float curvedRatio = math.pow(loadRatio, speedCurveExponent);
        float multiplier = 1f - curvedRatio;
        
        return math.max(multiplier, math.saturate(minSpeedAtMaxLoad));
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float ComputeAnimationSpeedMultiplier()
    {
        if (capacity <= 0f) return 1f;
        
        float loadRatio = math.saturate(currentWeight / capacity);
        float curvedRatio = math.pow(loadRatio, animationSpeedCurve);
        
        // Animation speed reduces less aggressively than movement speed
        float multiplier = math.lerp(1f, minAnimationSpeedAtMaxLoad, curvedRatio);
        
        return math.max(multiplier, 0.1f); // Never go below 10% animation speed
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float ComputeStaminaDrainMultiplier()
    {
        if (capacity <= 0f) return 1f;
        
        float loadRatio = math.saturate(currentWeight / capacity);
        
        // Stamina drain increases exponentially with weight
        return math.lerp(1f, maxStaminaDrainAtFullLoad, loadRatio * loadRatio);
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float ComputeBalanceStability()
    {
        if (capacity <= 0f) return 1f;
        
        float loadRatio = math.saturate(currentWeight / capacity);
        
        // Balance decreases linearly with weight
        return math.lerp(1f, 1f - balanceReduction, loadRatio);
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float ComputeSwayAmount()
    {
        if (capacity <= 0f) return 0f;
        
        float loadRatio = math.saturate(currentWeight / capacity);
        
        // Sway increases with weight, more noticeable when heavily loaded
        return swayAmplitude * loadRatio * loadRatio;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float ComputeFootstepVolume()
    {
        if (capacity <= 0f) return 1f;
        
        float loadRatio = math.saturate(currentWeight / capacity);
        
        // Footsteps get progressively louder
        return math.lerp(1f, footstepVolumeMultiplier, loadRatio);
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float ComputeBreathingIntensity()
    {
        if (capacity <= 0f) return 1f;
        
        float loadRatio = math.saturate(currentWeight / capacity);
        
        // Breathing intensity increases exponentially
        return math.lerp(1f, breathingIntensity, loadRatio * loadRatio);
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool IsOverloaded()
    {
        return currentWeight > capacity;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float GetLoadRatio()
    {
        return capacity > 0f ? math.saturate(currentWeight / capacity) : 0f;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float GetRemainingCapacity()
    {
        return math.max(0f, capacity - currentWeight);
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool CanCarryAdditionalWeight(float additionalWeight)
    {
        return currentWeight + additionalWeight <= capacity;
    }

    // Utility method for smooth weight transitions in animations
    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float ComputeWeightTransitionSpeed(float targetWeight, float baseTransitionSpeed)
    {
        float weightDifference = math.abs(targetWeight - currentWeight);
        float loadRatio = GetLoadRatio();
        
        // Slower transitions when heavily loaded
        float speedModifier = math.lerp(1f, 0.5f, loadRatio);
        
        return baseTransitionSpeed * speedModifier * (1f + weightDifference / capacity);
    }
}