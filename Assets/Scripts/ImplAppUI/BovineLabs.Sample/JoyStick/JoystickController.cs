using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class UIToolkitJoystick : MonoBehaviour
{
    [Header("Output")]
    public Vector2 Value { get; set; } // [-1..1], x right+, y up+
    public bool IsPressed { get; set; }
    public bool IsLocked { get; set; }

    [Header("Tuning")]
    [Range(0.0f, 0.6f)] public float deadZone = 0.12f;
    [Tooltip("Fraction of max radius that is the 'no-lock' bound. Release outside = lock.")]
    [Range(0.1f, 0.95f)] public float noLockRadiusRatio = 0.45f;
    [Range(0.05f, 0.35f)] public float springTime = 0.14f;
    public bool useUnscaledTime = true;

    [Header("Events")]
    public UnityEvent<Vector2> onValueChanged;
    public UnityEvent<bool> onLockChanged;
    public UnityEvent<bool> onPressChanged;

    // UI
    public UIDocument doc;
    public VisualElement root, innerPad, knob, noLockRing, deadRing;

    // Geometry (innerPad local space)
    public Vector2 center;
    public float moveRadius;   // clamp radius (px)
    public float knobRadius;
    public float noLockRadius; // px

    // Pointer state
    public int activePointer = -1;
    public bool dragging;

    // Spring
    public bool springing;
    public Vector2 springStart, springTarget;
    public float springT;

    // Lock
    public Vector2 lockLocalDir = Vector2.right; // local-space dir (y down)
    public Vector2 lockAnchor => center + lockLocalDir.normalized * moveRadius;

    // Colors
    public readonly Color baseKnob = new Color32(60, 60, 60, 255);
    public readonly Color draggingKnob = new Color32(52, 52, 52, 255);
    public readonly Color lockedKnob = new Color32(36, 36, 36, 255);

    public void OnEnable()
    {
        doc = GetComponent<UIDocument>();
        root = doc.rootVisualElement.Q<VisualElement>("joystick");
        innerPad = root.Q<VisualElement>("inner-pad");
        knob = root.Q<VisualElement>("knob");
        noLockRing = root.Q<VisualElement>("no-lock-ring");
        deadRing = root.Q<VisualElement>("dead-zone");

        root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

        innerPad.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
        innerPad.RegisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
        innerPad.RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
        innerPad.RegisterCallback<PointerCancelEvent>(OnPointerCancel, TrickleDown.TrickleDown);

        // Ensure no leftover translate on knob
        knob.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(0), 0));
    }

    public void OnDisable()
    {
        root?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        innerPad?.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        innerPad?.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        innerPad?.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        innerPad?.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
    }

    public void OnGeometryChanged(GeometryChangedEvent e)
    {
        var r = innerPad.contentRect; // local (0,0)
        center = new Vector2(r.width * 0.5f, r.height * 0.5f);

        knobRadius = Mathf.Max(1f, knob.resolvedStyle.width * 0.5f);
        float shortest = Mathf.Min(r.width, r.height);
        moveRadius = Mathf.Max(2f, shortest * 0.5f - knobRadius - 2f);
        noLockRadius = Mathf.Clamp01(noLockRadiusRatio) * moveRadius;

        SetKnobLocal(center);
        // UpdateRings();
        UpdateKnobVisual();
    }

    public void Update()
    {
        if (springing && !IsLocked)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            springT += dt / Mathf.Max(0.0001f, springTime);
            float t = Mathf.Clamp01(springT);
            t = 1f - Mathf.Pow(1f - t, 3f); // ease-out

            SetKnobLocal(Vector2.Lerp(springStart, springTarget, t));
            if (t >= 1f) springing = false;
        }

        // While locked and not dragging, keep the knob pinned
        if (IsLocked && !dragging)
            SetKnobLocal(lockAnchor);
    }

    // ---------- Pointer handling ----------
    public void OnPointerDown(PointerDownEvent evt)
    {
        if (activePointer != -1) return;

        activePointer = evt.pointerId;
        innerPad.CapturePointer(activePointer);
        IsPressed = true;
        dragging = true;
        springing = false;
        onPressChanged?.Invoke(true);

        Vector2 local = innerPad.WorldToLocal(evt.position);

        // If locked and press happens inside the no-lock bound, unlock immediately
        if (IsLocked && InsideNoLock(local))
            SetLocked(false);

        MoveKnob(local);
        evt.StopPropagation();
    }

    public void OnPointerMove(PointerMoveEvent evt)
    {
        if (!dragging || evt.pointerId != activePointer) return;
        MoveKnob(innerPad.WorldToLocal(evt.position));
        evt.StopPropagation();
    }

    public void OnPointerUp(PointerUpEvent evt)
    {
        if (evt.pointerId != activePointer) return;
        EndPointer(innerPad.WorldToLocal(evt.position));
        evt.StopPropagation();
    }

    public void OnPointerCancel(PointerCancelEvent evt)
    {
        if (evt.pointerId != activePointer) return;
        // Treat cancel like "let go" at last known position
        EndPointer(knobPositionLocal());
        evt.StopPropagation();
    }

    public void EndPointer(Vector2 localRelease)
    {
        dragging = false;
        IsPressed = false;
        onPressChanged?.Invoke(false);

        innerPad.ReleasePointer(activePointer);
        activePointer = -1;

        if (InsideNoLock(localRelease))
        {
            // Unlock and spring to center
            SetLocked(false);
            springStart = localRelease;
            springTarget = center;
            springT = 0f;
            springing = true;
        }
        else
        {
            // Lock in the released direction at full radius
            Vector2 dir = (localRelease - center);
            if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
            lockLocalDir = dir.normalized;
            SetLocked(true);
            SetKnobLocal(lockAnchor);
        }
    }

    // ---------- Core movement ----------
    public void MoveKnob(Vector2 local)
    {
        Vector2 offset = local - center;
        float max = moveRadius;
        if (offset.sqrMagnitude > max * max) offset = offset.normalized * max;

        SetKnobLocal(center + offset);
        // UpdateRings();
    }

    public void SetKnobLocal(Vector2 local)
    {
        // Position
        knob.style.left = local.x - knobRadius;
        knob.style.top  = local.y - knobRadius;

        // Output value (y up)
        Vector2 rel = local - center;
        Vector2 normLocal = rel / Mathf.Max(0.0001f, moveRadius);   // y down
        Vector2 value = new Vector2(
            Mathf.Clamp(normLocal.x, -1f, 1f),
            -Mathf.Clamp(normLocal.y, -1f, 1f)                       // invert to y up
        );

        // Dead zone only when not locked
        if (!IsLocked && value.magnitude < deadZone)
            value = Vector2.zero;

        if (value != Value)
        {
            Value = value;
            onValueChanged?.Invoke(Value);
        }

        UpdateKnobVisual();
    }

    // ---------- Helpers ----------
    public bool InsideNoLock(Vector2 local)
    {
        return (local - center).sqrMagnitude <= noLockRadius * noLockRadius;
    }

    public Vector2 knobPositionLocal()
    {
        // Reconstruct from style.left/top
        float x = knob.resolvedStyle.left + knobRadius;
        float y = knob.resolvedStyle.top + knobRadius;
        return new Vector2(x, y);
    }

    public void SetLocked(bool locked)
    {
        if (IsLocked == locked) return;
        IsLocked = locked;
        springing = false;
        onLockChanged?.Invoke(IsLocked);
        UpdateKnobVisual();
    }

    // public void UpdateRings()
    // {
    //     // No-lock ring reflects noLockRadius
    //     if (noLockRing != null && moveRadius > 1f)
    //     {
    //         float px = Mathf.Max(10f, noLockRadius * 2f);
    //         noLockRing.style.width = px;
    //         noLockRing.style.height = px;
    //     }
    //
    //     // Dead-zone ring reflects deadZone
    //     if (deadRing != null && moveRadius > 1f)
    //     {
    //         float px = Mathf.Clamp(deadZone * moveRadius * 2f, 6f, moveRadius * 2f);
    //         deadRing.style.width = px;
    //         deadRing.style.height = px;
    //     }
    // }

    public void UpdateKnobVisual()
    {
        Color c = IsLocked ? lockedKnob : (dragging ? draggingKnob : baseKnob);
        knob.style.backgroundColor = new StyleColor(c);

        // Subtle glow when locked/dragging by tinting ring color
        var ringCol = IsLocked
            ? new Color(1f, 0.56f, 0f, 0.28f)
            : new Color(1f, 0.56f, 0f, 0.18f);
        noLockRing.style.borderTopColor = ringCol;
        noLockRing.style.borderRightColor = ringCol;
        noLockRing.style.borderBottomColor = ringCol;
        noLockRing.style.borderLeftColor = ringCol;
        noLockRing.style.backgroundColor =
            new StyleColor(new Color(ringCol.r, ringCol.g, ringCol.b, ringCol.a * 0.35f));
    }
}