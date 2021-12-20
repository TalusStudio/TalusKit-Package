using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace TalusKit.Editor.CheckWindow
{
    public abstract class Check
    {
        public abstract string Name { get; }

        public abstract bool DefaultEnabled { get; }
        public abstract IEnumerable<CheckResult> GetResults(SceneObjects sceneObjects);
        public abstract void Resolve(CheckResult result);
        public abstract string[] ResolutionActions { get; }

#region STATIC

        public static List<Check> AllChecks
        {
            get
            {
                if (_Checks == null)
                {
                    Initialize();
                }

                return _Checks.Values.ToList();
            }
        }

        private static Dictionary<Type, Check> _Checks;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            _Checks = new Dictionary<Type, Check>();
            IEnumerable<Type> types = GetAllTypes();

            foreach(Type type in types)
            {
                var check = (Check)Activator.CreateInstance(type);
                _Checks.Add(type, check);
            }
        }

        public static T Get<T>() where T : Check
        {
            if (_Checks.ContainsKey(typeof(T)))
            {
                return (T) _Checks[typeof(T)];
            }

            Debug.LogError($"Check of type '{typeof(T)}' could not be accessed.");
            return null;
        }

        public static bool Has<T>() where T : Check => (_Checks.ContainsKey(typeof(T)));

        private static IEnumerable<Type> GetAllTypes()
        {
            var types = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] assemblyTypes = null;

                try
                {
                    assemblyTypes = assembly.GetTypes();
                }
                catch
                {
                    Debug.LogError($"Could not load types from assembly : {assembly.FullName}");
                }

                if (assemblyTypes == null)
                {
                    continue;
                }

                types.AddRange(assemblyTypes.Where(t => typeof(Check).IsAssignableFrom(t) && !t.IsAbstract));
            }
            return types.ToArray();
        }
        #endregion
    }

}
