using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace _src.Scripts.Easings.Easings.Data
{
    public enum Ease : byte
    {
        // ────────────── Straight diagonal line
        Linear, // /

        // ────────────── Sine Wave Based (Smooth S-curves)
        InSine, // ╰─ Slow start, normal end
        OutSine, // ─╮ Normal start, slow end  
        InOutSine, // ╰─╮ Slow start and end, fast middle

        // ────────────── Quadratic (Gentle curves)
        InQuad, // ╰── Gentle acceleration
        OutQuad, // ──╮ Gentle deceleration
        InOutQuad, // ╰─╮ Gentle ease in and out

        // ────────────── Cubic (More pronounced curves)
        InCubic, // ╰─── Moderate acceleration
        OutCubic, // ───╮ Moderate deceleration  
        InOutCubic, // ╰──╮ Moderate ease in and out

        // ────────────── Quartic (Strong curves)
        InQuart, // ╰──── Strong acceleration
        OutQuart, // ────╮ Strong deceleration
        InOutQuart, // ╰───╮ Strong ease in and out

        // ────────────── Quintic (Very strong curves)
        InQuint, // ╰───── Very strong acceleration  
        OutQuint, // ─────╮ Very strong deceleration
        InOutQuint, // ╰────╮ Very strong ease in and out

        // ────────────── Exponential (Extreme curves)
        InExpo, // ╰────── Explosive acceleration
        OutExpo, // ──────╮ Explosive deceleration
        InOutExpo, // ╰─────╮ Explosive ease in and out

        // ────────────── Circular (Quarter circle curves)
        InCirc, // ╰─────── Quarter circle acceleration
        OutCirc, // ───────╮ Quarter circle deceleration  
        InOutCirc, // ╰──────╮ Quarter circle ease in and out

        // ────────────── Elastic (Rubber band effect)
        InElastic, // ╰~~~── Pulls back then snaps forward
        OutElastic, // ──~~~╮ Overshoots then settles back
        InOutElastic, // ╰~~╮~~ Pulls back, snaps, overshoots, settles

        // ────────────── Back (Overshoots)
        InBack, // ╰\──── Pulls back slightly before moving forward
        OutBack, // ────/╮ Overshoots target then comes back
        InOutBack, // ╰\─/╮ Pulls back, overshoots, settles

        // ────────────── Bounce (Ball dropping effect)
        InBounce, // ╰.,'.─ Multiple small bounces before reaching end
        OutBounce, // ─'.,╮ Big bounce then smaller bounces at end
        InOutBounce, // ╰.,'╮ Bounces at start and end

        // ────────────── User Defined
        Custom // ????? Whatever curve you define!
    }

    
    public static class Easing
    {
        const float PI = math.PI;

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Evaluate(this Ease ease, float t)
        {
            return ease switch
            {
                Ease.InSine => InSine(t),
                Ease.OutSine => OutSine(t),
                Ease.InOutSine => InOutSine(t),
                Ease.InQuad => InQuad(t),
                Ease.OutQuad => OutQuad(t),
                Ease.InOutQuad => InOutQuad(t),
                Ease.InCubic => InCubic(t),
                Ease.OutCubic => OutCubic(t),
                Ease.InOutCubic => InOutCubic(t),
                Ease.InQuart => InQuart(t),
                Ease.OutQuart => OutQuart(t),
                Ease.InOutQuart => InOutQuart(t),
                Ease.InQuint => InQuint(t),
                Ease.OutQuint => OutQuint(t),
                Ease.InOutQuint => InOutQuint(t),
                Ease.InExpo => InExpo(t),
                Ease.OutExpo => OutExpo(t),
                Ease.InOutExpo => InOutExpo(t),
                Ease.InCirc => InCirc(t),
                Ease.OutCirc => OutCirc(t),
                Ease.InOutCirc => InOutCirc(t),
                Ease.InElastic => InElastic(t),
                Ease.OutElastic => OutElastic(t),
                Ease.InOutElastic => InOutElastic(t),
                Ease.InBack => InBack(t),
                Ease.OutBack => OutBack(t),
                Ease.InOutBack => InOutBack(t),
                Ease.InBounce => InBounce(t),
                Ease.OutBounce => OutBounce(t),
                Ease.InOutBounce => InOutBounce(t),
                Ease.Custom => 1,
                _ => t,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InSine(float t)
        {
            return 1f - math.cos((t * PI) * 0.5f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float OutSine(float t)
        {
            return math.sin((t * PI) * 0.5f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InOutSine(float t)
        {
            return -(math.cos(t * PI) - 1) * 0.5f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InQuad(float t)
        {
            return t * t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float OutQuad(float t)
        {
            return 1 - (1 - t) * (1 - t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InOutQuad(float t)
        {
            return t < 0.5f
                ? 2 * t * t
                : 1 - (math.pow(-2 * t + 2, 2) * 0.5f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InCubic(float t)
        {
            return t * t * t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float OutCubic(float t)
        {
            return 1 - math.pow(1 - t, 3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InOutCubic(float t)
        {
            return t < 0.5f
                ? 4 * t * t * t
                : 1 - math.pow(-2 * t + 2, 3) * 0.5f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InQuart(float t)
        {
            return t * t * t * t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float OutQuart(float t)
        {
            return 1 - math.pow(1 - t, 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InOutQuart(float t)
        {
            return t < 0.5f
                ? 8 * t * t * t * t
                : 1 - math.pow(-2 * t + 2, 4) * 0.5f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InQuint(float t)
        {
            return t * t * t * t * t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float OutQuint(float t)
        {
            return 1 - math.pow(1 - t, 5);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InOutQuint(float t)
        {
            return t < 0.5f
                ? 16 * t * t * t * t * t
                : 1 - math.pow(-2 * t + 2, 5) * 0.5f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InExpo(float t)
        {
            return t == 0
                ? 0
                : math.pow(2, 10 * (t - 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float OutExpo(float t)
        {
            return math.abs(t - 1) < float.Epsilon
                ? 1
                : -math.pow(2, -10 * t) + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InOutExpo(float t)
        {
            if (t == 0) return 0;
            if (math.abs(t - 1) < float.Epsilon) return 1;
            return t < 0.5f
                ? math.pow(2, 20 * t - 10) * 0.5f
                : (2 - math.pow(2, -20 * t + 10)) * 0.5f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InCirc(float t)
        {
            return 1 - math.sqrt(1 - math.pow(t, 2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float OutCirc(float t)
        {
            return math.sqrt(1 - math.pow(t - 1, 2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InOutCirc(float t)
        {
            return t < 0.5f
                ? (1 - math.sqrt(1 - math.pow(2 * t, 2))) * 0.5f
                : (math.sqrt(1 - math.pow(-2 * t + 2, 2)) + 1) * 0.5f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InElastic(float t)
        {
            if (t == 0) return 0;
            if (math.abs(t - 1) < float.Epsilon) return 1;
            return -math.sin(7.5f * PI * t) * math.pow(2, 10 * (t - 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float OutElastic(float t)
        {
            if (t == 0) return 0;
            if (math.abs(t - 1) < float.Epsilon) return 1;
            return math.sin(-7.5f * PI * (t + 1)) * math.pow(2, -10 * t) + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InOutElastic(float t)
        {
            if (t == 0) return 0;
            if (math.abs(t - 1) < float.Epsilon) return 1;
            return t < 0.5f
                ? 0.5f * math.sin(7.5f * PI * (2 * t)) * math.pow(2, 10 * (2 * t - 1))
                : 0.5f * (math.sin(-7.5f * PI * (2 * t - 1 + 1)) * math.pow(2, -10 * (2 * t - 1)) + 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InBack(float t)
        {
            return t * t * t - t * math.sin(t * PI);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float OutBack(float t)
        {
            return 1 - (math.pow(1 - t, 3) - (1 - t) * math.sin((1 - t) * PI));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InOutBack(float t)
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
        public static float InBounce(float t)
        {
            return 1 - Evaluate(Ease.OutBounce, 1 - t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float OutBounce(float t)
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
        public static float InOutBounce(float t)
        {
            return t < 0.5f
                ? (1 - InBounce(1 - 2 * t)) * 0.5f
                : (1 + OutBounce(2 * t - 1)) * 0.5f;
        }
    }
}