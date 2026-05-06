using Adventure.Infrastructure.Inventory;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

namespace Development.Editor
{
    public sealed class MushroomPainterWindow : EditorWindow
    {
        private const string DefaultRootName = "Mushrooms";

        [SerializeField] private MushroomCollectible mushroomPrefab;
        [SerializeField] private Transform placementRoot;
        [SerializeField] private bool paintMode;
        [SerializeField] private bool alignToSurface = true;
        [SerializeField] private bool randomYaw = true;
        [SerializeField] private Vector2 uniformScaleRange = Vector2.one;
        [SerializeField] private LayerMask placementMask = ~0;

        [MenuItem("Tools/Adventure/Mushroom Painter")]
        public static void Open()
        {
            GetWindow<MushroomPainterWindow>("Mushroom Painter");
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGui;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGui;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Scene Paint", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                mushroomPrefab = (MushroomCollectible)EditorGUILayout.ObjectField(
                    "Mushroom Prefab",
                    mushroomPrefab,
                    typeof(MushroomCollectible),
                    false);

                placementRoot = (Transform)EditorGUILayout.ObjectField(
                    "Placement Root",
                    placementRoot,
                    typeof(Transform),
                    true);

                alignToSurface = EditorGUILayout.Toggle("Align To Surface", alignToSurface);
                randomYaw = EditorGUILayout.Toggle("Random Yaw", randomYaw);
                uniformScaleRange = DrawScaleRange("Uniform Scale", uniformScaleRange);
                placementMask = LayerMaskField("Placement Mask", placementMask);
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Use Selected Prefab"))
                {
                    TryAssignSelectedPrefab();
                }

                if (GUILayout.Button("Find/Create Mushrooms Root"))
                {
                    placementRoot = ResolveOrCreateRoot();
                }
            }

            EditorGUILayout.Space();

            var nextPaintMode = EditorGUILayout.ToggleLeft("Paint Mode", paintMode);
            if (nextPaintMode != paintMode)
            {
                paintMode = nextPaintMode;
                SceneView.RepaintAll();
            }

            if (paintMode && mushroomPrefab == null)
            {
                EditorGUILayout.HelpBox("Assign a MushroomCollectible prefab before painting.", MessageType.Warning);
            }
            else if (paintMode)
            {
                EditorGUILayout.HelpBox("Left click in Scene view to place a mushroom. Hold Alt to orbit without painting.", MessageType.Info);
            }
        }

        private void OnSceneGui(SceneView sceneView)
        {
            if (!paintMode)
                return;

            if (mushroomPrefab == null)
                return;

            var evt = Event.current;
            if (evt == null)
                return;

            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            if (evt.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(controlId);
            }

            var ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
            if (!Physics.Raycast(ray, out var hit, 1000f, placementMask))
                return;

            DrawPreview(hit);

            if (evt.alt)
                return;

            if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                PlaceMushroom(hit);
                evt.Use();
            }
        }

        private void DrawPreview(RaycastHit hit)
        {
            Handles.color = Color.green;
            Handles.DrawWireDisc(hit.point, hit.normal, 0.35f);
            Handles.DrawLine(hit.point, hit.point + hit.normal * 0.5f);
        }

        private void PlaceMushroom(RaycastHit hit)
        {
            var parent = placementRoot != null ? placementRoot : ResolveOrCreateRoot();
            var prefabAsset = mushroomPrefab.gameObject;
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
            if (instance == null)
                return;

            Undo.RegisterCreatedObjectUndo(instance, "Paint Mushroom");

            if (parent != null)
            {
                Undo.SetTransformParent(instance.transform, parent, "Parent Painted Mushroom");
            }

            var rotation = CalculatePlacementRotation(hit.normal);
            var scale = GetRandomUniformScale(prefabAsset.transform.localScale);
            instance.transform.SetPositionAndRotation(hit.point, rotation);
            instance.transform.localScale = scale;

            Selection.activeGameObject = instance;
            EditorSceneManager.MarkSceneDirty(instance.scene);
        }

        private Quaternion CalculatePlacementRotation(Vector3 normal)
        {
            var baseRotation = alignToSurface
                ? Quaternion.FromToRotation(Vector3.up, normal)
                : Quaternion.identity;

            if (!randomYaw)
                return baseRotation;

            var yawAxis = alignToSurface ? normal : Vector3.up;
            return Quaternion.AngleAxis(Random.Range(0f, 360f), yawAxis) * baseRotation;
        }

        private Vector3 GetRandomUniformScale(Vector3 baseScale)
        {
            var min = Mathf.Max(0.01f, Mathf.Min(uniformScaleRange.x, uniformScaleRange.y));
            var max = Mathf.Max(min, Mathf.Max(uniformScaleRange.x, uniformScaleRange.y));
            var scale = Random.Range(min, max);
            return baseScale * scale;
        }

        private void TryAssignSelectedPrefab()
        {
            if (Selection.activeObject is not GameObject selected)
                return;

            if (PrefabUtility.GetPrefabAssetType(selected) == PrefabAssetType.NotAPrefab)
                return;

            var collectible = selected.GetComponent<MushroomCollectible>();
            if (collectible != null)
            {
                mushroomPrefab = collectible;
            }
        }

        private Transform ResolveOrCreateRoot()
        {
            var existing = GameObject.Find(DefaultRootName);
            if (existing != null)
                return existing.transform;

            var root = new GameObject(DefaultRootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Mushrooms Root");
            EditorSceneManager.MarkSceneDirty(root.scene);
            return root.transform;
        }

        private static Vector2 DrawScaleRange(string label, Vector2 range)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            range.x = EditorGUILayout.FloatField(range.x, GUILayout.MaxWidth(70f));
            GUILayout.Label("..", GUILayout.Width(20f));
            range.y = EditorGUILayout.FloatField(range.y, GUILayout.MaxWidth(70f));
            EditorGUILayout.EndHorizontal();
            return range;
        }

        private static LayerMask LayerMaskField(string label, LayerMask layerMask)
        {
            var layerNames = InternalEditorUtility.layers;
            var layerNumbers = new int[layerNames.Length];

            for (var i = 0; i < layerNames.Length; i++)
            {
                layerNumbers[i] = LayerMask.NameToLayer(layerNames[i]);
            }

            var maskWithoutEmpty = 0;
            for (var i = 0; i < layerNumbers.Length; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) != 0)
                {
                    maskWithoutEmpty |= 1 << i;
                }
            }

            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layerNames);

            var mask = 0;
            for (var i = 0; i < layerNumbers.Length; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) != 0)
                {
                    mask |= 1 << layerNumbers[i];
                }
            }

            layerMask.value = mask;
            return layerMask;
        }
    }
}
