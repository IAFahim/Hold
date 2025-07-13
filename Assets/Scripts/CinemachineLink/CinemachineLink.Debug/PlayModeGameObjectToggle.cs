#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CinemachineLink.CinemachineLink.Debug
{
    [DefaultExecutionOrder(-1000000000)]
    [ExecuteAlways]
    public class PlayModeGameObjectToggle : MonoBehaviour
    {
        [Header("GameObjects to disable in Play Mode")]
        public GameObject[] gameObjectsToToggle;
    
        private void OnEnable()
        {
            // Subscribe to play mode state changes
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
    
        private void OnDisable()
        {
            // Unsubscribe to prevent memory leaks
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
    
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    // Disable GameObjects when entering play mode
                    ToggleGameObjects(false);
                    break;
                
                case PlayModeStateChange.ExitingPlayMode:
                    // Enable GameObjects when exiting play mode
                    ToggleGameObjects(true);
                    break;
            }
        }
    
        private void ToggleGameObjects(bool enable)
        {
            if (gameObjectsToToggle == null) return;
        
            foreach (GameObject go in gameObjectsToToggle)
            {
                if (go != null)
                {
                    go.SetActive(enable);
                }
            }
        }
    
        // Optional: Method to manually toggle for testing
        [ContextMenu("Toggle GameObjects")]
        public void ManualToggle()
        {
            bool newState = !gameObjectsToToggle[0].activeInHierarchy;
            ToggleGameObjects(newState);
        }
    }

// Custom editor for better inspector experience
    [CustomEditor(typeof(PlayModeGameObjectToggle))]
    public class PlayModeGameObjectToggleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        
            EditorGUILayout.Space();
        
            PlayModeGameObjectToggle script = (PlayModeGameObjectToggle)target;
        
            if (GUILayout.Button("Preview Toggle"))
            {
                script.ManualToggle();
            }
        
            EditorGUILayout.HelpBox("This script will disable the selected GameObjects when entering Play Mode and re-enable them when exiting Play Mode.", MessageType.Info);
        }
    }
#endif
}