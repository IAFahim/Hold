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
    Voxelize
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
            // Organic
            { IconAnimationType.Wiggle, icon => StartCoroutine(Animate_Wiggle(icon)) },
            { IconAnimationType.PeekABoo, icon => StartCoroutine(Animate_PeekABoo(icon)) },
            { IconAnimationType.Nod, icon => StartCoroutine(Animate_Nod(icon)) },
            { IconAnimationType.Blink, icon => StartCoroutine(Animate_Blink(icon)) },
            // Celebratory
            { IconAnimationType.Fireworks, icon => StartCoroutine(Animate_Fireworks(icon)) },
            { IconAnimationType.Spotlight, icon => StartCoroutine(Animate_Spotlight(icon)) },
            { IconAnimationType.ConfettiPop, icon => StartCoroutine(Animate_ConfettiPop(icon)) },
            // Digital
            { IconAnimationType.ScanlineReveal, icon => StartCoroutine(Animate_ScanlineReveal(icon)) },
            { IconAnimationType.Glitch, icon => StartCoroutine(Animate_Glitch(icon)) },
            { IconAnimationType.Pixelate, icon => StartCoroutine(Animate_Pixelate(icon)) },
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

    // --- Category: Celebratory & Emphatic ---
    private IEnumerator Animate_Fireworks(VisualElement icon)
    {
        var container = icon.parent;
        if (container == null) yield break;
        int particleCount = 10;
        for (int i = 0; i < particleCount; i++)
        {
            var particle = new VisualElement();
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

    #endregion
}