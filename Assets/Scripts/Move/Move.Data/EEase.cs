using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Move.Move.Data
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
    
    
        /// <summary>
    /// Enum for wrapping modes. The values are explicitly set to match our bit-packing scheme.
    /// This defines how time is handled outside the 0-1 range.
    /// </summary>
    public enum EWrapMode : byte
    {
        /// <summary>Plays the curve once and then clamps at the end. (Value: 0)</summary>
        Clamp = 0,
        /// <summary>Restarts the curve from the beginning after it finishes. (Value: 1)</summary>
        Loop = 1,
        /// <summary>Plays the curve forward, then backward, then forward, etc. (Value: 2)</summary>
        PingPong = 2
    }

    /// <summary>
    /// A high-performance, Burst-compatible struct for evaluating easing functions.
    /// It efficiently packs the ease type, wrap mode, and a reversed flag into a single byte.
    /// Layout: [R|WW|EEEEE] (Bit 7: Reversed | Bits 6-5: WrapMode | Bits 4-0: EaseType)
    /// </summary>
    [BurstCompile]
    public struct Ease
    {
        /// <summary>
        /// The raw byte value storing the ease type and modifier flags.
        /// </summary>
        public byte Value;

        #region Bit-packing Constants & Masks

        // --- Ease Type (lower 5 bits) ---
        private const byte EaseMask = 0x1F; // 00011111

        // --- Wrap Mode (bits 5 and 6) ---
        private const int WrapModeShift = 5;
        private const byte WrapModeMask = 0x60; // 01100000

        // --- Reversed Flag (bit 7) ---
        public const byte ReversedFlag = 0x80; // 10000000

        #endregion
        
        #region Constructors & Operators

        private Ease(byte value) => Value = value;

        /// <summary>
        /// Creates an Ease struct from its constituent parts. This is the recommended factory method.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ease ToEase(EEase ease, EWrapMode wrapMode = EWrapMode.Clamp, bool reversed = false)
        {
            byte value = (byte)ease;
            value |= (byte)((byte)wrapMode << WrapModeShift);
            if (reversed)
            {
                value |= ReversedFlag;
            }
            return new Ease(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ease operator |(Ease ease, byte flag) => new Ease { Value = (byte)(ease.Value | flag) };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator EEase(Ease ease) => (EEase)(ease.Value & EaseMask);

        #endregion

        #region Properties

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly EEase BaseEase() => (EEase)(Value & EaseMask);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly EWrapMode WrapMode() => (EWrapMode)((Value & WrapModeMask) >> WrapModeShift);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsReversed() => (Value & ReversedFlag) != 0;

        #endregion

        #region Evaluation Logic

        /// <summary>
        /// The core evaluation method. Applies wrapping, reversal, and the base easing function.
        /// </summary>
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void E(float t, out float easedT)
        {
            // 1. Apply wrapping mode to the input time `t`.
            float wrappedT;
            switch (WrapMode())
            {
                case EWrapMode.Loop:
                    wrappedT = ApplyLoop(t);
                    break;
                case EWrapMode.PingPong:
                    wrappedT = ApplyPingPong(t);
                    break;
                default: // EWrapMode.Clamp
                    wrappedT = math.clamp(t, 0f, 1f);
                    break;
            }

            // 2. Determine if the curve shape should be reversed.
            bool reversed = IsReversed();
            float timeToEvaluate = reversed ? 1f - wrappedT : wrappedT;

            // 3. Evaluate the base easing function.
            Evaluate(timeToEvaluate, out float evaluatedResult);

            // 4. Apply the reversal transformation to the output if necessary.
            easedT = reversed ? 1f - evaluatedResult : evaluatedResult;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void Evaluate(float t, out float easedT)
        {
            var baseEaseValue = Value & EaseMask;
            easedT = baseEaseValue switch
            {
                (byte)EEase.InSine => InSine(t),
                (byte)EEase.OutSine => OutSine(t),
                (byte)EEase.InOutSine => InOutSine(t),
                (byte)EEase.InQuad => InQuad(t),
                (byte)EEase.OutQuad => OutQuad(t),
                (byte)EEase.InOutQuad => InOutQuad(t),
                (byte)EEase.InCubic => InCubic(t),
                (byte)EEase.OutCubic => OutCubic(t),
                (byte)EEase.InOutCubic => InOutCubic(t),
                (byte)EEase.InQuart => InQuart(t),
                (byte)EEase.OutQuart => OutQuart(t),
                (byte)EEase.InOutQuart => InOutQuart(t),
                (byte)EEase.InQuint => InQuint(t),
                (byte)EEase.OutQuint => OutQuint(t),
                (byte)EEase.InOutQuint => InOutQuint(t),
                (byte)EEase.InExpo => InExpo(t),
                (byte)EEase.OutExpo => OutExpo(t),
                (byte)EEase.InOutExpo => InOutExpo(t),
                (byte)EEase.InCirc => InCirc(t),
                (byte)EEase.OutCirc => OutCirc(t),
                (byte)EEase.InOutCirc => InOutCirc(t),
                (byte)EEase.InElastic => InElastic(t),
                (byte)EEase.OutElastic => OutElastic(t),
                (byte)EEase.InOutElastic => InOutElastic(t),
                (byte)EEase.InBack => InBack(t),
                (byte)EEase.OutBack => OutBack(t),
                (byte)EEase.InOutBack => InOutBack(t),
                (byte)EEase.InBounce => InBounce(t),
                (byte)EEase.OutBounce => OutBounce(t),
                (byte)EEase.InOutBounce => InOutBounce(t),
                (byte)EEase.Custom => 1f,
                _ => t, // EEase.Linear (default)
            };
        }

        #endregion

        #region Private Easing Functions
        private const float PI = math.PI;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float ApplyLoop(float t) => t - math.floor(t);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float ApplyPingPong(float t) { float wrapped = ApplyLoop(t); return ((int)math.floor(t) % 2 == 0) ? wrapped : 1f - wrapped; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float InSine(float t) => 1f - math.cos((t * PI) * 0.5f);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float OutSine(float t) => math.sin((t * PI) * 0.5f);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float InOutSine(float t) => -(math.cos(t * PI) - 1) * 0.5f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float InQuad(float t) => t * t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float OutQuad(float t) => 1 - (1 - t) * (1 - t);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float InOutQuad(float t) => t < 0.5f ? 2 * t * t : 1 - (math.pow(-2 * t + 2, 2) * 0.5f);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float InCubic(float t) => t * t * t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float OutCubic(float t) => 1 - math.pow(1 - t, 3);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float InOutCubic(float t) => t < 0.5f ? 4 * t * t * t : 1 - math.pow(-2 * t + 2, 3) * 0.5f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float InQuart(float t) => t * t * t * t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float OutQuart(float t) => 1 - math.pow(1 - t, 4);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float InOutQuart(float t) => t < 0.5f ? 8 * t * t * t * t : 1 - math.pow(-2 * t + 2, 4) * 0.5f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float InQuint(float t) => t * t * t * t * t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float OutQuint(float t) => 1 - math.pow(1 - t, 5);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float InOutQuint(float t) => t < 0.5f ? 16 * t * t * t * t * t : 1 - math.pow(-2 * t + 2, 5) * 0.5f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float InExpo(float t) => t == 0 ? 0 : math.pow(2, 10 * (t - 1));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float OutExpo(float t) => math.abs(t - 1) < float.Epsilon ? 1 : -math.pow(2, -10 * t) + 1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float InOutExpo(float t) { if (t == 0) return 0; if (math.abs(t - 1) < float.Epsilon) return 1; return t < 0.5f ? math.pow(2, 20 * t - 10) * 0.5f : (2 - math.pow(2, -20 * t + 10)) * 0.5f; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float InCirc(float t) => 1 - math.sqrt(1 - math.pow(t, 2));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float OutCirc(float t) => math.sqrt(1 - math.pow(t - 1, 2));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float InOutCirc(float t) => t < 0.5f ? (1 - math.sqrt(1 - math.pow(2 * t, 2))) * 0.5f : (math.sqrt(1 - math.pow(-2 * t + 2, 2)) + 1) * 0.5f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float InElastic(float t) { if (t == 0) return 0; if (math.abs(t - 1) < float.Epsilon) return 1; return -math.sin(7.5f * PI * t) * math.pow(2, 10 * (t - 1)); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float OutElastic(float t) { if (t == 0) return 0; if (math.abs(t - 1) < float.Epsilon) return 1; return math.sin(-7.5f * PI * (t + 1)) * math.pow(2, -10 * t) + 1; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float InOutElastic(float t) { if (t == 0) return 0; if (math.abs(t - 1) < float.Epsilon) return 1; return t < 0.5f ? 0.5f * math.sin(7.5f * PI * (2 * t)) * math.pow(2, 10 * (2 * t - 1)) : 0.5f * (math.sin(-7.5f * PI * (2 * t - 1 + 1)) * math.pow(2, -10 * (2 * t - 1)) + 2); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float InBack(float t) => t * t * t - t * math.sin(t * PI);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float OutBack(float t) => 1 - (math.pow(1 - t, 3) - (1 - t) * math.sin((1 - t) * PI));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float InOutBack(float t) { if (t < 0.5f) { float f = 2 * t; return 0.5f * (f * f * f - f * math.sin(f * PI)); } else { float f = 1 - (2 * t - 1); return 0.5f * (1 - (f * f * f - f * math.sin(f * PI))) + 0.5f; } }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float InBounce(float t) => 1 - OutBounce(1 - t);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float OutBounce(float t) { if (t < 1 / 2.75f) { return 7.5625f * t * t; } else if (t < 2 / 2.75f) { return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f; } else if (t < 2.5 / 2.75f) { return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f; } else { return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f; } }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static float InOutBounce(float t) => t < 0.5f ? InBounce(t * 2) * 0.5f : OutBounce(t * 2 - 1) * 0.5f + 0.5f;
        #endregion
    }
}