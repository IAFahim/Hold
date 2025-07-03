using System;
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
    
    
    public enum EWrapMode
    {
        /// <summary>Plays the curve once and then clamps at the end.</summary>
        Clamp,
        /// <summary>Restarts the curve from the beginning after it finishes.</summary>
        Loop,
        /// <summary>Plays the curve forward, then backward, then forward, etc.</summary>
        PingPong
    }

    /// <summary>
    /// A high-performance, Burst-compatible struct for evaluating easing functions.
    /// Combines a base ease type with optional wrapping modes (Loop, PingPong).
    /// </summary>
    [BurstCompile]
    public struct Ease
    {
        /// <summary>
        /// The raw byte value storing the ease type and wrap mode flags.
        /// </summary>
        public byte Value;

        #region Internal Constants & Flags

        // --- INTERNAL ease type values (lower 5 bits) ---
        private const byte _Linear = 0;
        private const byte _InSine = 1;
        private const byte _OutSine = 2;
        private const byte _InOutSine = 3;
        private const byte _InQuad = 4;
        private const byte _OutQuad = 5;
        private const byte _InOutQuad = 6;
        private const byte _InCubic = 7;
        private const byte _OutCubic = 8;
        private const byte _InOutCubic = 9;
        private const byte _InQuart = 10;
        private const byte _OutQuart = 11;
        private const byte _InOutQuart = 12;
        private const byte _InQuint = 13;
        private const byte _OutQuint = 14;
        private const byte _InOutQuint = 15;
        private const byte _InExpo = 16;
        private const byte _OutExpo = 17;
        private const byte _InOutExpo = 18;
        private const byte _InCirc = 19;
        private const byte _OutCirc = 20;
        private const byte _InOutCirc = 21;
        private const byte _InElastic = 22;
        private const byte _OutElastic = 23;
        private const byte _InOutElastic = 24;
        private const byte _InBack = 25;
        private const byte _OutBack = 26;
        private const byte _InOutBack = 27;
        private const byte _InBounce = 28;
        private const byte _OutBounce = 29;
        private const byte _InOutBounce = 30;
        private const byte _Custom = 31;

        // --- PUBLIC modifier flags (upper 3 bits) ---
        private const byte EaseMask = 0x1F; // 00011111
        private const byte WrapModeShift = 5;
        public const byte Loop = 1 << WrapModeShift; // 00100000 (32)
        public const byte PingPong = 2 << WrapModeShift; // 01000000 (64)

        #endregion
        

        #region Constructors & Operators

        private Ease(byte value) => Value = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ease ToEase(EEase ease, bool loop, bool pingPong)
        {
            byte value = (byte)ease;

            // PingPong is a more specific behavior, so it gets priority over a simple loop.
            if (pingPong)
            {
                value |= PingPong;
            }
            else if (loop)
            {
                value |= Loop;
            }
    
            return new Ease(value);
        }
        
        // This replaces the old ToEase method inside your Ease struct.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ease ToEase(EEase ease, EWrapMode wrapMode)
        {
            // Start with the base ease value.
            byte value = (byte)ease;

            // Add the correct flag based on the wrap mode.
            switch (wrapMode)
            {
                case EWrapMode.Loop:
                    value |= Loop;
                    break;
                case EWrapMode.PingPong:
                    value |= PingPong;
                    break;
                // For EWrapMode.Clamp, we do nothing, as that's the default (no flags).
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
        private readonly bool IsLoop() => (Value & Loop) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly bool GetIsPingPong() => (Value & PingPong) != 0;

        #endregion

        #region Evaluation Logic

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void E(float t, out float easedT)
        {
            float wrappedT;
            if (GetIsPingPong()) wrappedT = ApplyPingPong(t);
            else if (IsLoop()) wrappedT = ApplyLoop(t);
            else wrappedT = math.clamp(t, 0f, 1f);

            Evaluate(wrappedT, out easedT);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void Evaluate(float t, out float easedT)
        {
            var baseEaseValue = Value & EaseMask;
            easedT = baseEaseValue switch
            {
                _InSine => InSine(t),
                _OutSine => OutSine(t),
                _InOutSine => InOutSine(t),
                _InQuad => InQuad(t),
                _OutQuad => OutQuad(t),
                _InOutQuad => InOutQuad(t),
                _InCubic => InCubic(t),
                _OutCubic => OutCubic(t),
                _InOutCubic => InOutCubic(t),
                _InQuart => InQuart(t),
                _OutQuart => OutQuart(t),
                _InOutQuart => InOutQuart(t),
                _InQuint => InQuint(t),
                _OutQuint => OutQuint(t),
                _InOutQuint => InOutQuint(t),
                _InExpo => InExpo(t),
                _OutExpo => OutExpo(t),
                _InOutExpo => InOutExpo(t),
                _InCirc => InCirc(t),
                _OutCirc => OutCirc(t),
                _InOutCirc => InOutCirc(t),
                _InElastic => InElastic(t),
                _OutElastic => OutElastic(t),
                _InOutElastic => InOutElastic(t),
                _InBack => InBack(t),
                _OutBack => OutBack(t),
                _InOutBack => InOutBack(t),
                _InBounce => InBounce(t),
                _OutBounce => OutBounce(t),
                _InOutBounce => InOutBounce(t),
                _Custom => 1f,
                _ => t, // _Linear (default)
            };
        }

        #endregion

        #region Private Easing Functions

        private const float PI = math.PI;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ApplyLoop(float t) => t - math.floor(t);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ApplyPingPong(float t)
        {
            float wrapped = ApplyLoop(t);
            return ((int)math.floor(t) % 2 == 0) ? wrapped : 1f - wrapped;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InSine(float t) => 1f - math.cos((t * PI) * 0.5f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float OutSine(float t) => math.sin((t * PI) * 0.5f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InOutSine(float t) => -(math.cos(t * PI) - 1) * 0.5f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InQuad(float t) => t * t;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float OutQuad(float t) => 1 - (1 - t) * (1 - t);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InOutQuad(float t) => t < 0.5f ? 2 * t * t : 1 - (math.pow(-2 * t + 2, 2) * 0.5f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InCubic(float t) => t * t * t;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float OutCubic(float t) => 1 - math.pow(1 - t, 3);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InOutCubic(float t) => t < 0.5f ? 4 * t * t * t : 1 - math.pow(-2 * t + 2, 3) * 0.5f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InQuart(float t) => t * t * t * t;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float OutQuart(float t) => 1 - math.pow(1 - t, 4);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InOutQuart(float t) => t < 0.5f ? 8 * t * t * t * t : 1 - math.pow(-2 * t + 2, 4) * 0.5f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InQuint(float t) => t * t * t * t * t;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float OutQuint(float t) => 1 - math.pow(1 - t, 5);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InOutQuint(float t) =>
            t < 0.5f ? 16 * t * t * t * t * t : 1 - math.pow(-2 * t + 2, 5) * 0.5f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InExpo(float t) => t == 0 ? 0 : math.pow(2, 10 * (t - 1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float OutExpo(float t) => math.abs(t - 1) < float.Epsilon ? 1 : -math.pow(2, -10 * t) + 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InOutExpo(float t)
        {
            if (t == 0) return 0;
            if (math.abs(t - 1) < float.Epsilon) return 1;
            return t < 0.5f ? math.pow(2, 20 * t - 10) * 0.5f : (2 - math.pow(2, -20 * t + 10)) * 0.5f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InCirc(float t) => 1 - math.sqrt(1 - math.pow(t, 2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float OutCirc(float t) => math.sqrt(1 - math.pow(t - 1, 2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InOutCirc(float t) => t < 0.5f
            ? (1 - math.sqrt(1 - math.pow(2 * t, 2))) * 0.5f
            : (math.sqrt(1 - math.pow(-2 * t + 2, 2)) + 1) * 0.5f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InElastic(float t)
        {
            if (t == 0) return 0;
            if (math.abs(t - 1) < float.Epsilon) return 1;
            return -math.sin(7.5f * PI * t) * math.pow(2, 10 * (t - 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float OutElastic(float t)
        {
            if (t == 0) return 0;
            if (math.abs(t - 1) < float.Epsilon) return 1;
            return math.sin(-7.5f * PI * (t + 1)) * math.pow(2, -10 * t) + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InOutElastic(float t)
        {
            if (t == 0) return 0;
            if (math.abs(t - 1) < float.Epsilon) return 1;
            return t < 0.5f
                ? 0.5f * math.sin(7.5f * PI * (2 * t)) * math.pow(2, 10 * (2 * t - 1))
                : 0.5f * (math.sin(-7.5f * PI * (2 * t - 1 + 1)) * math.pow(2, -10 * (2 * t - 1)) + 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InBack(float t) => t * t * t - t * math.sin(t * PI);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float OutBack(float t) => 1 - (math.pow(1 - t, 3) - (1 - t) * math.sin((1 - t) * PI));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InOutBack(float t)
        {
            if (t < 0.5f)
            {
                float f = 2 * t;
                return 0.5f * (f * f * f - f * math.sin(f * PI));
            }
            else
            {
                float f = 1 - (2 * t - 1);
                return 0.5f * (1 - (f * f * f - f * math.sin(f * PI))) + 0.5f;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InBounce(float t) => 1 - OutBounce(1 - t);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float OutBounce(float t)
        {
            if (t < 1 / 2.75f)
            {
                return 7.5625f * t * t;
            }
            else if (t < 2 / 2.75f)
            {
                return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f;
            }
            else if (t < 2.5 / 2.75f)
            {
                return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f;
            }
            else
            {
                return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InOutBounce(float t) =>
            t < 0.5f ? InBounce(t * 2) * 0.5f : OutBounce(t * 2 - 1) * 0.5f + 0.5f;

        #endregion
    }
}