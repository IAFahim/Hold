using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using Random = UnityEngine.Random;

#region Animation Enum (The Full Library)

public enum IconAnimationType
{
    // --- Foundational & Core (15) ---
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
    Boing,
    SwipeIn,
    DoubleTap,
    Thump,
    FadeOutAndIn,

    // --- Physical & Material (15) ---
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
    Melt,
    PageTurn,
    RubberBand,
    WindGust,
    Unfurl,

    // --- Organic & Playful (15) ---
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
    Dizzy,
    Jitter,

    // --- Celebratory & Emphatic (15) ---
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
    SpotlightSweep,
    SonarPing,

    // --- Digital & Futuristic (15) ---
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
    SpiralIn,
    PowerDown,
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
            ResetIconStyle(navIcons[i]);
        }

        root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    #region Core Logic

    private void InitializeAnimationLibrary()
    {
        animationLibrary = new Dictionary<IconAnimationType, Func<VisualElement, Coroutine>>
        {
            // --- Foundational ---
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
            { IconAnimationType.Boing, icon => StartCoroutine(Animate_Boing(icon)) },
            { IconAnimationType.SwipeIn, icon => StartCoroutine(Animate_SwipeIn(icon)) },
            { IconAnimationType.DoubleTap, icon => StartCoroutine(Animate_DoubleTap(icon)) },
            { IconAnimationType.Thump, icon => StartCoroutine(Animate_Thump(icon)) },
            { IconAnimationType.FadeOutAndIn, icon => StartCoroutine(Animate_FadeOutAndIn(icon)) },

            // --- Physical ---
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
            { IconAnimationType.Melt, icon => StartCoroutine(Animate_Melt(icon)) },
            { IconAnimationType.PageTurn, icon => StartCoroutine(Animate_PageTurn(icon)) },
            { IconAnimationType.RubberBand, icon => StartCoroutine(Animate_RubberBand(icon)) },
            { IconAnimationType.WindGust, icon => StartCoroutine(Animate_WindGust(icon)) },
            { IconAnimationType.Unfurl, icon => StartCoroutine(Animate_Unfurl(icon)) },

            // --- Organic ---
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
            { IconAnimationType.Dizzy, icon => StartCoroutine(Animate_Dizzy(icon)) },
            { IconAnimationType.Jitter, icon => StartCoroutine(Animate_Jitter(icon)) },

            // --- Celebratory ---
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
            { IconAnimationType.SpotlightSweep, icon => StartCoroutine(Animate_SpotlightSweep(icon)) },
            { IconAnimationType.SonarPing, icon => StartCoroutine(Animate_SonarPing(icon)) },

            // --- Digital ---
            { IconAnimationType.ScanlineReveal, icon => StartCoroutine(Animate_ScanlineReveal(icon)) },
            { IconAnimationType.Glitch, icon => StartCoroutine(Animate_Glitch(icon)) },
            { IconAnimationType.Pixelate, icon => StartCoroutine(Animate_Pixelate(icon)) },
            { IconAnimationType.ChargingPulse, icon => StartCoroutine(Animate_ChargingPulse(icon)) },
            { IconAnimationType.CodeMatrix, icon => StartCoroutine(Animate_CodeMatrix(icon)) },
            { IconAnimationType.Hologram, icon => StartCoroutine(Animate_Hologram(icon)) },
            { IconAnimationType.FocusLock, icon => StartCoroutine(Animate_FocusLock(icon)) },
            { IconAnimationType.DataStream, icon => StartCoroutine(Animate_DataStream(icon)) },
            { IconAnimationType.Reboot, icon => StartCoroutine(Animate_Reboot(icon)) },
            { IconAnimationType.Voxelize, icon => StartCoroutine(Animate_Voxelize(icon)) },
            { IconAnimationType.SpiralIn, icon => StartCoroutine(Animate_SpiralIn(icon)) },
            { IconAnimationType.PowerDown, icon => StartCoroutine(Animate_PowerDown(icon)) },
            { IconAnimationType.KineticChain, icon => StartCoroutine(Animate_KineticChain(icon)) },
            { IconAnimationType.FlywheelSpin, icon => StartCoroutine(Animate_FlywheelSpin(icon)) },
        };
        animationKeys = animationLibrary.Keys.ToList();
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        if (currentIndex == -1 && navButtons.Count > 0) SelectButton(0, false);
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
        icon.style.opacity = 1;
        icon.style.transformOrigin = new StyleTransformOrigin(new TransformOrigin(new Length(50, LengthUnit.Percent),
            new Length(50, LengthUnit.Percent)));
        icon.style.borderTopLeftRadius = icon.style.borderTopRightRadius = 
            icon.style.borderBottomLeftRadius = icon.style.borderBottomRightRadius = 
            new StyleLength(new Length(0));
        icon.style.overflow = Overflow.Visible;
            

        var container = icon.parent;
        if (container == null) return;
        
        // Clean up any dynamically created particle/effect elements
        var particles = container.Query<VisualElement>(className: "particle-effect").ToList();
        foreach (var particle in particles) particle.RemoveFromHierarchy();

        // Reset specific effect elements
        Action<string, Action<VisualElement>> resetEffect = (className, styleAction) =>
        {
            var effectElement = container.Q<VisualElement>(className: className);
            if (effectElement != null) styleAction(effectElement);
        };

        resetEffect("scanline-effect", el => { el.style.opacity = 0; el.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(-100, LengthUnit.Percent))); });
        resetEffect("spotlight-effect", el => el.style.opacity = 0);
        resetEffect("ripple-effect", el => { el.style.opacity = 0; el.style.scale = new StyleScale(new Scale(Vector2.zero)); });
        resetEffect("liquid-fill-effect", el => { el.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(100, LengthUnit.Percent))); });
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
        float duration = 0.4f, time = 0, shakeAmount = 8f;
        while (time < duration)
        {
            float progress = time / duration;
            float currentX = Mathf.Sin(progress * Mathf.PI * 10) * (shakeAmount * (1 - progress));
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
        float duration = 0.5f, time = 0, windUpAngle = -30f, spinAngle = 360f;
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
        float duration = 0.7f;
        float time = 0f;
        while (time < duration)
        {
            float progress = time / duration;
            float damp = 1 - Mathf.Pow(progress, 3);
            float stretch = 1 + 0.3f * damp * Mathf.Sin(progress * Mathf.PI * 3.5f);
            float squash = 1 - 0.3f * damp * Mathf.Sin(progress * Mathf.PI * 3.5f);
            icon.style.scale = new StyleScale(new Scale(new Vector2(squash, stretch)));
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
        float duration = 0.4f;
        float time = 0f;
        float halfDuration = duration / 2f;
        
        icon.style.scale = new StyleScale(new Scale(new Vector2(0, 1)));
        icon.style.transformOrigin = new StyleTransformOrigin(new TransformOrigin(new Length(0, LengthUnit.Percent), new Length(50, LengthUnit.Percent)));

        while (time < halfDuration)
        {
            float progress = Easing.OutCubic(time / halfDuration);
            icon.style.scale = new StyleScale(new Scale(new Vector2(progress, 1)));
            time += Time.deltaTime;
            yield return null;
        }

        time = 0f;
        icon.style.transformOrigin = new StyleTransformOrigin(new TransformOrigin(new Length(50, LengthUnit.Percent), new Length(0, LengthUnit.Percent)));
        icon.style.scale = new StyleScale(new Scale(new Vector2(1, 0)));

        while (time < halfDuration)
        {
            float progress = Easing.OutCubic(time / halfDuration);
            icon.style.scale = new StyleScale(new Scale(new Vector2(1, progress)));
            time += Time.deltaTime;
            yield return null;
        }

        ResetIconStyle(icon);
    }
    
    private IEnumerator Animate_Boing(VisualElement icon)
    {
        float duration = 0.6f, time = 0, squashY = 0.7f, stretchY = 1.3f;
        float squashDuration = duration * 0.3f;

        while (time < squashDuration)
        {
            float progress = Easing.OutCubic(time / squashDuration);
            float scaleY = Mathf.Lerp(1f, squashY, progress);
            float scaleX = Mathf.Lerp(1f, 1 + (1 - scaleY), progress);
            icon.style.scale = new StyleScale(new Scale(new Vector2(scaleX, scaleY)));
            time += Time.deltaTime;
            yield return null;
        }

        time = 0;
        float bounceDuration = duration * 0.7f;
        while (time < bounceDuration)
        {
            float progress = EasingExtra.OutElastic(time / bounceDuration, 1.2f, 0.4f);
            float scaleY = Mathf.Lerp(squashY, 1f, progress);
            float scaleX = Mathf.Lerp(1 + (1-squashY), 1f, progress);
            icon.style.scale = new StyleScale(new Scale(new Vector2(scaleX, scaleY)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_SwipeIn(VisualElement icon)
    {
        float duration = 0.5f, time = 0, startX = -80f;
        while (time < duration)
        {
            float progress = Easing.OutBack(time / duration, 1.5f);
            icon.style.translate = new StyleTranslate(new Translate(new Length(Mathf.Lerp(startX, 0, progress), LengthUnit.Pixel), new Length(0)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }
    
    private IEnumerator Animate_DoubleTap(VisualElement icon)
    {
        float duration = 0.3f;
        yield return Animate_QuickTap(icon, 0.8f, duration / 2f);
        yield return new WaitForSeconds(0.05f);
        yield return Animate_QuickTap(icon, 0.8f, duration / 2f);
        ResetIconStyle(icon);
    }
    
    private IEnumerator Animate_QuickTap(VisualElement icon, float scale, float duration)
    {
        float time = 0f;
        float half = duration / 2f;
        while(time < half)
        {
            icon.style.scale = new StyleScale(new Scale(new Vector2(Mathf.Lerp(1f, scale, time/half), Mathf.Lerp(1f, scale, time/half))));
            time += Time.deltaTime;
            yield return null;
        }
        while(time < duration)
        {
            icon.style.scale = new StyleScale(new Scale(new Vector2(Mathf.Lerp(scale, 1f, (time-half)/half), Mathf.Lerp(scale, 1f, (time-half)/half))));
            time += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator Animate_Thump(VisualElement icon)
    {
        float duration = 0.25f, time = 0, scale = 1.15f;
        while (time < duration)
        {
            float progress = Easing.OutCubic(time / duration);
            float currentScale = 1f + (scale - 1f) * Mathf.Sin(progress * Mathf.PI);
            icon.style.scale = new StyleScale(new Scale(new Vector2(currentScale, currentScale)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_FadeOutAndIn(VisualElement icon)
    {
        float duration = 0.6f, time = 0;
        float half = duration / 2f;
        while (time < half)
        {
            icon.style.opacity = 1 - (time / half);
            time += Time.deltaTime;
            yield return null;
        }
        time = 0f;
        while (time < half)
        {
            icon.style.opacity = time / half;
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
            float progress = time / duration;
            float angle = Mathf.Lerp(0, 180, progress);
            // We can't do a true 3D rotate, so we simulate it with scale
            icon.style.scale = new StyleScale(new Scale(new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), 1)));
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
        // NOTE: Requires a child <VisualElement class="ripple-effect" />
        var ripple = icon.parent?.Q<VisualElement>(className: "ripple-effect");
        if (ripple == null) { yield return Animate_ElasticPop(icon); yield break; }

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
        icon.style.opacity = 1;
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
        float duration = 0.8f, time = 0, maxAngle = 45f;
        icon.style.transformOrigin = new StyleTransformOrigin(new TransformOrigin(new Length(50, LengthUnit.Percent), new Length(-100, LengthUnit.Percent)));
        while (time < duration)
        {
            float progress = time / duration;
            float damp = Mathf.Exp(-progress * 4f);
            float angle = maxAngle * Mathf.Sin(progress * Mathf.PI * 4) * damp;
            icon.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_LiquidFill(VisualElement icon)
    {
        // NOTE: Requires a child <VisualElement class="liquid-fill-effect" /> and `overflow: hidden` on the icon's USS.
        var liquid = icon.parent?.Q<VisualElement>(className: "liquid-fill-effect");
        if (liquid == null) { yield return Animate_ElasticPop(icon); yield break; }
        
        icon.style.overflow = Overflow.Hidden;
        float duration = 0.7f, time = 0;
        while (time < duration)
        {
            float progress = Easing.OutCubic(time / duration);
            liquid.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(Mathf.Lerp(100, 0, progress), LengthUnit.Percent)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Shutter(VisualElement icon)
    {
        float duration = 0.3f, time = 0;
        float half = duration / 2f;
        while (time < half)
        {
            float progress = time / half;
            icon.style.scale = new StyleScale(new Scale(new Vector2(1, 1 - progress)));
            time += Time.deltaTime;
            yield return null;
        }
        time = 0;
        while (time < half)
        {
            float progress = time / half;
            icon.style.scale = new StyleScale(new Scale(new Vector2(1, progress)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_OrigamiFold(VisualElement icon)
    {
        float duration = 0.5f, time = 0;
        float half = duration / 2f;
        icon.style.transformOrigin = new StyleTransformOrigin(new TransformOrigin(new Length(0), new Length(0)));
        while (time < half)
        {
            float progress = Easing.OutCubic(time / half);
            icon.style.scale = new StyleScale(new Scale(new Vector2(1 - progress, 1)));
            time += Time.deltaTime;
            yield return null;
        }
        time = 0;
        while (time < half)
        {
            float progress = Easing.OutCubic(time / half);
            icon.style.scale = new StyleScale(new Scale(new Vector2(progress, 1)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Magnetize(VisualElement icon)
    {
        float duration = 0.5f, time = 0;
        Vector2 startPos = Random.insideUnitCircle * 40f;
        while (time < duration)
        {
            float progress = Easing.OutBack(time / duration, 2.5f);
            icon.style.translate = new StyleTranslate(new Translate(new Length(Mathf.Lerp(startPos.x, 0, progress), LengthUnit.Pixel), new Length(Mathf.Lerp(startPos.y, 0, progress), LengthUnit.Pixel)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Melt(VisualElement icon)
    {
        float duration = 0.8f, time = 0;
        icon.style.transformOrigin = new StyleTransformOrigin(new TransformOrigin(new Length(50, LengthUnit.Percent), new Length(100, LengthUnit.Percent)));
        float half = duration / 2f;
        while (time < half)
        {
            float progress = Easing.InCubic(time / half);
            icon.style.scale = new StyleScale(new Scale(new Vector2(1f, 1 - progress * 0.7f)));
            icon.style.borderBottomLeftRadius = icon.style.borderBottomRightRadius = new StyleLength(new Length(progress * 50, LengthUnit.Percent));
            time += Time.deltaTime;
            yield return null;
        }
        time = 0;
        while (time < half)
        {
            float progress = Easing.OutElastic(time / half);
            icon.style.scale = new StyleScale(new Scale(new Vector2(1f, Mathf.Lerp(0.3f, 1f, progress))));
            icon.style.borderBottomLeftRadius = icon.style.borderBottomRightRadius = new StyleLength(new Length(Mathf.Lerp(50, 0, progress), LengthUnit.Percent));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_PageTurn(VisualElement icon)
    {
        float duration = 0.5f, time = 0;
        icon.style.transformOrigin = new StyleTransformOrigin(new TransformOrigin(new Length(100, LengthUnit.Percent), new Length(100, LengthUnit.Percent)));
        while (time < duration)
        {
            float progress = Easing.InOutCubic(time / duration);
            icon.style.scale = new StyleScale(new Scale(new Vector2(Mathf.Cos(progress * Mathf.PI * 0.5f), 1)));
            icon.style.opacity = 1 - progress;
            time += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.1f);
        ResetIconStyle(icon);
    }
    
    private IEnumerator Animate_RubberBand(VisualElement icon)
    {
        float duration = 0.7f, time = 0, stretch = 1.5f;
        while (time < duration)
        {
            float progress = EasingExtra.OutElastic(time / duration, 1.5f, 0.3f);
            icon.style.scale = new StyleScale(new Scale(new Vector2(Mathf.Lerp(stretch, 1f, progress), Mathf.Lerp(1 / stretch, 1f, progress))));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_WindGust(VisualElement icon)
    {
        float duration = 0.8f, time = 0, angle = 10f, xOffset = 20f;
        while (time < duration)
        {
            float progress = time / duration;
            float wave = Mathf.Sin(progress * Mathf.PI * 4);
            float damp = 1 - progress;
            icon.style.rotate = new StyleRotate(new Rotate(new Angle(wave * angle * damp, AngleUnit.Degree)));
            icon.style.translate = new StyleTranslate(new Translate(new Length(wave * xOffset * damp, LengthUnit.Pixel), new Length(0)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }
    
    private IEnumerator Animate_Unfurl(VisualElement icon)
    {
        float duration = 0.6f, time = 0;
        icon.style.transformOrigin = new StyleTransformOrigin(new TransformOrigin(new Length(50, LengthUnit.Percent), new Length(0, LengthUnit.Percent)));
        while (time < duration)
        {
            float progress = Easing.OutCubic(time / duration);
            icon.style.scale = new StyleScale(new Scale(new Vector2(1, progress)));
            icon.style.opacity = progress;
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
        float duration = 0.6f, time = 0, maxAngle = 25f;
        icon.style.transformOrigin = new StyleTransformOrigin(new TransformOrigin(new Length(0, LengthUnit.Percent), new Length(50, LengthUnit.Percent)));
        while (time < duration)
        {
            float progress = time / duration;
            float angle = Mathf.Sin(progress * Mathf.PI * 5) * (maxAngle * (1-progress));
            icon.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Gasp(VisualElement icon)
    {
        float duration = 0.4f, time = 0;
        float gaspDuration = duration * 0.3f;
        while (time < gaspDuration)
        {
            float progress = Easing.OutCubic(time / gaspDuration);
            icon.style.scale = new StyleScale(new Scale(new Vector2(Mathf.Lerp(1, 1.3f, progress), Mathf.Lerp(1, 1.3f, progress))));
            time += Time.deltaTime;
            yield return null;
        }
        time = 0;
        float releaseDuration = duration * 0.7f;
        while (time < releaseDuration)
        {
            float progress = Easing.OutBounce(time / releaseDuration);
            icon.style.scale = new StyleScale(new Scale(new Vector2(Mathf.Lerp(1.3f, 1f, progress), Mathf.Lerp(1.3f, 1f, progress))));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Crawl(VisualElement icon)
    {
        float duration = 1.0f, time = 0;
        while (time < duration)
        {
            float progress = time / duration;
            float xPos = (progress - 0.5f) * 40f;
            float yPos = Mathf.Abs(Mathf.Sin(progress * Mathf.PI * 4)) * 5f;
            icon.style.translate = new StyleTranslate(new Translate(new Length(xPos, LengthUnit.Pixel), new Length(yPos, LengthUnit.Pixel)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Float(VisualElement icon)
    {
        float duration = 1.5f, time = 0, height = 8f;
        while (time < duration)
        {
            float yPos = Mathf.Sin(Time.time * 4f) * height;
            icon.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(yPos, LengthUnit.Pixel)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Scurry(VisualElement icon)
    {
        float duration = 0.4f, time = 0;
        while (time < duration)
        {
            float xPos = (Random.value - 0.5f) * 10f;
            float yPos = (Random.value - 0.5f) * 4f;
            icon.style.translate = new StyleTranslate(new Translate(new Length(xPos, LengthUnit.Pixel), new Length(yPos, LengthUnit.Pixel)));
            time += Time.deltaTime;
            yield return new WaitForSeconds(0.02f);
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Exhale(VisualElement icon)
    {
        float duration = 0.8f, time = 0;
        while (time < duration)
        {
            float progress = time / duration;
            float scale = 1 - 0.2f * Easing.OutCubic(progress);
            icon.style.scale = new StyleScale(new Scale(new Vector2(scale, scale)));
            icon.style.opacity = 1 - 0.5f * Easing.OutCubic(progress);
            time += Time.deltaTime;
            yield return null;
        }
        yield return Animate_ElasticPop(icon);
    }

    private IEnumerator Animate_Dizzy(VisualElement icon)
    {
        float duration = 1.0f, time = 0;
        while (time < duration)
        {
            float progress = time / duration;
            float angle = progress * 360f * 2;
            float xOffset = Mathf.Sin(progress * Mathf.PI * 6) * 10 * (1 - progress);
            icon.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
            icon.style.translate = new StyleTranslate(new Translate(new Length(xOffset, LengthUnit.Pixel), new Length(0)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Jitter(VisualElement icon)
    {
        float duration = 0.3f, time = 0;
        while (time < duration)
        {
            icon.style.translate = new StyleTranslate(new Translate(
                new Length(Random.Range(-2f, 2f), LengthUnit.Pixel),
                new Length(Random.Range(-2f, 2f), LengthUnit.Pixel)
            ));
            time += Time.deltaTime;
            yield return null;
        }
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
            var particle = new VisualElement();
            particle.AddToClassList("particle-effect"); // For easy cleanup
            particle.style.position = Position.Absolute;
            particle.style.width = 5;
            particle.style.height = 5;
            particle.style.backgroundColor = Color.HSVToRGB(Random.value, 0.8f, 1f);
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
        // NOTE: Requires a child <VisualElement class="spotlight-effect" />
        var spotlight = icon.parent?.Q<VisualElement>(className: "spotlight-effect");
        if (spotlight == null) { yield return Animate_ElasticPop(icon); yield break; }

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
            particle.AddToClassList("particle-effect");
            container.Add(particle);
            StartCoroutine(AnimateConfettiParticle(particle, 1.0f));
        }

        yield return Animate_SqueezeAndPop(icon);
    }

    private IEnumerator AnimateConfettiParticle(VisualElement particle, float duration)
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
        float duration = 0.8f, time = 0;
        while (time < duration)
        {
            float progress = Easing.OutBounce(time/duration);
            icon.style.scale = new StyleScale(new Scale(new Vector2(Mathf.Lerp(1.5f, 1f, progress), Mathf.Lerp(1.5f, 1f, progress))));
            icon.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(Mathf.Lerp(-30f, 0, progress), LengthUnit.Pixel)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Shockwave(VisualElement icon)
    {
        // Use the ripple effect element for this
        yield return Animate_RippleOut(icon);
        yield return Animate_Thump(icon);
    }

    private IEnumerator Animate_Stomp(VisualElement icon)
    {
        float duration = 0.5f, time = 0;
        float fallDuration = duration * 0.2f;
        while (time < fallDuration)
        {
            float progress = Easing.InCubic(time / fallDuration);
            icon.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(Mathf.Lerp(-30, 0, progress), LengthUnit.Pixel)));
            time += Time.deltaTime;
            yield return null;
        }
        
        yield return Animate_SqueezeAndPop(icon);
    }

    private IEnumerator Animate_VictoryRise(VisualElement icon)
    {
        float duration = 0.7f, time = 0;
        while (time < duration)
        {
            float progress = Easing.OutCubic(time/duration);
            icon.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(Mathf.Lerp(0, -20, progress), LengthUnit.Pixel)));
            icon.style.scale = new StyleScale(new Scale(new Vector2(1 + 0.2f * progress, 1 + 0.2f * progress)));
            time += Time.deltaTime;
            yield return null;
        }
        yield return Animate_ElasticPop(icon);
    }

    private IEnumerator Animate_Applause(VisualElement icon)
    {
        float duration = 0.5f, time = 0;
        while (time < duration)
        {
            float progress = time / duration;
            float offset = Mathf.Sin(progress * Mathf.PI * 12) * 5 * (1 - progress);
            icon.style.translate = new StyleTranslate(new Translate(new Length(offset, LengthUnit.Pixel), new Length(0)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_PowerUp(VisualElement icon)
    {
        yield return Animate_QuickTap(icon, 1.2f, 0.15f);
        yield return Animate_QuickTap(icon, 1.3f, 0.15f);
        yield return Animate_ElasticPop(icon);
    }

    private IEnumerator Animate_Fanfare(VisualElement icon)
    {
        float duration = 0.6f, time = 0, angle = 15f;
        while (time < duration)
        {
            float progress = Easing.OutElastic(time / duration);
            icon.style.scale = new StyleScale(new Scale(new Vector2(progress, progress)));
            icon.style.rotate = new StyleRotate(new Rotate(new Angle(Mathf.Lerp(-angle, 0, progress), AngleUnit.Degree)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_SpotlightSweep(VisualElement icon)
    {
        var spotlight = icon.parent?.Q<VisualElement>(className: "spotlight-effect");
        if (spotlight == null) { yield return Animate_ElasticPop(icon); yield break; }

        float duration = 1.0f, time = 0;
        spotlight.style.opacity = 1;
        while (time < duration)
        {
            float progress = time / duration;
            float angle = Mathf.Sin(progress * Mathf.PI * 2) * 45f;
            spotlight.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_SonarPing(VisualElement icon)
    {
        var container = icon.parent;
        if (container == null) yield break;
        
        for (int i = 0; i < 3; i++)
        {
            var ping = new VisualElement { name = "ping" };
            ping.AddToClassList("particle-effect");
            ping.style.position = Position.Absolute;
            ping.style.width = ping.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            ping.style.borderTopWidth = ping.style.borderBottomWidth = ping.style.borderLeftWidth = ping.style.borderRightWidth = 2;
            ping.style.borderTopColor = ping.style.borderBottomColor = ping.style.borderLeftColor = ping.style.borderRightColor = Color.white;
            ping.style.borderTopLeftRadius = ping.style.borderTopRightRadius = ping.style.borderBottomLeftRadius = ping.style.borderBottomRightRadius = new StyleLength(new Length(50, LengthUnit.Percent));

            container.Add(ping);
            StartCoroutine(AnimatePingParticle(ping, 0.8f));
            yield return new WaitForSeconds(0.15f);
        }
        yield return new WaitForSeconds(0.5f);
        ResetIconStyle(icon);
    }

    private IEnumerator AnimatePingParticle(VisualElement ping, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            float progress = Easing.OutCubic(time / duration);
            ping.style.scale = new StyleScale(new Scale(new Vector2(1 + progress * 2f, 1 + progress * 2f)));
            ping.style.opacity = 1 - progress;
            time += Time.deltaTime;
            yield return null;
        }
        ping.RemoveFromHierarchy();
    }


    // --- Category: Digital & Futuristic ---
    private IEnumerator Animate_ScanlineReveal(VisualElement icon)
    {
        // NOTE: Requires a child <VisualElement class="scanline-effect" />
        var scanline = icon.parent?.Q<VisualElement>(className: "scanline-effect");
        if (scanline == null) { yield return Animate_ElasticPop(icon); yield break; }

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
    
    // Placeholder - true pixelation requires shaders or asset swapping.
    private IEnumerator Animate_Pixelate(VisualElement icon)
    {
        float duration = 0.4f, time = 0;
        float half = duration / 2f;
        while(time < half)
        {
            float progress = Easing.InCubic(time / half);
            icon.style.scale = new StyleScale(new Scale(new Vector2(1, 1 - progress)));
            time += Time.deltaTime;
            yield return null;
        }
        time = 0;
        while(time < half)
        {
            float progress = Easing.OutCubic(time / half);
            icon.style.scale = new StyleScale(new Scale(new Vector2(1, progress)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_ChargingPulse(VisualElement icon)
    {
        float duration = 1.2f, time = 0, maxScale = 1.1f;
        while (time < duration)
        {
            float scale = 1 + (maxScale - 1) * Mathf.Sin(Time.time * 8f) * 0.5f + 0.5f;
            icon.style.scale = new StyleScale(new Scale(new Vector2(scale, scale)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_CodeMatrix(VisualElement icon)
    {
        yield return Animate_DataStream(icon, true);
    }

    private IEnumerator Animate_Hologram(VisualElement icon)
    {
        float duration = 0.8f, time = 0;
        var scanline = icon.parent?.Q<VisualElement>(className: "scanline-effect");
        if (scanline != null) scanline.style.opacity = 0.5f;

        while (time < duration)
        {
            icon.style.opacity = Random.Range(0.7f, 1.0f);
            icon.style.translate = new StyleTranslate(new Translate(new Length(Random.Range(-1f,1f), LengthUnit.Pixel), new Length(0)));
            if (scanline != null)
            {
                scanline.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(Random.Range(-50, 50), LengthUnit.Percent)));
            }
            time += Time.deltaTime;
            yield return new WaitForSeconds(0.05f);
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_FocusLock(VisualElement icon)
    {
        float duration = 0.4f, time = 0;
        while (time < duration)
        {
            float progress = Easing.OutCubic(time / duration);
            float scale = Mathf.Lerp(1.5f, 1f, progress);
            icon.style.scale = new StyleScale(new Scale(new Vector2(scale, scale)));
            icon.style.opacity = progress;
            time += Time.deltaTime;
            yield return null;
        }
        yield return Animate_Thump(icon);
    }

    private IEnumerator Animate_DataStream(VisualElement icon, bool vertical = false)
    {
        var container = icon.parent;
        if (container == null) yield break;
        icon.style.opacity = 0;
        for (int i = 0; i < 15; i++)
        {
            var bit = new VisualElement();
            bit.AddToClassList("particle-effect");
            bit.style.position = Position.Absolute;
            bit.style.backgroundColor = Color.green;
            bit.style.width = 5;
            bit.style.height = 5;
            container.Add(bit);
            StartCoroutine(AnimateDataBit(bit, 0.5f, vertical));
            yield return new WaitForSeconds(0.02f);
        }
        icon.style.opacity = 1;
        ResetIconStyle(icon);
    }
    
    private IEnumerator AnimateDataBit(VisualElement bit, float duration, bool vertical)
    {
        float time = 0;
        float startX = vertical ? Random.Range(-20f, 20f) : -40f;
        float startY = vertical ? -40f : Random.Range(-20f, 20f);
        float endX = vertical ? startX : 40f;
        float endY = vertical ? 40f : startY;

        while (time < duration)
        {
            float progress = Easing.InCubic(time / duration);
            bit.style.translate = new StyleTranslate(new Translate(
                new Length(Mathf.Lerp(startX, endX, progress), LengthUnit.Pixel), 
                new Length(Mathf.Lerp(startY, endY, progress), LengthUnit.Pixel)));
            bit.style.opacity = 1 - progress;
            time += Time.deltaTime;
            yield return null;
        }
        bit.RemoveFromHierarchy();
    }

    private IEnumerator Animate_Reboot(VisualElement icon)
    {
        yield return Animate_FadeOutAndIn(icon);
        yield return Animate_ElasticPop(icon);
    }

    private IEnumerator Animate_Voxelize(VisualElement icon)
    {
        // Stand-in effect for true voxelization
        float duration = 0.5f, time = 0;
        float half = duration / 2f;
        icon.style.scale = new StyleScale(new Scale(new Vector2(0,0)));
        while(time < half)
        {
            float progress = Easing.OutCubic(time/half);
            icon.style.scale = new StyleScale(new Scale(new Vector2(progress, 0.1f)));
            time += Time.deltaTime;
            yield return null;
        }
        time = 0;
        while(time < half)
        {
            float progress = Easing.OutCubic(time/half);
            icon.style.scale = new StyleScale(new Scale(new Vector2(1, Mathf.Lerp(0.1f, 1, progress))));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }
    
    private IEnumerator Animate_SpiralIn(VisualElement icon)
    {
        float duration = 0.6f, time = 0, startRadius = 80f, rotations = 2f;
        while (time < duration)
        {
            float progress = Easing.OutCubic(time / duration);
            float angle = progress * 360f * rotations;
            float radius = Mathf.Lerp(startRadius, 0, progress);
            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            float y = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
            icon.style.translate = new StyleTranslate(new Translate(new Length(x, LengthUnit.Pixel), new Length(y, LengthUnit.Pixel)));
            icon.style.scale = new StyleScale(new Scale(new Vector2(progress, progress)));
            time += Time.deltaTime;
            yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_PowerDown(VisualElement icon)
    {
        float duration = 0.5f, time = 0;
        while (time < duration)
        {
            float progress = Easing.InCubic(time / duration);
            icon.style.scale = new StyleScale(new Scale(new Vector2(1, 1 - progress)));
            icon.style.opacity = 1 - progress;
            time += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
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

    #endregion
}


#region Easing Class
/// <summary>
/// A utility class for easing functions.
/// Functions are based on the equations from http://robertpenner.com/easing/
/// </summary>
public static class EasingExtra
{
    public static float InQuad(float k) => k * k;
    public static float OutQuad(float k) => k * (2f - k);
    public static float InOutQuad(float k)
    {
        if ((k *= 2f) < 1f) return 0.5f * k * k;
        return -0.5f * ((k -= 1f) * (k - 2f) - 1f);
    }

    public static float InCubic(float k) => k * k * k;
    public static float OutCubic(float k) => 1f + ((k -= 1f) * k * k);

    public static float InOutCubic(float k)
    {
        if ((k *= 2f) < 1f) return 0.5f * k * k * k;
        return 0.5f * ((k -= 2f) * k * k + 2f);
    }

    public static float InBack(float k, float s = 1.70158f) => k * k * ((s + 1f) * k - s);
    public static float OutBack(float k, float s = 1.70158f) => (k -= 1f) * k * ((s + 1f) * k + s) + 1f;

    public static float InOutBack(float k, float s = 1.70158f)
    {
        s *= 1.525f;
        if ((k *= 2f) < 1f) return 0.5f * (k * k * ((s + 1f) * k - s));
        return 0.5f * ((k -= 2f) * k * ((s + 1f) * k + s) + 2f);
    }
    
    public static float OutBounce(float k)
    {
        if (k < (1f / 2.75f))
        {
            return 7.5625f * k * k;
        }
        if (k < (2f / 2.75f))
        {
            return 7.5625f * (k -= (1.5f / 2.75f)) * k + 0.75f;
        }
        if (k < (2.5f / 2.75f))
        {
            return 7.5625f * (k -= (2.25f / 2.75f)) * k + 0.9375f;
        }
        return 7.5625f * (k -= (2.625f / 2.75f)) * k + 0.984375f;
    }

    public static float OutElastic(float k, float amplitude = 1f, float period = 0.3f)
    {
        if (k == 0) return 0;
        if (k == 1) return 1;

        float s;
        if (amplitude < 1)
        {
            amplitude = 1;
            s = period / 4f;
        }
        else
        {
            s = period / (2f * Mathf.PI) * Mathf.Asin(1f / amplitude);
        }

        return (amplitude * Mathf.Pow(2f, -10f * k) * Mathf.Sin((k - s) * (2f * Mathf.PI) / period) + 1f);
    }
}
#endregion