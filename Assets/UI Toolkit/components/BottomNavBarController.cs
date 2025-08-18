using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BottomNavBarController : MonoBehaviour
{
    public UIDocument uiDocument;

    private VisualElement root;
    private VisualElement selectionIndicator;
    private List<Button> navButtons;

    private int currentIndex = -1;

    private const string NAV_BUTTON_CLASS = "nav-button";
    private const string SELECTED_BUTTON_CLASS = "nav-button--selected";

    void OnEnable()
    {
        if (uiDocument == null) return;

        root = uiDocument.rootVisualElement;
        selectionIndicator = root.Q<VisualElement>("SelectionIndicator");
        navButtons = root.Query<Button>(className: NAV_BUTTON_CLASS).ToList();

        for (int i = 0; i < navButtons.Count; i++)
        {
            int index = i;
            navButtons[i].RegisterCallback<ClickEvent>(evt => SelectButton(index));
        }

        root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        if (currentIndex == -1 && navButtons.Count > 0)
        {
            // Select last button initially, without animation
            SelectButton(navButtons.Count - 1, false); 
        }
        root.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    // Corrected version of the corrected logic
    private void SelectButton(int index, bool animated = true)
    {
        if (index < 0 || index >= navButtons.Count || index == currentIndex) return;

        for (int i = 0; i < navButtons.Count; i++)
        {
            navButtons[i].EnableInClassList(SELECTED_BUTTON_CLASS, i == index);
        }

        Button selectedButton = navButtons[index];
    
        // This calculation is now correct because the indicator and button
        // share the same parent and coordinate space.
        float targetX = selectedButton.layout.xMin + (selectedButton.layout.width / 2f) - (selectionIndicator.layout.width / 2f);

        var duration = animated ? 0.4f : 0f;
        selectionIndicator.style.transitionDuration = 
            new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(duration, TimeUnit.Second) });
    
        selectionIndicator.style.left = targetX;

        currentIndex = index;
    }
}