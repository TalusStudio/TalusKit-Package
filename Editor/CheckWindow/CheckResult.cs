using System.Collections.Generic;
using System.Linq;

using TalusKit.Runtime.CheckWindow;

using UnityEditor;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace TalusKit.Editor.CheckWindow
{
    public class CheckResult
    {
        public enum Result
        {
            Notice,
            Warning,
            Failed
        }

        private readonly Object[] _Objects;
        public readonly GUIContent Message;

        public int ResolutionActionIndex;

        public CheckResult(Check check, Result resultInstance, string message, params Object[] objects)
        {
            Check = check;
            result = resultInstance;
            Message = new GUIContent(message, GetIcon(resultInstance));
            _Objects = objects;
        }

        public Check Check { get; }
        public Result result { get; }

        public Object MainObject
        {
            get
            {
                if (_Objects != null && _Objects.Length > 0)
                {
                    return _Objects[0];
                }

                return null;
            }
        }

        public string Action => Check.ResolutionActions[ResolutionActionIndex];

        public static Texture GetIcon(Result r)
        {
            switch (r)
            {
                default:
                case Result.Notice:
                    return Contents.Notice;

                case Result.Warning:
                    return Contents.Warning;

                case Result.Failed:
                    return Contents.Failed;
            }
        }

        public void SetIgnored(bool ignored = true)
        {
            if (!(MainObject is GameObject))
            {
                return;
            }

            var go = MainObject as GameObject;
            Scene scene = go.scene;
            IgnoredCheckResults[] ignoredResults = Object.FindObjectsOfType<IgnoredCheckResults>();
            IgnoredCheckResults targetResults = ignoredResults.FirstOrDefault(result => result.gameObject.scene == scene);

            if (targetResults == null)
            {
                var newGo = new GameObject("__TALUSKIT__IGNORED_CHECK_RESULTS");
                SceneManager.MoveGameObjectToScene(newGo, scene);
                targetResults = newGo.AddComponent<IgnoredCheckResults>();
            }

            if (targetResults.IgnoredResults == null)
            {
                targetResults.IgnoredResults = new List<IgnoredCheckResults.IgnoredCheckResult>();
            }

            if (ignored)
            {
                if (!targetResults.IgnoredResults.Any(o => o.GameObject == go && o.Check == Check.GetType().ToString()))
                {
                    var r = new IgnoredCheckResults.IgnoredCheckResult()
                    {
                        Check = Check.GetType().ToString(),
                        GameObject = go
                    };

                    targetResults.IgnoredResults.Add(r);
                }
            }
            else
            {
                if (targetResults.IgnoredResults.Any(o => o.GameObject == go && o.Check == Check.GetType().ToString()))
                {
                    targetResults.IgnoredResults.Remove(
                        targetResults.IgnoredResults.FirstOrDefault(o =>
                                o.GameObject == go && o.Check == Check.GetType().ToString()));
                }
            }
        }

        static class Contents
        {
            public static Texture Notice;
            public static Texture Warning;
            public static Texture Failed;

            static Contents()
            {
                Notice = EditorGUIUtility.IconContent("console.infoicon.sml").image;
                Warning = EditorGUIUtility.IconContent("console.warnicon.sml").image;
                Failed = EditorGUIUtility.IconContent("console.erroricon.sml").image;
            }
        }
    }
}
