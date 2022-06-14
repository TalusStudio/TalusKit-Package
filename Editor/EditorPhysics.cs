using System;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace TalusKit.Editor
{
    internal class EditorPhysics : EditorWindow
    {
        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnDestroy()
        {
            // Make sure we are not simulating after this is destroyed.
            StopSimulation();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical("box");
            {
                Tickrate = (int)EditorGUILayout.Slider("Tickrate", Tickrate, 8, 64);
                Speed = EditorGUILayout.Slider("Speed", Speed, 0.001f, 2f);
                Gravity = EditorGUILayout.Vector3Field("Gravity", Gravity);
            }

            EditorGUILayout.EndVertical();

            if (!_IsSimulating)
            {
                if (GUILayout.Button("Simulate"))
                {
                    StartSimulation();
                }
            }
            else
            {
                if (GUILayout.Button("Stop Simulate"))
                {
                    StopSimulation();
                }
            }
        }

#region Create Instance

        [MenuItem("TalusKit/Utility/Editor Physics", false, 201)]
        private static void CreateInstance()
        {
            CreateWindow<EditorPhysics>().Show();
        }

#endregion

        private void StartSimulation()
        {
            if (_IsSimulating)
            {
                return;
            }

            _IsSimulating = true;

            _SimulatedObjects.Clear();

            foreach (var item in Selection.gameObjects)
            {
                _SimulatedObjects.Add(new PlayingRigidbody(item));
            }

            // Register objects to be undoable
            foreach (var item in _SimulatedObjects)
            {
                Undo.RegisterFullObjectHierarchyUndo(item.ActualGameObject, "Editor Physics Run");
            }

            // Take simulation to manual
            _OldGravity = Physics.gravity;
            _OldAutoSimulatingFlag = Physics.autoSimulation;

            Physics.autoSimulation = false;
            Physics.gravity = Gravity;

            Undo.undoRedoPerformed += OnUndoPerformed;
            SceneManager.activeSceneChanged += OnSceneChanged;

            _FrozeSceneRigidbodies.Clear();

            GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

            foreach (var root in rootObjects)
            {
                Rigidbody[] objs = root.GetComponentsInChildren<Rigidbody>(false);

                foreach (var obj in objs)
                {
                    if (!obj.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    //If object is not the object we want to simulate
                    bool found = false;

                    foreach (var item in _SimulatedObjects)
                    {
                        if (item.ActualGameObject == obj.gameObject)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        _FrozeSceneRigidbodies.Add(new HoldRigidbody(obj));
                    }
                }
            }

            foreach (var item in _FrozeSceneRigidbodies)
            {
                item.Freeze();
            }

            foreach (var item in _SimulatedObjects)
            {
                item.Start();
            }
        }

        private void StopSimulation()
        {
            if (!_IsSimulating)
            {
                return;
            }

            _IsSimulating = false;

            Physics.autoSimulation = _OldAutoSimulatingFlag;
            Physics.gravity = _OldGravity;

            Undo.undoRedoPerformed -= OnUndoPerformed;
            SceneManager.activeSceneChanged -= OnSceneChanged;

            foreach (var item in _FrozeSceneRigidbodies)
            {
                item.Revert();
            }

            foreach (var item in _SimulatedObjects)
            {
                item.Stop();
            }

            _FrozeSceneRigidbodies.Clear();
            _SimulatedObjects.Clear();
        }

        private void ToggleSimulation()
        {
            if (_IsSimulating)
            {
                StopSimulation();
            }
            else
            {
                StartSimulation();
            }
        }

        private void OnEditorUpdate()
        {
            if (_IsSimulating)
            {
                Physics.gravity = Gravity;
                Physics.Simulate((1f / Tickrate) * Speed);
            }
        }

        private void OnUndoPerformed()
        {
            if (_IsSimulating)
            {
                StopSimulation();
            }
        }

        private void OnSceneChanged(Scene arg0, Scene arg1)
        {
            StopSimulation();
        }

        private void OnSceneGUI(SceneView obj)
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Semicolon)
            {
                Event.current.Use();
                ToggleSimulation();
            }
        }

#region Variables

        private int Tickrate = 33;
        private float Speed = 1f;
        private Vector3 Gravity = new Vector3(0, -9.8f, 0f);

        private readonly List<HoldRigidbody> _FrozeSceneRigidbodies = new List<HoldRigidbody>(2048);
        private readonly List<PlayingRigidbody> _SimulatedObjects = new List<PlayingRigidbody>(64);
        private bool _IsSimulating;
        private Vector3 _OldGravity;
        private bool _OldAutoSimulatingFlag;

#endregion

#region Classes

        class HoldRigidbody : IEquatable<Rigidbody>
        {
            public readonly RigidbodyConstraints Constraints;
            public readonly bool IsKinematic;
            public readonly Rigidbody Target;

            public HoldRigidbody(Rigidbody rigid)
            {
                Target = rigid;
                IsKinematic = rigid.isKinematic;
                Constraints = rigid.constraints;
            }

            public bool Equals(Rigidbody other) => Target == other;

            public void Freeze()
            {
                Target.isKinematic = true;
                Target.constraints = RigidbodyConstraints.FreezeAll;
            }

            public void Revert()
            {
                Target.velocity = Vector3.zero;
                Target.angularVelocity = Vector3.zero;
                Target.isKinematic = IsKinematic;
                Target.constraints = Constraints;
            }
        }

        class PlayingRigidbody
        {
            public GameObject ActualGameObject;
            public bool ActuallyHadCollider;
            public bool ActuallyHadRigidbody;
            public CollisionDetectionMode OldDetectionMode;
            public Rigidbody TargetRigidbody;
            public PlayingRigidbody(GameObject obj) => ActualGameObject = obj;

            public void Start()
            {
                var collider = ActualGameObject.GetComponent<Collider>();
                ActuallyHadCollider = collider;

                if (!ActuallyHadCollider)
                {
                    collider = ActualGameObject.AddComponent<MeshCollider>();
                    (collider as MeshCollider).convex = true;
                }

                TargetRigidbody = ActualGameObject.GetComponent<Rigidbody>();
                ActuallyHadRigidbody = TargetRigidbody;

                if (!ActuallyHadRigidbody) TargetRigidbody = ActualGameObject.AddComponent<Rigidbody>();

                OldDetectionMode = TargetRigidbody.collisionDetectionMode;
                TargetRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                TargetRigidbody.velocity = Vector3.zero;
                TargetRigidbody.angularVelocity = Vector3.zero;
            }

            public void Stop()
            {
                if (ActuallyHadRigidbody)
                {
                    TargetRigidbody.collisionDetectionMode = OldDetectionMode;
                    TargetRigidbody.velocity = Vector3.zero;
                    TargetRigidbody.angularVelocity = Vector3.zero;
                }
                else
                {
                    DestroyImmediate(TargetRigidbody);
                    TargetRigidbody = null;
                }

                if (!ActuallyHadCollider)
                {
                    var collider = ActualGameObject.GetComponent<Collider>();
                    DestroyImmediate(collider);
                    collider = null;
                }
            }
        }

#endregion
    }
}