using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using Random = UnityEngine.Random;

#region Animation Enum (The Full Library)

public enum IconAnimationType
{
    // Foundational & Foundational
    ElasticPop,
    Shake,
    Heartbeat,
    Tada,
    DropInAndBounce,
    WindUpAndSpin,
    FlickAndSettle,
    JellyBounce,
    Slingshot,
    Unfold,

    // Physical & Material
    SqueezeAndPop,
    Flip3D,
    PendulumSwing,
    RippleOut,
    PaperToss,
    NewtonsCradle,
    LiquidFill,
    Shutter,
    OrigamiFold,
    Magnetize,

    // Organic & Playful
    Wiggle,
    PeekABoo,
    Nod,
    Blink,
    TailWag,
    Gasp,
    Crawl,
    Float,
    Scurry,
    Exhale,

    // Celebratory & Emphatic
    Fireworks,
    Spotlight,
    ConfettiPop,
    Crown,
    Shockwave,
    Stomp,
    VictoryRise,
    Applause,
    PowerUp,
    Fanfare,

    // Digital & Futuristic
    ScanlineReveal,
    Glitch,
    ChargingPulse,
    Pixelate,
    CodeMatrix,
    Hologram,
    FocusLock,
    DataStream,
    Reboot,
    Voxelize,

    // Additional Animations
    RubberBand,
    Swing,
    Wobble,
    Flash,
    Pulse,
    Bounce,
    Jello,
    LightSpeedIn,
    RollIn,
    RotateIn,
    SlideInLeft,
    SlideInRight,
    SlideInUp,
    SlideInDown,
    ZoomIn,
    KineticChain,
    FlywheelSpin
}

#endregion

public class BottomNavBarController : MonoBehaviour
{
    public UIDocument uiDocument;
    private VisualElement root, selectionIndicator;
    private List<Button> navButtons;
    private List<VisualElement> navIcons;
    private int currentIndex = -1;
    private Coroutine currentAnimation;
    private Dictionary<IconAnimationType, Func<VisualElement, Coroutine>> animationLibrary;
    private List<IconAnimationType> animationKeys;
    private int _currentAnimationIndex = 0;

    private const string SELECTED_ICON_CLASS = "nav-button__icon--selected";

    void OnEnable()
    {
        if (uiDocument == null) return;
        InitializeAnimationLibrary();
        root = uiDocument.rootVisualElement;
        selectionIndicator = root.Q<VisualElement>("SelectionIndicator");
        navButtons = root.Query<Button>(className: "nav-button").ToList();
        navIcons = root.Query<VisualElement>(className: "nav-button__icon").ToList();
        for (int i = 0; i < navButtons.Count; i++)
        {
            int index = i;
            navButtons[i].RegisterCallback<ClickEvent>(evt => SelectButton(index));
        }

        root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    #region Core Logic

    private void InitializeAnimationLibrary()
    {
        animationLibrary = new Dictionary<IconAnimationType, Func<VisualElement, Coroutine>>
        {
            // Foundational
            { IconAnimationType.ElasticPop, icon => StartCoroutine(Animate_ElasticPop(icon)) },
            { IconAnimationType.Shake, icon => StartCoroutine(Animate_Shake(icon)) },
            { IconAnimationType.Heartbeat, icon => StartCoroutine(Animate_Heartbeat(icon)) },
            { IconAnimationType.Tada, icon => StartCoroutine(Animate_Tada(icon)) },
            { IconAnimationType.DropInAndBounce, icon => StartCoroutine(Animate_DropInAndBounce(icon)) },
            { IconAnimationType.WindUpAndSpin, icon => StartCoroutine(Animate_WindUpAndSpin(icon)) },
            { IconAnimationType.FlickAndSettle, icon => StartCoroutine(Animate_FlickAndSettle(icon)) },
            { IconAnimationType.JellyBounce, icon => StartCoroutine(Animate_JellyBounce(icon)) },
            { IconAnimationType.Slingshot, icon => StartCoroutine(Animate_SlingShot(icon)) },
            { IconAnimationType.Unfold, icon => StartCoroutine(Animate_Unfold(icon)) },
            // Physical
            { IconAnimationType.SqueezeAndPop, icon => StartCoroutine(Animate_SqueezeAndPop(icon)) },
            { IconAnimationType.Flip3D, icon => StartCoroutine(Animate_Flip3D(icon)) },
            { IconAnimationType.PendulumSwing, icon => StartCoroutine(Animate_PendulumSwing(icon)) },
            { IconAnimationType.RippleOut, icon => StartCoroutine(Animate_RippleOut(icon)) },
            { IconAnimationType.PaperToss, icon => StartCoroutine(Animate_PaperToss(icon)) },
            { IconAnimationType.NewtonsCradle, icon => StartCoroutine(Animate_NewtonsCradle(icon)) },
            { IconAnimationType.LiquidFill, icon => StartCoroutine(Animate_LiquidFill(icon)) },
            { IconAnimationType.Shutter, icon => StartCoroutine(Animate_Shutter(icon)) },
            { IconAnimationType.OrigamiFold, icon => StartCoroutine(Animate_OrigamiFold(icon)) },
            { IconAnimationType.Magnetize, icon => StartCoroutine(Animate_Magnetize(icon)) },
            // Organic
            { IconAnimationType.Wiggle, icon => StartCoroutine(Animate_Wiggle(icon)) },
            { IconAnimationType.PeekABoo, icon => StartCoroutine(Animate_PeekABoo(icon)) },
            { IconAnimationType.Nod, icon => StartCoroutine(Animate_Nod(icon)) },
            { IconAnimationType.Blink, icon => StartCoroutine(Animate_Blink(icon)) },
            { IconAnimationType.TailWag, icon => StartCoroutine(Animate_TailWag(icon)) },
            { IconAnimationType.Gasp, icon => StartCoroutine(Animate_Gasp(icon)) },
            { IconAnimationType.Crawl, icon => StartCoroutine(Animate_Crawl(icon)) },
            { IconAnimationType.Float, icon => StartCoroutine(Animate_Float(icon)) },
            { IconAnimationType.Scurry, icon => StartCoroutine(Animate_Scurry(icon)) },
            { IconAnimationType.Exhale, icon => StartCoroutine(Animate_Exhale(icon)) },
            // Celebratory
            { IconAnimationType.Fireworks, icon => StartCoroutine(Animate_Fireworks(icon)) },
            { IconAnimationType.Spotlight, icon => StartCoroutine(Animate_Spotlight(icon)) },
            { IconAnimationType.ConfettiPop, icon => StartCoroutine(Animate_ConfettiPop(icon)) },
            { IconAnimationType.Crown, icon => StartCoroutine(Animate_Crown(icon)) },
            { IconAnimationType.Shockwave, icon => StartCoroutine(Animate_Shockwave(icon)) },
            { IconAnimationType.Stomp, icon => StartCoroutine(Animate_Stomp(icon)) },
            { IconAnimationType.VictoryRise, icon => StartCoroutine(Animate_VictoryRise(icon)) },
            { IconAnimationType.Applause, icon => StartCoroutine(Animate_Applause(icon)) },
            { IconAnimationType.PowerUp, icon => StartCoroutine(Animate_PowerUp(icon)) },
            { IconAnimationType.Fanfare, icon => StartCoroutine(Animate_Fanfare(icon)) },
            // Digital
            { IconAnimationType.ScanlineReveal, icon => StartCoroutine(Animate_ScanlineReveal(icon)) },
            { IconAnimationType.Glitch, icon => StartCoroutine(Animate_Glitch(icon)) },
            { IconAnimationType.ChargingPulse, icon => StartCoroutine(Animate_ChargingPulse(icon)) },
            { IconAnimationType.Pixelate, icon => StartCoroutine(Animate_Pixelate(icon)) },
            { IconAnimationType.CodeMatrix, icon => StartCoroutine(Animate_CodeMatrix(icon)) },
            { IconAnimationType.Hologram, icon => StartCoroutine(Animate_Hologram(icon)) },
            { IconAnimationType.FocusLock, icon => StartCoroutine(Animate_FocusLock(icon)) },
            { IconAnimationType.DataStream, icon => StartCoroutine(Animate_DataStream(icon)) },
            { IconAnimationType.Reboot, icon => StartCoroutine(Animate_Reboot(icon)) },
            { IconAnimationType.Voxelize, icon => StartCoroutine(Animate_Voxelize(icon)) },
            // Additional
            { IconAnimationType.RubberBand, icon => StartCoroutine(Animate_RubberBand(icon)) },
            { IconAnimationType.Swing, icon => StartCoroutine(Animate_Swing(icon)) },
            { IconAnimationType.Wobble, icon => StartCoroutine(Animate_Wobble(icon)) },
            { IconAnimationType.Flash, icon => StartCoroutine(Animate_Flash(icon)) },
            { IconAnimationType.Pulse, icon => StartCoroutine(Animate_Pulse(icon)) },
            { IconAnimationType.Bounce, icon => StartCoroutine(Animate_Bounce(icon)) },
            { IconAnimationType.Jello, icon => StartCoroutine(Animate_Jello(icon)) },
            { IconAnimationType.LightSpeedIn, icon => StartCoroutine(Animate_LightSpeedIn(icon)) },
            { IconAnimationType.RollIn, icon => StartCoroutine(Animate_RollIn(icon)) },
            { IconAnimationType.RotateIn, icon => StartCoroutine(Animate_RotateIn(icon)) },
            { IconAnimationType.SlideInLeft, icon => StartCoroutine(Animate_SlideInLeft(icon)) },
            { IconAnimationType.SlideInRight, icon => StartCoroutine(Animate_SlideInRight(icon)) },
            { IconAnimationType.SlideInUp, icon => StartCoroutine(Animate_SlideInUp(icon)) },
            { IconAnimationType.SlideInDown, icon => StartCoroutine(Animate_SlideInDown(icon)) },
            { IconAnimationType.ZoomIn, icon => StartCoroutine(Animate_ZoomIn(icon)) },
            { IconAnimationType.KineticChain, icon => StartCoroutine(Animate_KineticChain(icon)) },
            { IconAnimationType.FlywheelSpin, icon => StartCoroutine(Animate_FlywheelSpin(icon)) },
        };
        animationKeys = animationLibrary.Keys.ToList();
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        if (currentIndex == -1 && navButtons.Count > 0) SelectButton(navButtons.Count - 1, false);
        root.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    private void SelectButton(int index, bool animated = true)
    {
        if (index < 0 || index >= navButtons.Count || index == currentIndex) return;
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            if (currentIndex != -1) ResetIconStyle(navIcons[currentIndex]);
        }

        for (int i = 0; i < navIcons.Count; i++) navIcons[i].EnableInClassList(SELECTED_ICON_CLASS, i == index);
        Button selectedButton = navButtons[index];
        float targetX = selectedButton.layout.xMin + (selectedButton.layout.width / 2f) -
                        (selectionIndicator.layout.width / 2f);
        var duration = animated ? 0.4f : 0f;
        selectionIndicator.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue>
            { new TimeValue(duration, TimeUnit.Second) });
        selectionIndicator.style.left = targetX;
        if (animated) PlayNextAnimationInCycle(navIcons[index]);
        currentIndex = index;
    }

    private void PlayNextAnimationInCycle(VisualElement icon)
    {
        if (animationKeys.Count == 0) return;
        IconAnimationType nextAnimationType = animationKeys[_currentAnimationIndex];
        Debug.Log($"Playing Animation ({_currentAnimationIndex + 1}/{animationKeys.Count}): {nextAnimationType}");
        currentAnimation = animationLibrary[nextAnimationType](icon);
        _currentAnimationIndex = (_currentAnimationIndex + 1) % animationKeys.Count;
    }

    private void ResetIconStyle(VisualElement icon)
    {
        icon.style.scale = new StyleScale(new Scale(Vector3.one));
        icon.style.rotate = new StyleRotate(new Rotate(new Angle(0)));
        icon.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(0)));
        icon.style.opacity = StyleKeyword.Null;
        icon.style.transformOrigin = new StyleTransformOrigin(new TransformOrigin(new Length(50, LengthUnit.Percent),
            new Length(50, LengthUnit.Percent)));

        var container = icon.parent;
        if (container == null) return;

        var scanline = container.Q<VisualElement>(className: "scanline-effect");
        if (scanline != null)
        {
            scanline.style.opacity = 0;
            scanline.style.translate =
                new StyleTranslate(new Translate(new Length(0), new Length(-100, LengthUnit.Percent)));
        }

        var spotlight = container.Q<VisualElement>(className: "spotlight-effect");
        if (spotlight != null)
        {
            spotlight.style.opacity = 0;
        }

        var ripple = container.Q<VisualElement>(className: "ripple-effect");
        if (ripple != null)
        {
            ripple.style.opacity = 0;
            ripple.style.scale = new StyleScale(new Scale(Vector2.zero));
        }
    }

    #endregion

    #region Animation Coroutines (The Master Library)

    // --- Category: Foundational ---
    private IEnumerator Animate_ElasticPop(VisualElement icon)
    {
        float duration = 0.6f, time = 0;
        while (time < duration)
        {
            float progress = Easing.OutElastic(time / duration);
            icon.style.scale = new StyleScale(new Scale(new Vector2(progress, progress)));
            time += Time.deltaTime;
            yield return null;
        }

        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Shake(VisualElement icon)
    {
        float duration = 0.3f, time = 0, shakeAmount = 8f;
        while (time < duration)
        {
            float progress = time / duration;
            float currentX = Mathf.Sin(progress * Mathf.PI * 8) * (shakeAmount * (1 - progress));
            icon.style.translate =
                new StyleTranslate(new Translate(new Length(currentX, LengthUnit.Pixel), new Length(0)));
            time += Time.deltaTime;
            yield return null;
        }

        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Heartbeat(VisualElement icon)
    {
        float beat1Duration = 0.15f, beat2Duration = 0.15f, delay = 0.05f, beatScale = 1.25f;
        float time = 0;
        while (time < beat1Duration)
        {
            float progress = time / beat1Duration;
            float scaleValue = 1 + Mathf.Sin(progress * Mathf.PI) * (beatScale - 1);
            icon.style.scale = new StyleScale(new Scale(new Vector2(scaleValue, scaleValue)));
            time += Time.deltaTime;
            yield return null;
        }

        icon.style.scale = new StyleScale(new Scale(Vector3.one));
        yield return new WaitForSeconds(delay);
        time = 0;
        while (time < beat2Duration)
        {
            float progress = time / beat2Duration;
            float scaleValue = 1 + Mathf.Sin(progress * Mathf.PI) * (beatScale - 1);
            icon.style.scale = new StyleScale(new Scale(new Vector2(scaleValue, scaleValue)));
            time += Time.deltaTime;
            yield return null;
        }

        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Tada(VisualElement icon)
    {
        float duration = 0.5f, time = 0, maxScale = 1.2f, maxAngle = 8f;
        while (time < duration)
        {
            float progress = time / duration;
            float scaleValue = 1 + Mathf.Sin(progress * Mathf.PI) * (maxScale - 1);
            float angle = Mathf.Sin(progress * Mathf.PI * 3) * maxAngle * (1 - progress);
            icon.style.scale = new StyleScale(new Scale(new Vector2(scaleValue, scaleValue)));
            icon.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
            time += Time.deltaTime;
            yield return null;
        }

        ResetIconStyle(icon);
    }

    private IEnumerator Animate_DropInAndBounce(VisualElement icon)
    {
        float duration = 0.7f, time = 0, startY = -80f;
        while (time < duration)
        {
            float progress = Easing.OutBounce(time / duration);
            float currentY = Mathf.Lerp(startY, 0, progress);
            icon.style.translate =
                new StyleTranslate(new Translate(new Length(0), new Length(currentY, LengthUnit.Pixel)));
            time += Time.deltaTime;
            yield return null;
        }

        ResetIconStyle(icon);
    }

    private IEnumerator Animate_WindUpAndSpin(VisualElement icon)
    {
        float duration = 0.5f, time = 0, windUpAngle = -30f, spinAngle = 360f + 45f;
        float windUpDuration = duration * 0.3f;
        while (time < windUpDuration)
        {
            float progress = Easing.OutCubic(time / windUpDuration);
            icon.style.rotate =
                new StyleRotate(new Rotate(new Angle(Mathf.Lerp(0, windUpAngle, progress), AngleUnit.Degree)));
            time += Time.deltaTime;
            yield return null;
        }

        time = 0;
        float spinDuration = duration * 0.7f;
        while (time < spinDuration)
        {
            float progress = Easing.OutCubic(time / spinDuration);
            icon.style.rotate =
                new StyleRotate(new Rotate(new Angle(Mathf.Lerp(windUpAngle, spinAngle, progress), AngleUnit.Degree)));
            time += Time.deltaTime;
            yield return null;
        }

        ResetIconStyle(icon);
    }

    private IEnumerator Animate_FlickAndSettle(VisualElement icon)
    {
        float duration = 0.8f, time = 0, startVelocity = 720f;
        while (time < duration)
        {
            float progress = time / duration;
            float angle = startVelocity * (1 - progress) * Mathf.Sin(progress * Mathf.PI * 2.5f) * 0.1f;
            icon.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
            time += Time.deltaTime;
            yield return null;
        }

        ResetIconStyle(icon);
    }

    private IEnumerator Animate_JellyBounce(VisualElement icon)
    {
        float duration = 0.6f, time = 0, squashFactor = 0.7f, stretchFactor = 1.3f;
        while (time < duration)
        {
            float progress = time / duration;
            float scaleX, scaleY;
            if (progress < 0.5f)
            {
                scaleX = Mathf.Lerp(1, stretchFactor, Easing.OutQuad(progress * 2));
                scaleY = Mathf.Lerp(1, squashFactor, Easing.OutQuad(progress * 2));
            }
            else
            {
                scaleX = Mathf.Lerp(stretchFactor, 1, Easing.InQuad((progress - 0.5f) * 2));
                scaleY = Mathf.Lerp(squashFactor, 1, Easing.InQuad((progress - 0.5f) * 2));
            }

            icon.style.scale = new StyleScale(new Scale(new Vector2(scaleX, scaleY)));
            time += Time.deltaTime;
            yield return null;
        }

        ResetIconStyle(icon);
    }

    private IEnumerator Animate_SlingShot(VisualElement icon)
    {
        float duration = 0.5f, time = 0, pullBackDist = -20f;
        float pullDuration = duration * 0.4f;
        while (time < pullDuration)
        {
            float progress = Easing.OutCubic(time / pullDuration);
            icon.style.translate = new StyleTranslate(new Translate(new Length(0),
                new Length(Mathf.Lerp(0, pullBackDist, progress), LengthUnit.Pixel)));
            time += Time.deltaTime;
            yield return null;
        }

        time = 0;
        float shootDuration = duration * 0.6f;
        while (time < shootDuration)
        {
            float progress = Easing.OutBounce(time / shootDuration);
            icon.style.translate = new StyleTranslate(new Translate(new Length(0),
                new Length(Mathf.Lerp(pullBackDist, 0, progress), LengthUnit.Pixel)));
            time += Time.deltaTime;
            yield return null;
        }

        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Unfold(VisualElement icon)
    {
        float duration = 0.4f, time = 0;
        icon.style.scale = new StyleScale(new Scale(new Vector2(0, 1)));
        float halfDuration = duration / 2;
        while (time < halfDuration)
        {
            float progress = Easing.OutCubic(time / halfDuration);
            icon.style.scale = new StyleScale(new Scale(new Vector2(progress, 1)));
            time += Time.deltaTime;
            yield return null;
        }

        icon.style.scale = new StyleScale(new Scale(new Vector2(1, 0)));
        time = 0;
        while (time < halfDuration)
        {
            float progress = Easing.OutCubic(time / halfDuration);
            icon.style.scale = new StyleScale(new Scale(new Vector2(1, progress)));
            time += Time.deltaTime;
            yield return null;
        }

        ResetIconStyle(icon);
    }

    // --- Category: Physical & Material ---
    private IEnumerator Animate_SqueezeAndPop(VisualElement icon)
    {
        float duration = 0.4f, time = 0, squeezeScaleX = 1.2f, squeezeScaleY = 0.8f;
        float squeezeDuration = duration * 0.4f;
        while (time < squeezeDuration)
        {
            float progress = Easing.OutCubic(time / squeezeDuration);
            float currentScaleX = Mathf.Lerp(1f, squeezeScaleX, progress);
            float currentScaleY = Mathf.Lerp(1f, squeezeScaleY, progress);
            icon.style.scale = new StyleScale(new Scale(new Vector2(currentScaleX, currentScaleY)));
            time += Time.deltaTime;
            yield return null;
        }

        time = 0;
        float popDuration = duration * 0.6f;
        while (time < popDuration)
        {
            float progress = Easing.OutElastic(time / popDuration);
            float currentScale = Mathf.Lerp(squeezeScaleX, 1f, progress);
            icon.style.scale = new StyleScale(new Scale(new Vector2(currentScale, currentScale)));
            time += Time.deltaTime;
            yield return null;
        }

        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Flip3D(VisualElement icon)
    {
        float duration = 0.5f, time = 0;
        while (time < duration)
        {
            float progress = Easing.InOutCubic(time / duration);
            float scaleX = Mathf.Cos(progress * Mathf.PI);
            icon.style.scale = new StyleScale(new Scale(new Vector2(scaleX, 1)));
            time += Time.deltaTime;
            yield return null;
        }

        ResetIconStyle(icon);
    }

    private IEnumerator Animate_PendulumSwing(VisualElement icon)
    {
        float duration = 1.2f, time = 0, maxAngle = 30f;
        icon.style.transformOrigin =
            new StyleTransformOrigin(new TransformOrigin(new Length(50, LengthUnit.Percent), new Length(0)));
        while (time < duration)
        {
            float progress = time / duration;
            float angle = maxAngle * Mathf.Sin(progress * Mathf.PI * 3.5f) / (1 + progress * 5);
            icon.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
            time += Time.deltaTime;
            yield return null;
        }

        ResetIconStyle(icon);
    }

    private IEnumerator Animate_RippleOut(VisualElement icon)
    {
        var ripple = icon.parent?.Q<VisualElement>(className: "ripple-effect");
        if (ripple == null)
        {
            yield break;
        }

        float duration = 0.5f, time = 0;
        while (time < duration)
        {
            float progress = Easing.OutCubic(time / duration);
            ripple.style.scale = new StyleScale(new Scale(new Vector2(progress * 1.5f, progress * 1.5f)));
            ripple.style.opacity = 1 - progress;
            time += Time.deltaTime;
            yield return null;
        }

        ResetIconStyle(icon);
    }

    private IEnumerator Animate_PaperToss(VisualElement icon)
    {
        float duration = 0.6f, time = 0;
        while (time < duration)
        {
            float progress = Easing.InCubic(time / duration);
            icon.style.scale = new StyleScale(new Scale(Vector2.one * (1 - progress)));
            icon.style.rotate = new StyleRotate(new Rotate(new Angle(progress * 720f, AngleUnit.Degree)));
            icon.style.translate = new StyleTranslate(new Translate(new Length(progress * 100f, LengthUnit.Pixel),
                new Length(progress * -50f, LengthUnit.Pixel)));
            icon.style.opacity = 1 - progress;
            time += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_NewtonsCradle(VisualElement icon)
    {
        float duration = 1.0f, time = 0, amplitude = 10f;
        while (time < duration)
        {
            float progress = time / duration;
            float x = amplitude * Mathf.Sin(time * 20) * Mathf.Exp(-progress * 3);
            icon.style.translate = new StyleTranslate(new Translate(new Length(x, LengthUnit.Pixel), new Length(0)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_LiquidFill(VisualElement icon)
    {
        float duration = 0.6f, time = 0;
        icon.style.transformOrigin = new StyleTransformOrigin(new TransformOrigin(new Length(50, LengthUnit.Percent), new Length(100, LengthUnit.Percent)));
        icon.style.scale = new StyleScale(new Scale(new Vector2(1,0)));
        while (time < duration)
        {
            float progress = Easing.OutBounce(time / duration);
            icon.style.scale = new StyleScale(new Scale(new Vector2(1, progress)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Shutter(VisualElement icon)
    {
        float duration = 0.4f, time = 0;
        float half = duration / 2;
        while (time < half)
        {
            float progress = time / half;
            icon.style.scale = new StyleScale(new Scale(new Vector2(1 - progress, 1)));
            time += Time.deltaTime;
            yield return null;
        }
        icon.style.scale = new StyleScale(new Scale(new Vector2(0,1)));
        time = 0;
        while (time < half)
        {
            float progress = time / half;
            icon.style.scale = new StyleScale(new Scale(new Vector2(progress, 1)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_OrigamiFold(VisualElement icon)
    {
        float duration = 0.6f, time = 0;
        icon.style.transformOrigin = new StyleTransformOrigin(new TransformOrigin(new Length(0, LengthUnit.Percent), new Length(0, LengthUnit.Percent)));
        while (time < duration)
        {
            float progress = time / duration;
            float angle = 180 * Mathf.Sin(progress * Mathf.PI);
            icon.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Magnetize(VisualElement icon)
    {
        float duration = 0.5f, time = 0, attractDist = 20f;
        while (time < duration * 0.6f)
        {
            float progress = Easing.InCubic(time / (duration * 0.6f));
            icon.style.translate = new StyleTranslate(new Translate(new Length(Mathf.Lerp(0, attractDist, progress), LengthUnit.Pixel), new Length(0)));
            time += Time.deltaTime;
            yield return null;
        }
        time = 0;
        while (time < duration * 0.4f)
        {
            float progress = Easing.OutBounce(time / (duration * 0.4f));
            icon.style.translate = new StyleTranslate(new Translate(new Length(Mathf.Lerp(attractDist, 0, progress), LengthUnit.Pixel), new Length(0)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    // --- Category: Organic & Playful ---
    private IEnumerator Animate_Wiggle(VisualElement icon)
    {
        float duration = 0.5f, time = 0, maxAngle = 15f;
        while (time < duration)
        {
            float progress = time / duration;
            float angle = Mathf.Sin(progress * Mathf.PI * 6) * (maxAngle * (1 - progress));
            icon.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
            time += Time.deltaTime;
            yield return null;
        }

        ResetIconStyle(icon);
    }

    private IEnumerator Animate_PeekABoo(VisualElement icon)
    {
        float duration = 0.5f, time = 0, hideY = 40f;
        float hideDuration = duration * 0.4f;
        while (time < hideDuration)
        {
            float progress = Easing.InBack(time / hideDuration, 1.2f);
            icon.style.translate = new StyleTranslate(new Translate(new Length(0),
                new Length(Mathf.Lerp(0, hideY, progress), LengthUnit.Pixel)));
            time += Time.deltaTime;
            yield return null;
        }

        time = 0;
        float showDuration = duration * 0.6f;
        while (time < showDuration)
        {
            float progress = Easing.OutElastic(time / showDuration);
            icon.style.translate = new StyleTranslate(new Translate(new Length(0),
                new Length(Mathf.Lerp(hideY, 0, progress), LengthUnit.Pixel)));
            time += Time.deltaTime;
            yield return null;
        }

        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Nod(VisualElement icon)
    {
        float duration = 0.5f, time = 0, maxAngle = 20f;
        icon.style.transformOrigin = new StyleTransformOrigin(new TransformOrigin(new Length(50, LengthUnit.Percent),
            new Length(100, LengthUnit.Percent)));
        while (time < duration)
        {
            float progress = time / duration;
            float angle = Mathf.Abs(Mathf.Sin(progress * Mathf.PI * 2)) * -maxAngle;
            icon.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
            time += Time.deltaTime;
            yield return null;
        }

        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Blink(VisualElement icon)
    {
        float duration = 0.2f, time = 0;
        while (time < duration)
        {
            float progress = time / duration;
            float scaleY = 1 - Mathf.Sin(progress * Mathf.PI);
            icon.style.scale = new StyleScale(new Scale(new Vector2(1, scaleY)));
            time += Time.deltaTime;
            yield return null;
        }

        ResetIconStyle(icon);
    }

    private IEnumerator Animate_TailWag(VisualElement icon)
    {
        float duration = 0.6f, time = 0, maxAngle = 20f;
        icon.style.transformOrigin = new StyleTransformOrigin(new TransformOrigin(new Length(50, LengthUnit.Percent), new Length(0, LengthUnit.Percent)));
        while (time < duration)
        {
            float progress = time / duration;
            float angle = maxAngle * Mathf.Sin(progress * Mathf.PI * 5) * (1 - progress);
            icon.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Gasp(VisualElement icon)
    {
        float duration = 0.3f, time = 0, gaspScale = 1.3f;
        while (time < duration / 2)
        {
            float progress = (time / (duration / 2));
            float scale = Mathf.Lerp(1, gaspScale, Easing.OutQuad(progress));
            icon.style.scale = new StyleScale(new Scale(new Vector2(scale, scale)));
            time += Time.deltaTime;
            yield return null;
        }
        time = 0;
        while (time < duration / 2)
        {
            float progress = (time / (duration / 2));
            float scale = Mathf.Lerp(gaspScale, 1, Easing.InQuad(progress));
            icon.style.scale = new StyleScale(new Scale(new Vector2(scale, scale)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Crawl(VisualElement icon)
    {
        float duration = 0.8f, time = 0, startX = -50f;
        while (time < duration)
        {
            float progress = Easing.OutSine(time / duration);
            float x = Mathf.Lerp(startX, 0, progress);
            float scaleY = 0.8f + 0.2f * Mathf.Sin(progress * Mathf.PI * 4);
            icon.style.translate = new StyleTranslate(new Translate(new Length(x, LengthUnit.Pixel), new Length(0)));
            icon.style.scale = new StyleScale(new Scale(new Vector2(1, scaleY)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Float(VisualElement icon)
    {
        float duration = 1.0f, time = 0, floatAmp = 10f;
        while (time < duration)
        {
            float progress = time / duration;
            float y = floatAmp * Mathf.Sin(progress * Mathf.PI * 2);
            icon.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(y, LengthUnit.Pixel)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Scurry(VisualElement icon)
    {
        float duration = 0.4f, time = 0, scurryDist = 30f;
        while (time < duration)
        {
            float progress = Easing.InOutSine(time / duration);
            float x = scurryDist * Mathf.Sin(progress * Mathf.PI * 3);
            icon.style.translate = new StyleTranslate(new Translate(new Length(x, LengthUnit.Pixel), new Length(0)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Exhale(VisualElement icon)
    {
        float duration = 0.8f, time = 0, maxScale = 1.1f;
        while (time < duration)
        {
            float progress = time / duration;
            float scale = maxScale - (maxScale - 1) * progress;
            icon.style.scale = new StyleScale(new Scale(new Vector2(scale, scale)));
            icon.style.opacity = 1 - progress * 0.5f;
            time += Time.deltaTime;
            yield return null;
        }
        icon.style.opacity = 1;
        ResetIconStyle(icon);
    }

    // --- Category: Celebratory & Emphatic ---
    private IEnumerator Animate_Fireworks(VisualElement icon)
    {
        var container = icon.parent;
        if (container == null) yield break;
        int particleCount = 10;
        for (int i = 0; i < particleCount; i++)
        {
            var particle = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    width = 5,
                    height = 5,
                    backgroundColor = Color.HSVToRGB(Random.value, 0.8f, 1f)
                }
            };
            var radius = new StyleLength(new Length(50, LengthUnit.Percent));
            particle.style.borderTopLeftRadius = particle.style.borderTopRightRadius =
                particle.style.borderBottomLeftRadius = particle.style.borderBottomRightRadius = radius;
            container.Add(particle);
            StartCoroutine(AnimateParticle(particle, 0.6f));
        }

        yield return Animate_ElasticPop(icon);
    }

    private IEnumerator AnimateParticle(VisualElement particle, float duration)
    {
        float time = 0;
        Vector2 direction = Random.insideUnitCircle.normalized;
        float distance = Random.Range(30f, 60f);
        while (time < duration)
        {
            float progress = Easing.OutCubic(time / duration);
            particle.style.translate = new StyleTranslate(new Translate(
                new Length(direction.x * progress * distance, LengthUnit.Pixel),
                new Length(direction.y * progress * distance, LengthUnit.Pixel)));
            particle.style.opacity = 1 - progress;
            time += Time.deltaTime;
            yield return null;
        }

        particle.RemoveFromHierarchy();
    }

    private IEnumerator Animate_Spotlight(VisualElement icon)
    {
        var spotlight = icon.parent?.Q<VisualElement>(className: "spotlight-effect");
        if (spotlight == null)
        {
            yield break;
        }

        float duration = 0.7f, time = 0;
        spotlight.style.opacity = 1;
        while (time < duration)
        {
            float progress = Easing.InOutCubic(time / duration);
            spotlight.style.rotate =
                new StyleRotate(new Rotate(new Angle(Mathf.Lerp(-45, 45, progress), AngleUnit.Degree)));
            time += Time.deltaTime;
            yield return null;
        }

        ResetIconStyle(icon);
    }
    
    private IEnumerator Animate_KineticChain(VisualElement icon)
    {
        float duration = 0.6f, time = 0;
        float segmentCount = 5f;

        while (time < duration)
        {
            float progress = Easing.OutElastic(time / duration);
            float totalAngle = 45f * progress;

            for (int i = 0; i < (int)segmentCount; i++)
            {
                float segmentProgress = (float)i / segmentCount;
                float angle = totalAngle * (1 - segmentProgress);

                if (i == 0)
                {
                    icon.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
                }
                else
                {
                    var segment = icon.parent?.Q<VisualElement>($"segment-{i}");
                    if (segment != null)
                    {
                        segment.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
                    }
                }
            }

            time += Time.deltaTime;
            yield return null;
        }

        ResetIconStyle(icon);
    }
    
    private IEnumerator Animate_FlywheelSpin(VisualElement icon)
    {
        float duration = 0.6f, time = 0;
        float spinSpeed = 1080f;

        while (time < duration)
        {
            float progress = Easing.OutCubic(time / duration);
            float rotation = spinSpeed * time;

            icon.style.rotate = new StyleRotate(new Rotate(new Angle(rotation, AngleUnit.Degree)));
            icon.style.scale = new StyleScale(new Scale(new Vector2(
                1f + progress * 0.1f, 1f + progress * 0.1f
            )));

            time += Time.deltaTime;
            yield return null;
        }

        ResetIconStyle(icon);
    }

    private IEnumerator Animate_ConfettiPop(VisualElement icon)
    {
        var container = icon.parent;
        if (container == null) yield break;
        for (int i = 0; i < 15; i++)
        {
            var particle = new VisualElement
            {
                style =
                {
                    position = Position.Absolute, width = 6, height = 6,
                    backgroundColor = Color.HSVToRGB(Random.value, 0.8f, 1f)
                }
            };
            container.Add(particle);
            StartCoroutine(Animate_ConfettiParticle(particle, 1.0f));
        }

        yield return Animate_SqueezeAndPop(icon);
    }

    private IEnumerator Animate_ConfettiParticle(VisualElement particle, float duration)
    {
        float time = 0;
        Vector2 velocity = Random.insideUnitCircle * 150f;
        velocity.y = Mathf.Abs(velocity.y) * -1;
        float gravity = 400f;
        Vector2 position = Vector2.zero;
        while (time < duration)
        {
            velocity.y += gravity * Time.deltaTime;
            position += velocity * Time.deltaTime;
            particle.style.translate = new StyleTranslate(new Translate(new Length(position.x, LengthUnit.Pixel),
                new Length(position.y, LengthUnit.Pixel)));
            particle.style.opacity = 1 - (time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        particle.RemoveFromHierarchy();
    }

    private IEnumerator Animate_Crown(VisualElement icon)
    {
        float duration = 0.5f, time = 0, scale = 1.2f;
        while (time < duration)
        {
            float progress = Easing.OutElastic(time / duration);
            icon.style.scale = new StyleScale(new Scale(new Vector2(progress * scale, progress * scale)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Shockwave(VisualElement icon)
    {
        var ripple = icon.parent?.Q<VisualElement>(className: "ripple-effect");
        if (ripple == null) yield break;
        float duration = 0.6f, time = 0;
        while (time < duration)
        {
            float progress = Easing.OutQuad(time / duration);
            ripple.style.scale = new StyleScale(new Scale(new Vector2(progress * 2f, progress * 2f)));
            ripple.style.opacity = 1 - progress;
            icon.style.scale = new StyleScale(new Scale(new Vector2(1 + 0.2f * (1 - progress), 1 + 0.2f * (1 - progress))));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Stomp(VisualElement icon)
    {
        float duration = 0.5f, time = 0, stompDist = 20f;
        float downDuration = duration * 0.3f;
        while (time < downDuration)
        {
            float progress = Easing.InCubic(time / downDuration);
            icon.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(stompDist * progress, LengthUnit.Pixel)));
            icon.style.scale = new StyleScale(new Scale(new Vector2(1 + 0.2f * progress, 1 - 0.2f * progress)));
            time += Time.deltaTime;
            yield return null;
        }
        time = 0;
        float upDuration = duration * 0.7f;
        while (time < upDuration)
        {
            float progress = Easing.OutBounce(time / upDuration);
            icon.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(stompDist * (1 - progress), LengthUnit.Pixel)));
            icon.style.scale = new StyleScale(new Scale(new Vector2(1 + 0.2f * (1 - progress), 1 - 0.2f * (1 - progress))));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_VictoryRise(VisualElement icon)
    {
        float duration = 0.8f, time = 0, riseDist = -50f;
        while (time < duration)
        {
            float progress = Easing.OutCubic(time / duration);
            float y = riseDist * progress;
            float scale = 1 + 0.5f * progress;
            icon.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(y, LengthUnit.Pixel)));
            icon.style.scale = new StyleScale(new Scale(new Vector2(scale, scale)));
            icon.style.opacity = 1 - progress;
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Applause(VisualElement icon)
    {
        int claps = 8;
        for (int i = 0; i < claps; i++)
        {
            float x = 5f * Random.Range(-1f, 1f);
            float y = 5f * Random.Range(-1f, 1f);
            icon.style.translate = new StyleTranslate(new Translate(new Length(x, LengthUnit.Pixel), new Length(y, LengthUnit.Pixel)));
            yield return new WaitForSeconds(0.1f);
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_PowerUp(VisualElement icon)
    {
        float duration = 0.6f, time = 0;
        while (time < duration)
        {
            float progress = time / duration;
            float scale = 1 + progress * 0.5f;
            icon.style.scale = new StyleScale(new Scale(new Vector2(scale, scale)));
            icon.style.opacity = 1 - progress;
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Fanfare(VisualElement icon)
    {
        float duration = 0.5f, time = 0;
        while (time < duration)
        {
            float progress = Easing.OutBack(time / duration);
            float angle = 360 * progress;
            icon.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    // --- Category: Digital & Futuristic ---
    private IEnumerator Animate_ScanlineReveal(VisualElement icon)
    {
        var scanline = icon.parent?.Q<VisualElement>(className: "scanline-effect");
        if (scanline == null)
        {
            yield break;
        }

        float duration = 0.4f, time = 0;
        scanline.style.opacity = 1;
        while (time < duration)
        {
            float progress = Easing.OutCubic(time / duration);
            float yPos = Mathf.Lerp(-50, 50, progress);
            scanline.style.translate =
                new StyleTranslate(new Translate(new Length(0), new Length(yPos, LengthUnit.Percent)));
            time += Time.deltaTime;
            yield return null;
        }

        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Glitch(VisualElement icon)
    {
        float duration = 0.3f, time = 0;
        while (time < duration)
        {
            float xGlitch = Random.Range(-5f, 5f);
            float yGlitch = Random.Range(-5f, 5f);
            icon.style.translate = new StyleTranslate(new Translate(new Length(xGlitch, LengthUnit.Pixel),
                new Length(yGlitch, LengthUnit.Pixel)));
            yield return new WaitForSeconds(0.03f);
            time += 0.03f;
        }

        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Pixelate(VisualElement icon)
    {
        yield return Animate_ElasticPop(icon);
    } // Placeholder as it requires asset swapping

    private IEnumerator Animate_ChargingPulse(VisualElement icon)
    {
        float duration = 1.0f;
        float pulseCount = 5;
        for (int i = 0; i < pulseCount; i++)
        {
            float pulseDuration = duration / pulseCount * (1 - (float)i / pulseCount);
            float time = 0;
            while (time < pulseDuration / 2)
            {
                float progress = time / (pulseDuration / 2);
                float scale = 1 + 0.2f * progress;
                icon.style.scale = new StyleScale(new Scale(new Vector2(scale, scale)));
                time += Time.deltaTime;
                yield return null;
            }
            time = 0;
            while (time < pulseDuration / 2)
            {
                float progress = time / (pulseDuration / 2);
                float scale = 1 + 0.2f * (1 - progress);
                icon.style.scale = new StyleScale(new Scale(new Vector2(scale, scale)));
                time += Time.deltaTime;
                yield return null;
            }
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_CodeMatrix(VisualElement icon)
    {
        float duration = 0.5f, time = 0;
        icon.style.opacity = 0;
        while (time < duration)
        {
            float progress = time / duration;
            float y = -100 * (1 - progress);
            icon.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(y, LengthUnit.Percent)));
            icon.style.opacity = progress;
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Hologram(VisualElement icon)
    {
        float duration = 0.5f, time = 0;
        icon.style.opacity = 0;
        while (time < duration)
        {
            float progress = Easing.OutSine(time / duration);
            icon.style.opacity = progress;
            float scale = 1 + 0.1f * Mathf.Sin(progress * Mathf.PI * 5);
            icon.style.scale = new StyleScale(new Scale(new Vector2(scale, scale)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_FocusLock(VisualElement icon)
    {
        float duration = 0.5f, time = 0, startScale = 2f;
        icon.style.scale = new StyleScale(new Scale(new Vector2(startScale, startScale)));
        icon.style.opacity = 0.5f;
        while (time < duration)
        {
            float progress = Easing.OutQuad(time / duration);
            float scale = Mathf.Lerp(startScale, 1, progress);
            icon.style.scale = new StyleScale(new Scale(new Vector2(scale, scale)));
            icon.style.opacity = 0.5f + 0.5f * progress;
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_DataStream(VisualElement icon)
    {
        yield return Animate_ScanlineReveal(icon);
    }

    private IEnumerator Animate_Reboot(VisualElement icon)
    {
        float duration = 0.5f, time = 0;
        float half = duration / 2;
        while (time < half)
        {
            float progress = time / half;
            icon.style.opacity = 1 - progress;
            time += Time.deltaTime;
            yield return null;
        }
        yield return Animate_Glitch(icon);
        time = 0;
        while (time < half)
        {
            float progress = time / half;
            icon.style.opacity = progress;
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Voxelize(VisualElement icon)
    {
        float duration = 0.5f, time = 0, steps = 5;
        for (int i = 0; i < steps; i++)
        {
            float scale = 1 - (float)i / steps;
            icon.style.scale = new StyleScale(new Scale(new Vector2(scale, scale)));
            yield return new WaitForSeconds(duration / steps);
        }
        icon.style.scale = new StyleScale(new Scale(new Vector2(0,0)));
        yield return new WaitForSeconds(0.2f);
        ResetIconStyle(icon);
    }

    // --- Additional Animations ---
    private IEnumerator Animate_RubberBand(VisualElement icon)
    {
        float[] times = {0, 0.3f, 0.4f, 0.5f, 0.65f, 0.75f, 0.8f, 0.9f, 1.0f};
        Vector2[] scales = {new Vector2(1,1), new Vector2(1.25f,0.75f), new Vector2(0.75f,1.25f), new Vector2(1.15f,0.85f), new Vector2(0.95f,1.05f), new Vector2(1.05f,0.95f), new Vector2(1.1f,0.9f), new Vector2(0.9f,1.1f), new Vector2(1,1)};
        float duration = 1.0f, time = 0;
        while (time < duration)
        {
            float progress = time / duration;
            int index = 1;
            while (index < times.Length && progress > times[index]) index++;
            float localProgress = (progress - times[index-1]) / (times[index] - times[index-1]);
            Vector2 currentScale = Vector2.Lerp(scales[index-1], scales[index], localProgress);
            icon.style.scale = new StyleScale(new Scale(currentScale));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Swing(VisualElement icon)
    {
        icon.style.transformOrigin = new StyleTransformOrigin(new TransformOrigin(new Length(50, LengthUnit.Percent), new Length(0)));
        float duration = 1.0f, time = 0;
        float[] angles = {0, 15, -10, 10, -5, 5, 0};
        float stepDuration = duration / (angles.Length - 1);
        for (int i = 1; i < angles.Length; i++)
        {
            float startAngle = angles[i-1];
            float endAngle = angles[i];
            time = 0;
            while (time < stepDuration)
            {
                float progress = time / stepDuration;
                float angle = Mathf.Lerp(startAngle, endAngle, progress);
                icon.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
                time += Time.deltaTime;
                yield return null;
            }
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Wobble(VisualElement icon)
    {
        float duration = 1.0f, time = 0;
        float[] trans = {0, -25, 20, -15, 10, -5, 0};
        float[] rot = {0, -5, 3, -3, 2, -1, 0};
        float stepDuration = duration / (trans.Length - 1);
        for (int i = 1; i < trans.Length; i++)
        {
            float startTrans = trans[i-1];
            float endTrans = trans[i];
            float startRot = rot[i-1];
            float endRot = rot[i];
            time = 0;
            while (time < stepDuration)
            {
                float progress = time / stepDuration;
                float currentTrans = Mathf.Lerp(startTrans, endTrans, progress);
                float currentRot = Mathf.Lerp(startRot, endRot, progress);
                icon.style.translate = new StyleTranslate(new Translate(new Length(currentTrans, LengthUnit.Percent), new Length(0)));
                icon.style.rotate = new StyleRotate(new Rotate(new Angle(currentRot, AngleUnit.Degree)));
                time += Time.deltaTime;
                yield return null;
            }
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Flash(VisualElement icon)
    {
        float duration = 1.0f;
        icon.style.opacity = 0;
        yield return new WaitForSeconds(0.25f);
        icon.style.opacity = 1;
        yield return new WaitForSeconds(0.25f);
        icon.style.opacity = 0;
        yield return new WaitForSeconds(0.25f);
        icon.style.opacity = 1;
        yield return new WaitForSeconds(0.25f);
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Pulse(VisualElement icon)
    {
        float duration = 0.5f, time = 0, scale = 1.1f;
        while (time < duration)
        {
            float progress = time / duration;
            float currentScale = 1 + (scale - 1) * Mathf.Sin(progress * Mathf.PI);
            icon.style.scale = new StyleScale(new Scale(new Vector2(currentScale, currentScale)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Bounce(VisualElement icon)
    {
        float duration = 0.75f, time = 0;
        while (time < duration)
        {
            float progress = time / duration;
            float y = -Mathf.Abs(30 * Easing.OutBounce(progress));
            icon.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(y, LengthUnit.Pixel)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Jello(VisualElement icon)
    {
        float duration = 1.0f, time = 0;
        float[] skews = {0, -12.5f, 6.25f, -3.125f, 1.5625f, -0.78125f, 0.390625f, -0.1953125f, 0.09865625f, 0};
        float stepDuration = duration / (skews.Length - 1);
        for (int i = 1; i < skews.Length; i++)
        {
            float startSkew = skews[i-1];
            float endSkew = skews[i];
            time = 0;
            while (time < stepDuration)
            {
                float progress = time / stepDuration;
                float currentSkew = Mathf.Lerp(startSkew, endSkew, progress);
                icon.style.rotate = new StyleRotate(new Rotate(new Angle(currentSkew, AngleUnit.Degree)));
                time += Time.deltaTime;
                yield return null;
            }
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_LightSpeedIn(VisualElement icon)
    {
        float duration = 1.0f, time = 0;
        icon.style.translate = new StyleTranslate(new Translate(new Length(100, LengthUnit.Percent), new Length(0)));
        icon.style.opacity = 0;
        while (time < duration)
        {
            float progress = Easing.OutCubic(time / duration);
            float x = Mathf.Lerp(100, 0, progress);
            icon.style.translate = new StyleTranslate(new Translate(new Length(x, LengthUnit.Percent), new Length(0)));
            icon.style.opacity = progress;
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_RollIn(VisualElement icon)
    {
        float duration = 1.0f, time = 0;
        icon.style.translate = new StyleTranslate(new Translate(new Length(-100, LengthUnit.Percent), new Length(0)));
        icon.style.rotate = new StyleRotate(new Rotate(new Angle(-120, AngleUnit.Degree)));
        icon.style.opacity = 0;
        while (time < duration)
        {
            float progress = Easing.OutCubic(time / duration);
            float x = Mathf.Lerp(-100, 0, progress);
            float angle = Mathf.Lerp(-120, 0, progress);
            icon.style.translate = new StyleTranslate(new Translate(new Length(x, LengthUnit.Percent), new Length(0)));
            icon.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
            icon.style.opacity = progress;
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_RotateIn(VisualElement icon)
    {
        float duration = 0.5f, time = 0;
        icon.style.rotate = new StyleRotate(new Rotate(new Angle(-200, AngleUnit.Degree)));
        icon.style.opacity = 0;
        while (time < duration)
        {
            float progress = Easing.OutQuad(time / duration);
            float angle = Mathf.Lerp(-200, 0, progress);
            icon.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
            icon.style.opacity = progress;
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_SlideInLeft(VisualElement icon)
    {
        float duration = 0.5f, time = 0;
        icon.style.translate = new StyleTranslate(new Translate(new Length(-100, LengthUnit.Percent), new Length(0)));
        while (time < duration)
        {
            float progress = Easing.OutCubic(time / duration);
            float x = Mathf.Lerp(-100, 0, progress);
            icon.style.translate = new StyleTranslate(new Translate(new Length(x, LengthUnit.Percent), new Length(0)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_SlideInRight(VisualElement icon)
    {
        float duration = 0.5f, time = 0;
        icon.style.translate = new StyleTranslate(new Translate(new Length(100, LengthUnit.Percent), new Length(0)));
        while (time < duration)
        {
            float progress = Easing.OutCubic(time / duration);
            float x = Mathf.Lerp(100, 0, progress);
            icon.style.translate = new StyleTranslate(new Translate(new Length(x, LengthUnit.Percent), new Length(0)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_SlideInUp(VisualElement icon)
    {
        float duration = 0.5f, time = 0;
        icon.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(100, LengthUnit.Percent)));
        while (time < duration)
        {
            float progress = Easing.OutCubic(time / duration);
            float y = Mathf.Lerp(100, 0, progress);
            icon.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(y, LengthUnit.Percent)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_SlideInDown(VisualElement icon)
    {
        float duration = 0.5f, time = 0;
        icon.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(-100, LengthUnit.Percent)));
        while (time < duration)
        {
            float progress = Easing.OutCubic(time / duration);
            float y = Mathf.Lerp(-100, 0, progress);
            icon.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(y, LengthUnit.Percent)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_ZoomIn(VisualElement icon)
    {
        float duration = 0.5f, time = 0;
        icon.style.scale = new StyleScale(new Scale(new Vector2(0,0)));
        while (time < duration)
        {
            float progress = Easing.OutQuad(time / duration);
            icon.style.scale = new StyleScale(new Scale(new Vector2(progress, progress)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    #endregion
}