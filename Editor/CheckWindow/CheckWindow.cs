using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using TalusKit.Runtime.CheckWindow;

using Object = UnityEngine.Object;

namespace TalusKit.Editor.CheckWindow
{
    public class CheckWindow : EditorWindow
    {
        private const string _PreferencePrefix = "TalusKit.Check.";
        private const string _ShowIgnoredPreference = "TalusKit.ShowIgnored";

        private static Dictionary<Check, int[]> _IntValues;
        private Dictionary<Check, bool> _CheckStates;
        private int _ErrorCount;

        private string _FilterString = "";
        private bool _InvertSort;

        private int _NoticeCount;

        private readonly string _Preference = "TalusKit.CheckResolve.";

        private List<CheckResult> _Results = new List<CheckResult>();

        private Vector2 _Scroll;

        private SortMode _SortMode = SortMode.None;
        private int _WarningCount;

        Dictionary<Scene, IgnoredCheckResults> _IgnoredLists;

        private bool ShowIgnored
        {
            get => EditorPrefs.GetBool(_ShowIgnoredPreference, true);
            set => EditorPrefs.SetBool(_ShowIgnoredPreference, value);
        }

        private bool ShowNotice
        {
            get => EditorPrefs.GetBool(_Preference + "showNotice", true);
            set
            {
                if (value != ShowNotice)
                {
                    EditorPrefs.SetBool(_Preference + "showNotice", value);
                }
            }
        }

        private bool ShowWarning
        {
            get => EditorPrefs.GetBool(_Preference + "showWarning", true);
            set
            {
                if (value != ShowWarning)
                {
                    EditorPrefs.SetBool(_Preference + "showWarning", value);
                }
            }
        }

        private bool ShowError
        {
            get => EditorPrefs.GetBool(_Preference + "showError", true);
            set
            {
                if (value != ShowError)
                {
                    EditorPrefs.SetBool(_Preference + "showError", value);
                }
            }
        }

        private bool ShowIgnoreObject
        {
            get => EditorPrefs.GetBool(_Preference + "showIgnoreObject", true);
            set
            {
                if (value != ShowIgnoreObject)
                {
                    EditorPrefs.SetBool(_Preference + "showIgnoreObject", value);
                }
            }
        }

        private void OnEnable()
        {
            titleContent = new GUIContent(EditorGUIUtility.IconContent("Valid"))
            {
                text = "Check/Resolve"
            };

            minSize = new Vector2(640, 180);
            InitializeCheckStates();
        }

        private void OnGUI()
        {
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar, GUILayout.Height(22)))
            {
                if (GUILayout.Button("Check", EditorStyles.toolbarButton))
                {
                    PerformChecks();
                }

                if (GUILayout.Button("", EditorStyles.toolbarPopup))
                {
                    var r = new Rect(0, 0, 16, 20);
                    var menu = new GenericMenu();

                    foreach (Check check in _CheckStates.Keys)
                    {
                        menu.AddItem(new GUIContent(check.Name), _CheckStates[check],
                            () => SetCheckState(check, !_CheckStates[check]));
                    }

                    menu.DropDown(r);
                }

                if (GUILayout.Button("Resolve", EditorStyles.toolbarButton))
                {
                    Resolve();
                }

                GUILayout.FlexibleSpace();

                _FilterString =
                        EditorGUILayout.DelayedTextField(_FilterString, EditorStyles.toolbarSearchField, GUILayout.Width(180));

                EditorGUI.BeginChangeCheck();
                ShowIgnoreObject = GUILayout.Toggle(ShowIgnoreObject, "Show Ignore Object", EditorStyles.toolbarButton);
                SetIgnoreObjectVisibility(ShowIgnoreObject);
                EditorGUI.EndChangeCheck();

                ShowIgnored = GUILayout.Toggle(ShowIgnored, "Ignored", EditorStyles.toolbarButton);

                ShowNotice = GUILayout.Toggle(ShowNotice,
                    new GUIContent(_NoticeCount.ToString(), CheckResult.GetIcon(CheckResult.Result.Notice)),
                    EditorStyles.toolbarButton);

                ShowWarning = GUILayout.Toggle(ShowWarning,
                    new GUIContent(_WarningCount.ToString(), CheckResult.GetIcon(CheckResult.Result.Warning)),
                    EditorStyles.toolbarButton);

                ShowError = GUILayout.Toggle(ShowError,
                    new GUIContent(_ErrorCount.ToString(), CheckResult.GetIcon(CheckResult.Result.Failed)),
                    EditorStyles.toolbarButton);
            }

            using (new GUILayout.VerticalScope())
            {
                GUI.backgroundColor = Color.white * 1.3f;

                using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    SortButton("Check Type", SortMode.CheckType, GUILayout.Width(128));
                    SortButton("Object", SortMode.ObjectName,  GUILayout.Width(128));
                    SortButton("Message", SortMode.Message, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(256));
                    SortButton("Resolution", SortMode.Resolution, GUILayout.Width(128));
                    SortButton("Ignored", SortMode.Ignored, GUILayout.ExpandWidth(true));
                }

                _Scroll = GUILayout.BeginScrollView(_Scroll, false, true);

                if (_Results != null && _Results.Count > 0)
                {
                    bool odd = true;

                    foreach (CheckResult result in _Results)
                    {
                        if (!ShowIgnored && IsIgnored(result) || result.MainObject == null)
                        {
                            continue;
                        }

                        if (result.result == CheckResult.Result.Notice && !ShowNotice)
                        {
                            continue;
                        }

                        if (result.result == CheckResult.Result.Warning && !ShowWarning)
                        {
                            continue;
                        }

                        if (result.result == CheckResult.Result.Failed && !ShowError)
                        {
                            continue;
                        }

                        if (!string.IsNullOrEmpty(_FilterString) && !result.Message.text.Contains(_FilterString))
                        {
                            continue;
                        }

                        GUI.backgroundColor = Color.white * (odd ? 0.9f : 0.8f);
                        odd = !odd;

                        using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
                        {
                            EditorGUI.BeginDisabledGroup(ShowIgnored && IsIgnored(result));

                            GUILayout.Label(result.Check.Name, Styles.Line, GUILayout.Width(128));

                            if (GUILayout.Button(result.MainObject != null ? result.MainObject.name : "Null", Styles.Line,
                                    GUILayout.Width(128)))
                            {
                                Selection.activeObject = result.MainObject;
                            }

                            if (GUILayout.Button(result.Message, Styles.Line, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(256)))
                            {
                                EditorUtility.DisplayDialog(result.result.ToString(), result.Message.text, "Close");
                            }

                            ShowMenu(result);
                            EditorGUI.EndDisabledGroup();

                            GUILayout.Space(18);

                            EditorGUI.BeginChangeCheck();
                            bool ignored = GUILayout.Toggle(IsIgnored(result), "", EditorStyles.toggle, GUILayout.ExpandWidth(true));
                            if (EditorGUI.EndChangeCheck())
                            {
                                SetIgnored(result, ignored);
                            }

                        }
                    }
                }
                else
                {
                    GUILayout.Label("No Results");
                }

                GUI.backgroundColor = Color.white;

                GUILayout.FlexibleSpace();
                GUILayout.EndScrollView();
            }
        }

        [MenuItem("TalusKit/Debugging/Check and Resolve", priority = 99999)]
        public static void OpenWindow()
        {
            GetWindow<CheckWindow>(false);
        }

        private void InitializeCheckStates()
        {
            _CheckStates = new Dictionary<Check, bool>();

            foreach (Check check in Check.AllChecks)
            {
                _CheckStates.Add(check, EditorPrefs.GetBool(_PreferencePrefix + check.Name, check.DefaultEnabled));
            }
        }

        private void SetCheckState(Check check, bool value)
        {
            _CheckStates[check] = value;
            EditorPrefs.SetBool(_PreferencePrefix + check.Name, value);
        }

        private void SortButton(string label, SortMode sortMode, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(label, _SortMode == sortMode ? Styles.SortHeader : Styles.Header, options))
            {
                if (_SortMode == sortMode)
                {
                    _InvertSort = !_InvertSort;
                }
                else
                {
                    _SortMode = sortMode;
                    _InvertSort = false;
                }

                if (_Results != null)
                {
                    switch (sortMode)
                    {
                        default:
                        case SortMode.None:
                            break;

                        case SortMode.CheckType:
                            _Results = _Results.OrderBy((a) => a.Check.Name).ToList();
                            break;

                        case SortMode.ObjectName:
                            _Results = _Results.OrderBy((a) => a.MainObject == null ? "" : a.MainObject.name).ToList();
                            break;

                        case SortMode.Message:
                            _Results = _Results.OrderBy((a) => a.Message.text).OrderBy((a) => a.result).ToList();
                            break;

                        case SortMode.Resolution:
                            _Results = _Results.OrderBy((a) => a.Check.ResolutionActions[a.ResolutionActionIndex]).ToList();
                            break;

                        case SortMode.Ignored:
                            _Results = _Results.OrderBy(IsIgnored).ToList();
                            break;
                    }

                    if (_InvertSort)
                    {
                        _Results.Reverse();
                    }

                    Repaint();
                }
            }
        }

        private void BuildIgnoredList()
        {
            if (_IgnoredLists == null)
            {
                _IgnoredLists = new Dictionary<Scene, IgnoredCheckResults>();
            }
            else
            {
                _IgnoredLists.Clear();
            }

            var all = FindObjectsOfType<IgnoredCheckResults>().ToList();

            foreach (IgnoredCheckResults one in all)
            {
                if (!_IgnoredLists.ContainsKey(one.gameObject.scene))
                {
                    _IgnoredLists.Add(one.gameObject.scene, one);
                }
                else
                {
                    Debug.LogWarning($"Found at least two IgnoredCheckResults objects in scene {one.gameObject.scene.name}");
                }
            }
        }

        private bool IsIgnored(CheckResult result)
        {
            if (result.MainObject == null || !(result.MainObject is GameObject))
            {
                return false;
            }

            var go = result.MainObject as GameObject;

            if (!_IgnoredLists.ContainsKey(go.scene))
            {
                return false;
            }

            IgnoredCheckResults igl = _IgnoredLists[go.scene];
            return igl.IgnoredResults.Any(o => (o.Check == result.Check.GetType().ToString()) && (o.GameObject == go));
        }

        private void SetIgnored(CheckResult result, bool ignored)
        {
            result.SetIgnored(ignored);

            if (result.MainObject is GameObject)
            {
                var go = result.MainObject as GameObject;
                EditorSceneManager.MarkSceneDirty(go.scene);
            }

            BuildIgnoredList();
        }

        private void SetIgnoreObjectVisibility(bool show)
        {
            var ignoreObj = FindObjectOfType<IgnoredCheckResults>();
            if (ignoreObj != null)
            {
                ignoreObj.gameObject.hideFlags = (!show ? HideFlags.HideInHierarchy : HideFlags.None);
                EditorApplication.DirtyHierarchyWindowSorting();

                var serializedIgnoredCheck = new SerializedObject(ignoreObj.gameObject);
                serializedIgnoredCheck.Update();

                SerializedProperty property = serializedIgnoredCheck.FindProperty("m_ObjectHideFlags");
                property.intValue = (int)(!show ? HideFlags.HideInHierarchy : HideFlags.None);

                if (serializedIgnoredCheck.hasModifiedProperties)
                {
                    serializedIgnoredCheck.ApplyModifiedProperties();
                }
            }
        }

        void ShowMenu(CheckResult result)
        {
            if (_IntValues == null)
            {
                _IntValues = new Dictionary<Check, int[]>();
            }

            if (!_IntValues.ContainsKey(result.Check))
            {
                int count = result.Check.ResolutionActions.Length;
                int[] values = new int[count];

                for (int i = 0; i < count; i++)
                {
                    values[i] = i;
                }

                _IntValues.Add(result.Check, values);
            }

            result.ResolutionActionIndex = EditorGUILayout.IntPopup(result.ResolutionActionIndex,
                result.Check.ResolutionActions, _IntValues[result.Check], EditorStyles.toolbarDropDown, GUILayout.Width(128));
        }

        private void Resolve()
        {
            foreach (var result in _Results)
            {
                if (!IsIgnored(result))
                {
                    result.Check.Resolve(result);
                }
            }

            PerformChecks();
        }

        private void PerformChecks()
        {
            var results = new List<CheckResult>();
            bool canceled = false;

            try
            {
                var so = new SceneObjects();

                int count = _CheckStates.Count;
                int i = 0;

                foreach (KeyValuePair<Check, bool> kvp in _CheckStates)
                {
                    float t = (float) i / count;

                    if (EditorUtility.DisplayCancelableProgressBar("Performing Checks", kvp.Key.Name, t))
                    {
                        canceled = true;
                        break;
                    }

                    if (kvp.Value)
                    {
                        results.AddRange(kvp.Key.GetResults(so));
                    }

                    i++;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (!canceled)
            {
                _Results = results;
            }

            _SortMode = SortMode.None;

            _NoticeCount = _Results.Count(o => o.result == CheckResult.Result.Notice);
            _WarningCount = _Results.Count(o => o.result == CheckResult.Result.Warning);
            _ErrorCount = _Results.Count(o => o.result == CheckResult.Result.Failed);

            BuildIgnoredList();
            Repaint();
        }

        private enum SortMode
        {
            None,
            CheckType,
            ObjectName,
            Message,
            Resolution,
            Ignored
        }


        private static class Styles
        {
            public static readonly GUIStyle Header;
            public static readonly GUIStyle SortHeader;
            public static readonly GUIStyle Line;

            static Styles()
            {
                Header = new GUIStyle(EditorStyles.toolbarButton)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Bold
                };

                SortHeader = new GUIStyle(EditorStyles.toolbarDropDown)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Bold
                };

                Line = new GUIStyle(EditorStyles.toolbarButton)
                {
                    alignment = TextAnchor.MiddleLeft
                };
            }
        }
    }

    public class SceneObjects
    {
        public readonly GameObject[] AllObjects;
        public readonly List<Component> ReferencedComponents;
        public readonly List<GameObject> ReferencedGameObjects;
        public readonly List<Object> ReferencedObjects;

        public SceneObjects()
        {
            AllObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            AllObjects = AllObjects.Where(o => !PrefabUtility.IsPartOfPrefabAsset(o) && o.hideFlags == HideFlags.None)
                                   .ToArray();

            ReferencedGameObjects = new List<GameObject>();
            ReferencedComponents = new List<Component>();
            ReferencedObjects = new List<Object>();

            if (AllObjects == null || AllObjects.Length == 0)
            {
                return;
            }

            try
            {
                int count = AllObjects.Length;
                int i = 0;

                foreach (GameObject sceneObject in AllObjects)
                {
                    float progress = ++i / (float) count;

                    if (EditorUtility.DisplayCancelableProgressBar("Building Reference Table...", $"{sceneObject.name}",
                            progress))
                    {
                        ReferencedComponents.Clear();
                        ReferencedGameObjects.Clear();
                        ReferencedObjects.Clear();
                        break;
                    }

                    Component[] components = sceneObject.GetComponents<Component>();

                    foreach (Component component in components)
                    {
                        if (component is Transform)
                        {
                            continue;
                        }
                        else if (component is Renderer)
                        {
                            var renderer = component as Renderer;

                            if (renderer.probeAnchor != null)
                            {
                                ReferencedComponents.Add(renderer.probeAnchor);
                                ReferencedGameObjects.Add(renderer.probeAnchor.gameObject);
                            }

                            foreach (var sharedMat in renderer.sharedMaterials)
                            {
                                ReferencedObjects.Add(sharedMat);
                            }
                        }
                        else
                        {
                            Type t = component.GetType();

                            FieldInfo[] fields = t.GetFields(BindingFlags.Public
                                                             | BindingFlags.Instance
                                                             | BindingFlags.NonPublic);

                            foreach (FieldInfo f in fields)
                            {
                                if (f.FieldType == typeof(GameObject))
                                {
                                    var go = f.GetValue(component) as GameObject;

                                    if (go != null && go != component.gameObject)
                                    {
                                        ReferencedGameObjects.Add(go);
                                    }
                                }
                                else if (f.FieldType == typeof(GameObject[]))
                                {
                                    var golist = f.GetValue(component) as GameObject[];

                                    foreach (var go in golist)
                                    {
                                        if (go != null && go != component.gameObject)
                                        {
                                            ReferencedGameObjects.Add(go);
                                        }
                                    }
                                }
                                else if (f.FieldType == typeof(Transform))
                                {
                                    var tr = f.GetValue(component) as Transform;

                                    if (tr != null && tr.gameObject != component.gameObject)
                                    {
                                        ReferencedGameObjects.Add(tr.gameObject);
                                        ReferencedComponents.Add(tr);
                                    }
                                }
                                else if (f.FieldType == typeof(Transform[]))
                                {
                                    var trlist = f.GetValue(component) as Transform[];

                                    foreach (var tr in trlist)
                                    {
                                        if (tr != null && tr.gameObject != component.gameObject)
                                        {
                                            ReferencedGameObjects.Add(tr.gameObject);
                                            ReferencedComponents.Add(tr);
                                        }
                                    }
                                }
                                else if (f.FieldType.IsSubclassOf(typeof(Component)))
                                {
                                    var comp = f.GetValue(component) as Component;

                                    if (comp != null && comp.gameObject != component.gameObject)
                                    {
                                        ReferencedGameObjects.Add(comp.gameObject);
                                        ReferencedComponents.Add(comp);
                                    }
                                }
                                else if (f.FieldType.IsSubclassOf(typeof(Component[])))
                                {
                                    var complist = f.GetValue(component) as Component[];

                                    foreach (var comp in complist)
                                    {
                                        if (comp != null && comp.gameObject != component.gameObject)
                                        {
                                            ReferencedGameObjects.Add(comp.gameObject);
                                            ReferencedComponents.Add(comp);
                                        }
                                    }
                                }
                                else if (f.FieldType == typeof(UnityEvent))
                                {
                                    var ue = f.GetValue(component) as UnityEvent;
                                    int evtCount = ue.GetPersistentEventCount();

                                    for (int k = 0; k < evtCount; k++)
                                    {
                                        Object target = ue.GetPersistentTarget(k);

                                        if (target is GameObject)
                                        {
                                            ReferencedGameObjects.Add(target as GameObject);
                                        }
                                        else if (target.GetType().IsSubclassOf(typeof(Component)))
                                        {
                                            ReferencedGameObjects.Add((target as Component).gameObject);
                                        }
                                    }
                                }
                                else if (f.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                                {
                                    var obj = f.GetValue(component) as UnityEngine.Object;
                                    ReferencedObjects.Add(obj);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}