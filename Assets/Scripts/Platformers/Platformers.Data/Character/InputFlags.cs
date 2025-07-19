using System;

[Flags]
public enum InputFlags : ushort
{
    None = 0,
        
    // Held inputs (continuous)
    JumpHeld = 0b0000_0000_0000_0001,
    RollHeld = 0b0000_0000_0000_0010,
    SprintHeld = 0b0000_0000_0000_0100,
        
    // Pressed inputs (one-frame)
    JumpPressed = 0b0000_0000_0000_1000,
    DashPressed = 0b0000_0000_0001_0000,
    CrouchPressed = 0b0000_0000_0010_0000,
    RopePressed = 0b0000_0000_0100_0000,
    ClimbPressed = 0b0000_0000_1000_0000,
    FlyNoCollisionsPressed = 0b0000_0001_0000_0000,
}