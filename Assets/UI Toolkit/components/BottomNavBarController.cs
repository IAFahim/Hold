using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
// Add this using statement to access the built-in Easing functions easily
using UnityEngine.UIElements.Experimental; 
using Random = UnityEngine.Random;

public enum IconAnimationType
{
    // Implemented Animations
    WindUpAndSpin,
    FlickAndSettle,
    JellyBounce,
    SlingShot,
    DropInAndBounce,
    Tada,
    Heartbeat,
    Shake,
    ElasticPop,
    Unfold,

    // Conceptual Animations
    ConfettiPop, NewtonCradle, Wiggle, PeekABoo, FocusPulse, Glitch, Charging, LiquidFill, Flip, Squeeze
}

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
    private const string SELECTED_ICON_CLASS = "nav-button__icon--selected";

    void OnEnable()
    {
        if (uiDocument == null) return;
        InitializeAnimationLibrary();
        root = uiDocument.rootVisualElement;
        selectionIndicator = root.Q<VisualElement>("SelectionIndicator");
        navButtons = root.Query<Button>(className: "nav-button").ToList();
        navIcons = root.Query<VisualElement>(className: "nav-button__icon").ToList();
        for (int i = 0; i < navButtons.Count; i++) { int index = i; navButtons[i].RegisterCallback<ClickEvent>(evt => SelectButton(index)); }
        root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    private void InitializeAnimationLibrary()
    {
        animationLibrary = new Dictionary<IconAnimationType, Func<VisualElement, Coroutine>>
        {
            { IconAnimationType.WindUpAndSpin, icon => StartCoroutine(Animate_WindUpAndSpin(icon)) },
            { IconAnimationType.FlickAndSettle, icon => StartCoroutine(Animate_FlickAndSettle(icon)) },
            { IconAnimationType.JellyBounce, icon => StartCoroutine(Animate_JellyBounce(icon)) },
            { IconAnimationType.SlingShot, icon => StartCoroutine(Animate_SlingShot(icon)) },
            { IconAnimationType.DropInAndBounce, icon => StartCoroutine(Animate_DropInAndBounce(icon)) },
            { IconAnimationType.Tada, icon => StartCoroutine(Animate_Tada(icon)) },
            { IconAnimationType.Heartbeat, icon => StartCoroutine(Animate_Heartbeat(icon)) },
            { IconAnimationType.Shake, icon => StartCoroutine(Animate_Shake(icon)) },
            { IconAnimationType.ElasticPop, icon => StartCoroutine(Animate_ElasticPop(icon)) },
            { IconAnimationType.Unfold, icon => StartCoroutine(Animate_Unfold(icon)) },
        };
        animationKeys = animationLibrary.Keys.ToList();
    }

    #region Core Logic (Unchanged)
    private void OnGeometryChanged(GeometryChangedEvent evt) { if (currentIndex == -1 && navButtons.Count > 0) SelectButton(navButtons.Count - 1, false); root.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged); }
    private void SelectButton(int index, bool animated = true) { if (index < 0 || index >= navButtons.Count || index == currentIndex) return; if (currentAnimation != null) { StopCoroutine(currentAnimation); if (currentIndex != -1) ResetIconStyle(navIcons[currentIndex]); } for (int i = 0; i < navIcons.Count; i++) navIcons[i].EnableInClassList(SELECTED_ICON_CLASS, i == index); Button selectedButton = navButtons[index]; float targetX = selectedButton.layout.xMin + (selectedButton.layout.width / 2f) - (selectionIndicator.layout.width / 2f); var duration = animated ? 0.4f : 0f; selectionIndicator.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(duration, TimeUnit.Second) }); selectionIndicator.style.left = targetX; if (animated) PlayRandomAnimation(navIcons[index]); currentIndex = index; }
    private void PlayRandomAnimation(VisualElement icon) { int randomIndex = Random.Range(0, animationKeys.Count); IconAnimationType randomAnimationType = animationKeys[randomIndex]; Debug.Log($"Playing Animation: {randomAnimationType}"); currentAnimation = animationLibrary[randomAnimationType](icon); }
    private void ResetIconStyle(VisualElement icon) { icon.style.scale = new StyleScale(new Scale(Vector3.one)); icon.style.rotate = new StyleRotate(new Rotate(new Angle(0))); icon.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(0))); }
    #endregion

    // --- ANIMATION LIBRARY USING UNITY'S BUILT-IN EASING ---

    #region Animation Coroutines

    private IEnumerator Animate_WindUpAndSpin(VisualElement icon)
    {
        float duration = 0.5f, time = 0, windUpAngle = -30f, spinAngle = 360f + 45f;
        float windUpDuration = duration * 0.3f;
        while (time < windUpDuration)
        {
            float progress = Easing.OutCubic(time / windUpDuration); // Using Unity's Easing
            icon.style.rotate = new StyleRotate(new Rotate(new Angle(Mathf.Lerp(0, windUpAngle, progress), AngleUnit.Degree)));
            time += Time.deltaTime; yield return null;
        }
        time = 0; float spinDuration = duration * 0.7f;
        while (time < spinDuration)
        {
            float progress = Easing.OutCubic(time / spinDuration); // Using Unity's Easing
            icon.style.rotate = new StyleRotate(new Rotate(new Angle(Mathf.Lerp(windUpAngle, spinAngle, progress), AngleUnit.Degree)));
            time += Time.deltaTime; yield return null;
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
            time += Time.deltaTime; yield return null;
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
            if (progress < 0.5f) {
                 scaleX = Mathf.Lerp(1, stretchFactor, Easing.OutQuad(progress * 2)); // Using Unity's Easing
                 scaleY = Mathf.Lerp(1, squashFactor, Easing.OutQuad(progress * 2)); // Using Unity's Easing
            } else {
                 scaleX = Mathf.Lerp(stretchFactor, 1, Easing.InQuad((progress - 0.5f) * 2)); // Using Unity's Easing
                 scaleY = Mathf.Lerp(squashFactor, 1, Easing.InQuad((progress - 0.5f) * 2)); // Using Unity's Easing
            }
            icon.style.scale = new StyleScale(new Scale(new Vector2(scaleX, scaleY)));
            time += Time.deltaTime; yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_SlingShot(VisualElement icon)
    {
        float duration = 0.5f, time = 0, pullBackDist = -20f;
        float pullDuration = duration * 0.4f;
        while (time < pullDuration)
        {
            float progress = Easing.OutCubic(time / pullDuration); // Using Unity's Easing
            icon.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(Mathf.Lerp(0, pullBackDist, progress), LengthUnit.Pixel)));
            time += Time.deltaTime; yield return null;
        }
        time = 0; float shootDuration = duration * 0.6f;
        while (time < shootDuration)
        {
            float progress = Easing.OutBounce(time / shootDuration); // Using Unity's Easing
            icon.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(Mathf.Lerp(pullBackDist, 0, progress), LengthUnit.Pixel)));
            time += Time.deltaTime; yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_DropInAndBounce(VisualElement icon)
    {
        float duration = 0.7f, time = 0, startY = -80f;
        while (time < duration)
        {
            float progress = Easing.OutBounce(time / duration); // Using Unity's Easing
            float currentY = Mathf.Lerp(startY, 0, progress);
            icon.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(currentY, LengthUnit.Pixel)));
            time += Time.deltaTime; yield return null;
        }
        ResetIconStyle(icon);
    }

    private IEnumerator Animate_Tada(VisualElement icon) { float duration = 0.5f, time = 0, maxScale = 1.2f, maxAngle = 8f; while (time < duration) { float progress = time / duration; float scaleValue = 1 + Mathf.Sin(progress * Mathf.PI) * (maxScale - 1); float angle = Mathf.Sin(progress * Mathf.PI * 3) * maxAngle * (1 - progress); icon.style.scale = new StyleScale(new Scale(new Vector2(scaleValue, scaleValue))); icon.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree))); time += Time.deltaTime; yield return null; } ResetIconStyle(icon); }
    private IEnumerator Animate_Heartbeat(VisualElement icon) { float beat1Duration = 0.15f, beat2Duration = 0.15f, delay = 0.05f, beatScale = 1.25f; float time = 0; while (time < beat1Duration) { float progress = time / beat1Duration; float scaleValue = 1 + Mathf.Sin(progress * Mathf.PI) * (beatScale - 1); icon.style.scale = new StyleScale(new Scale(new Vector2(scaleValue, scaleValue))); time += Time.deltaTime; yield return null; } icon.style.scale = new StyleScale(new Scale(Vector3.one)); yield return new WaitForSeconds(delay); time = 0; while (time < beat2Duration) { float progress = time / beat2Duration; float scaleValue = 1 + Mathf.Sin(progress * Mathf.PI) * (beatScale - 1); icon.style.scale = new StyleScale(new Scale(new Vector2(scaleValue, scaleValue))); time += Time.deltaTime; yield return null; } ResetIconStyle(icon); }
    private IEnumerator Animate_Shake(VisualElement icon) { float duration = 0.3f, time = 0, shakeAmount = 8f; while (time < duration) { float progress = time / duration; float currentX = Mathf.Sin(progress * Mathf.PI * 8) * (shakeAmount * (1 - progress)); icon.style.translate = new StyleTranslate(new Translate(new Length(currentX, LengthUnit.Pixel), new Length(0))); time += Time.deltaTime; yield return null; } ResetIconStyle(icon); }
    
    private IEnumerator Animate_ElasticPop(VisualElement icon)
    {
        float duration = 0.6f, time = 0;
        while (time < duration)
        {
            float progress = Easing.OutElastic(time / duration); // Using Unity's Easing
            icon.style.scale = new StyleScale(new Scale(new Vector2(progress, progress)));
            time += Time.deltaTime; yield return null;
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
            float progress = Easing.OutCubic(time / halfDuration); // Using Unity's Easing
            icon.style.scale = new StyleScale(new Scale(new Vector2(progress, 1)));
            time += Time.deltaTime; yield return null;
        }
        icon.style.scale = new StyleScale(new Scale(new Vector2(1, 0)));
        time = 0;
        while (time < halfDuration)
        {
            float progress = Easing.OutCubic(time / halfDuration); // Using Unity's Easing
            icon.style.scale = new StyleScale(new Scale(new Vector2(1, progress)));
            time += Time.deltaTime; yield return null;
        }
        ResetIconStyle(icon);
    }

    #endregion
}