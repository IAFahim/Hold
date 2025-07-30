using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[BurstCompile]
public struct PlatformerCharacterControl : IComponentData
{
    public float3 MoveVector;
    public InputFlags Value;


    // Held input methods
    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool IsJumpHeld()
    {
        return (Value & InputFlags.JumpHeld) != 0;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool IsRollHeld()
    {
        return (Value & InputFlags.RollHeld) != 0;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool IsSprintHeld()
    {
        return (Value & InputFlags.SprintHeld) != 0;
    }

    // Pressed input methods
    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool IsJumpPressed()
    {
        return (Value & InputFlags.JumpPressed) != 0;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool IsDashPressed()
    {
        return (Value & InputFlags.DashPressed) != 0;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool IsCrouchPressed()
    {
        return (Value & InputFlags.CrouchPressed) != 0;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool IsRopePressed()
    {
        return (Value & InputFlags.RopePressed) != 0;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool IsClimbPressed()
    {
        return (Value & InputFlags.ClimbPressed) != 0;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool IsFlyNoCollisionsPressed()
    {
        return (Value & InputFlags.FlyNoCollisionsPressed) != 0;
    }

    // Setter methods for held inputs
    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetJumpHeld(bool held)
    {
        if (held)
            Value |= InputFlags.JumpHeld;
        else
            Value &= ~InputFlags.JumpHeld;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetRollHeld(bool held)
    {
        if (held)
            Value |= InputFlags.RollHeld;
        else
            Value &= ~InputFlags.RollHeld;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetSprintHeld(bool held)
    {
        if (held)
            Value |= InputFlags.SprintHeld;
        else
            Value &= ~InputFlags.SprintHeld;
    }

    // Setter methods for pressed inputs (these should be cleared after processing)
    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetJumpPressed(bool active)
    {
        if (active)
            Value |= InputFlags.JumpPressed;
        else
            Value &= ~InputFlags.JumpPressed;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetDashPressed(bool active)
    {
        if (active)
            Value |= InputFlags.DashPressed;
        else
            Value &= ~InputFlags.DashPressed;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetCrouchPressed(bool active)
    {
        if (active)
            Value |= InputFlags.CrouchPressed;
        else
            Value &= ~InputFlags.CrouchPressed;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetRopePressed(bool active)
    {
        if (active)
            Value |= InputFlags.RopePressed;
        else
            Value &= ~InputFlags.RopePressed;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetClimbPressed(bool active)
    {
        if (active)
            Value |= InputFlags.ClimbPressed;
        else
            Value &= ~InputFlags.ClimbPressed;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetFlyNoCollisionsPressed(bool active)
    {
        if (active)
            Value |= InputFlags.FlyNoCollisionsPressed;
        else
            Value &= ~InputFlags.FlyNoCollisionsPressed;
    }

    // Clear methods for pressed inputs (call after processing one-frame inputs)
    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearPressedInputs()
    {
        Value &= ~(InputFlags.JumpPressed | InputFlags.DashPressed | InputFlags.CrouchPressed | InputFlags.RopePressed |
                   InputFlags.ClimbPressed | InputFlags.FlyNoCollisionsPressed);
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearJumpPressed()
    {
        Value &= ~InputFlags.JumpPressed;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearDashPressed()
    {
        Value &= ~InputFlags.DashPressed;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearCrouchPressed()
    {
        Value &= ~InputFlags.CrouchPressed;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearRopePressed()
    {
        Value &= ~InputFlags.RopePressed;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearClimbPressed()
    {
        Value &= ~InputFlags.ClimbPressed;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearFlyNoCollisionsPressed()
    {
        Value &= ~InputFlags.FlyNoCollisionsPressed;
    }

    // Clear all inputs
    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearAll()
    {
        Value = InputFlags.None;
        MoveVector = float3.zero;
    }
}