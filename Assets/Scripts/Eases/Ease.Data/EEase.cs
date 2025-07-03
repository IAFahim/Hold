namespace Eases.Ease.Data
{
    /// <summary>
    /// Enum representation of easing functions. Useful for selecting an ease type in the Unity Inspector.
    /// </summary>
    public enum EEase : byte
    {
        // ────────────── Straight diagonal line
        Linear = 0, // /

        // ────────────── Sine Wave Based (Smooth S-curves)
        InSine = 1, // ╰─ Slow start, normal end
        OutSine = 2, // ─╮ Normal start, slow end  
        InOutSine = 3, // ╰─╮ Slow start and end, fast middle

        // ────────────── Quadratic (Gentle curves)
        InQuad = 4, // ╰── Gentle acceleration
        OutQuad = 5, // ──╮ Gentle deceleration
        InOutQuad = 6, // ╰─╮ Gentle ease in and out

        // ────────────── Cubic (More pronounced curves)
        InCubic = 7, // ╰─── Moderate acceleration
        OutCubic = 8, // ───╮ Moderate deceleration  
        InOutCubic = 9, // ╰──╮ Moderate ease in and out

        // ────────────── Quartic (Strong curves)
        InQuart = 10, // ╰──── Strong acceleration
        OutQuart = 11, // ────╮ Strong deceleration
        InOutQuart = 12, // ╰───╮ Strong ease in and out

        // ────────────── Quintic (Very strong curves)
        InQuint = 13, // ╰───── Very strong acceleration  
        OutQuint = 14, // ─────╮ Very strong deceleration
        InOutQuint = 15, // ╰────╮ Very strong ease in and out

        // ────────────── Exponential (Extreme curves)
        InExpo = 16, // ╰────── Explosive acceleration
        OutExpo = 17, // ──────╮ Explosive deceleration
        InOutExpo = 18, // ╰─────╮ Explosive ease in and out

        // ────────────── Circular (Quarter circle curves)
        InCirc = 19, // ╰─────── Quarter circle acceleration
        OutCirc = 20, // ───────╮ Quarter circle deceleration  
        InOutCirc = 21, // ╰──────╮ Quarter circle ease in and out

        // ────────────── Elastic (Rubber band effect)
        InElastic = 22, // ╰~~~── Pulls back then snaps forward
        OutElastic = 23, // ──~~~╮ Overshoots then settles back
        InOutElastic = 24, // ╰~~╮~~ Pulls back, snaps, overshoots, settles

        // ────────────── Back (Overshoots)
        InBack = 25, // ╰\──── Pulls back slightly before moving forward
        OutBack = 26, // ────/╮ Overshoots target then comes back
        InOutBack = 27, // ╰\─/╮ Pulls back, overshoots, settles

        // ────────────── Bounce (Ball dropping effect)
        InBounce = 28, // ╰.,'.─ Multiple small bounces before reaching end
        OutBounce = 29, // ─'.,╮ Big bounce then smaller bounces at end
        InOutBounce = 30, // ╰.,'╮ Bounces at start and end

        // ────────────── User Defined
        Custom = 31, // ????? Whatever curve you define!
    }
}