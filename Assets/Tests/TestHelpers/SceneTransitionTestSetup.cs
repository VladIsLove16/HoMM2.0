using System.Reflection;
using UnityEngine;

namespace Tests.TestHelpers
{
    public static class SceneTransitionTestSetup
    {
        private static readonly FieldInfo InstanceField = typeof(SceneTransitionDataService)
            .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);

        public static SceneTransitionDataService EnsureService()
        {
            if (SceneTransitionDataService.Instance != null)
            {
                return SceneTransitionDataService.Instance;
            }

            var go = new GameObject("SceneTransitionDataService_Test");
            var service = go.AddComponent<SceneTransitionDataService>();

            // Awake may not fire immediately in edit mode tests; assign the singleton manually.
            InstanceField?.SetValue(null, service);
            return service;
        }

        public static void DestroyService()
        {
            var instance = SceneTransitionDataService.Instance;
            if (instance == null)
            {
                return;
            }

            Object.DestroyImmediate(instance.gameObject);
            InstanceField?.SetValue(null, null);
        }
    }
}
