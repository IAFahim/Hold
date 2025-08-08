using System;
using System.Collections.Generic;
using System.Linq;
using Missions.Missions.Authoring.Schemas;
using Missions.Missions.Authoring.Settings;
using UnityEngine;
using UnityEngine.UIElements;

namespace Missions.Missions.Debug.Runtime
{
    [DefaultExecutionOrder(-1000)]
    public class SchemaDebugOverlay : MonoBehaviour
    {
        [Header("Settings References (assign assets in build)")]
        public MissionSettings missionSettings;
        public GoalSettings goalSettings;
        public LocationSettings locationSettings;
        public NameSettings nameSettings;
        public TimeSettings timeSettings;
        public ItemSettings itemSettings;

        [Header("UI")]
        public UIDocument uiDocument;

        private enum Dataset { Missions, Goals, Locations, Times, Items, Names }

        private DropdownField _datasetField;
        private TextField _searchField;
        private Button _validateBtn;
        private Button _exportBtn;
        private Button _copyBtn;
        private ListView _listView;
        private Label _detailsLabel;

        private List<BaseSchema> _currentList = new();

        private void OnEnable()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }
            if (uiDocument == null) return;

            var root = uiDocument.rootVisualElement;
            _datasetField = root.Q<DropdownField>("datasetDropdown");
            _searchField = root.Q<TextField>("searchField");
            _validateBtn = root.Q<Button>("validateButton");
            _exportBtn = root.Q<Button>("exportButton");
            _copyBtn = root.Q<Button>("copyButton");
            _listView = root.Q<ListView>("itemsList");
            _detailsLabel = root.Q<Label>("detailsLabel");

            // Populate dataset dropdown
            if (_datasetField != null)
            {
                var values = Enum.GetNames(typeof(Dataset)).ToList();
                _datasetField.choices = values;
                _datasetField.value = values[0];
                _datasetField.RegisterValueChangedCallback(_ => RefreshList());
            }

            if (_searchField != null)
            {
                _searchField.RegisterValueChangedCallback(_ => RefreshList());
            }

            if (_validateBtn != null) _validateBtn.clicked += ValidateLinks;
            if (_exportBtn != null) _exportBtn.clicked += ExportJson;
            if (_copyBtn != null) _copyBtn.clicked += CopySummaryToClipboard;

            if (_listView != null)
            {
                _listView.fixedItemHeight = 28;
                _listView.selectionType = SelectionType.Single;
                _listView.makeItem = () => new Label();
                _listView.bindItem = (el, i) => { (el as Label).text = SafeName(_currentList[i]); };
                _listView.itemsSource = _currentList;
                _listView.onSelectionChange += sel =>
                {
                    var obj = sel.FirstOrDefault() as BaseSchema;
                    ShowDetails(obj);
                };
            }

            RefreshList();
        }

        private void RefreshList()
        {
            var ds = GetDataset();
            string query = _searchField != null ? _searchField.value?.ToLower() : string.Empty;
            IEnumerable<BaseSchema> src = ds switch
            {
                Dataset.Missions => missionSettings?.schemas ?? Array.Empty<MissionSchema>(),
                Dataset.Goals => goalSettings?.schemas ?? Array.Empty<GoalSchema>(),
                Dataset.Locations => locationSettings?.schemas ?? Array.Empty<LocationSchema>(),
                Dataset.Times => timeSettings?.schemas ?? Array.Empty<TimeSchema>(),
                Dataset.Items => itemSettings?.schemas ?? Array.Empty<ItemSchema>(),
                Dataset.Names => nameSettings?.schemas ?? Array.Empty<NameSchema>(),
                _ => Array.Empty<BaseSchema>()
            };
            _currentList = src
                .Where(s => s != null)
                .OrderBy(s => s.name)
                .Where(s => string.IsNullOrEmpty(query) || s.name.ToLower().Contains(query))
                .Cast<BaseSchema>()
                .ToList();

            if (_listView != null)
            {
                _listView.itemsSource = _currentList;
                _listView.Rebuild();
            }
        }

        private Dataset GetDataset()
        {
            if (_datasetField == null || string.IsNullOrEmpty(_datasetField.value)) return Dataset.Missions;
            return (Dataset)Enum.Parse(typeof(Dataset), _datasetField.value);
        }

        private string SafeName(UnityEngine.Object obj) => obj == null ? "<null>" : obj.name;

        private void ShowDetails(BaseSchema schema)
        {
            if (_detailsLabel == null) return;
            if (schema == null)
            {
                _detailsLabel.text = "Select an item";
                return;
            }

            switch (schema)
            {
                case MissionSchema ms:
                    string goals = ms.goals != null ? string.Join(", ", ms.goals.Where(g => g).Select(g => g.name)) : "<none>";
                    _detailsLabel.text = $"Mission\nID: {ms.ID}\nName: {SafeName(ms.nameSchema)}\nLocation: {SafeName(ms.locationSchema)}\nGoals: {goals}";
                    break;
                case GoalSchema gs:
                    _detailsLabel.text = $"Goal\nID: {gs.ID}\nTargetType: {gs.targetType}\nRange: {SafeName(gs.rangeSchema)}";
                    break;
                case LocationSchema ls:
                    _detailsLabel.text = $"Location\nID: {ls.ID}\nPosition: {ls.position}\nRange: {ls.range}";
                    break;
                case TimeSchema ts:
                    _detailsLabel.text = $"Time\nID: {ts.ID}\nCrossLink: {SafeName(ts.crossLink)}\nRange: {SafeName(ts.rangeFloat)}";
                    break;
                case ItemSchema isch:
                    _detailsLabel.text =
                        $"Item\nID: {isch.ID}\nName: {SafeName(isch.name)}\nWeight: {isch.weightKg} kg\nFlags: {isch.flags}";
                    break;
                case NameSchema n:
                    _detailsLabel.text = $"Name\nID: {n.ID}\nText: {n.fixed32}";
                    break;
                default:
                    _detailsLabel.text = $"{schema.GetType().Name}\nID: {schema.ID}\n{schema.name}";
                    break;
            }
        }

        private void ValidateLinks()
        {
            var issues = new List<string>();
            // Missions
            if (missionSettings?.schemas != null)
            {
                foreach (var m in missionSettings.schemas)
                {
                    if (m == null) continue;
                    if (!m.nameSchema) issues.Add($"Mission '{m.name}' missing NameSchema");
                    if (!m.locationSchema) issues.Add($"Mission '{m.name}' missing LocationSchema");
                    if (m.goals == null || m.goals.Length == 0) issues.Add($"Mission '{m.name}' has no goals");
                    else if (m.goals.Any(g => !g)) issues.Add($"Mission '{m.name}' has null entries in goals");
                }
            }
            // Time cross-links
            if (timeSettings?.schemas != null)
            {
                foreach (var t in timeSettings.schemas)
                {
                    if (t == null) continue;
                    if (!t.crossLink) issues.Add($"Time '{t.name}' missing crossLink reference");
                }
            }
            // Items
            if (itemSettings?.schemas != null)
            {
                foreach (var it in itemSettings.schemas)
                {
                    if (it == null) continue;
                    if (!it.name) issues.Add($"Item '{it.name}' missing Name ref");
                    if (it.weightKg < 0) issues.Add($"Item '{it.name}' has negative weight");
                }
            }

            if (issues.Count == 0)
            {
                Debug.Log("[SchemaDebug] No issues found.");
            }
            else
            {
                foreach (var issue in issues) Debug.LogWarning($"[SchemaDebug] {issue}");
                Debug.Log($"[SchemaDebug] Found {issues.Count} issues.");
            }
        }

        private void ExportJson()
        {
            var payload = new
            {
                missions = missionSettings?.schemas?.Where(s => s).Select(s => new
                {
                    s.ID,
                    name = SafeName(s.nameSchema),
                    location = SafeName(s.locationSchema),
                    goals = s.goals?.Where(g => g).Select(g => g.name).ToArray() ?? Array.Empty<string>()
                }).ToArray() ?? Array.Empty<object>(),
                times = timeSettings?.schemas?.Where(s => s).Select(s => new { s.ID, cross = SafeName(s.crossLink) }).ToArray() ?? Array.Empty<object>(),
                items = itemSettings?.schemas?.Where(s => s).Select(s => new { s.ID, name = SafeName(s.name), s.weightKg, flags = s.flags.ToString() }).ToArray() ?? Array.Empty<object>()
            };
            var json = JsonUtility.ToJson(new Wrapper(payload), true);
#if UNITY_EDITOR
            var path = UnityEditor.EditorUtility.SaveFilePanel("Export Schema Debug JSON", Application.dataPath, "SchemaDebug", "json");
            if (!string.IsNullOrEmpty(path)) System.IO.File.WriteAllText(path, json);
#else
            Debug.Log(json);
#endif
        }

        private void CopySummaryToClipboard()
        {
            string sum = $"Missions: {missionSettings?.schemas?.Length ?? 0}, Goals: {goalSettings?.schemas?.Length ?? 0}, Locations: {locationSettings?.schemas?.Length ?? 0}, Times: {timeSettings?.schemas?.Length ?? 0}, Items: {itemSettings?.schemas?.Length ?? 0}";
#if UNITY_EDITOR
            GUIUtility.systemCopyBuffer = sum;
#endif
            Debug.Log($"[SchemaDebug] {sum}");
        }

        [Serializable]
        private class Wrapper { public object data; public Wrapper(object data) { this.data = data; } }
    }
}