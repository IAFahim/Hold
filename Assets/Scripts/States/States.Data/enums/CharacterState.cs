namespace States.States.Data.enums
{
    public enum ECharacterState : byte
    {
        Uninitialized,

        GroundMove,
        Crouched,
        AirMove,
        WallRun,
        Rolling,
        LedgeGrab,
        LedgeStandingUp,
        Dashing,
        Swimming,
        Climbing,
        FlyingNoCollisions,
        RopeSwing,
        Sliding
    }
}