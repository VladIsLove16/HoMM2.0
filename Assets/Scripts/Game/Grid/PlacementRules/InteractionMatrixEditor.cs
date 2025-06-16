//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(InteractionMatrixSO))]
//public class InteractionMatrixEditor : Editor
//{
//    private InteractionMatrixSO matrixSO;

//    private void OnEnable()
//    {
//        matrixSO = (InteractionMatrixSO)target;

//        // Если матрица еще не инициализирована, инициализируем её
//        if (matrixSO.interactionMatrix == null || matrixSO.interactionMatrix.Length == 0)
//        {
//            matrixSO.InitializeMatrix();
//        }
//    }

//    public override void OnInspectorGUI()
//    {
//        // Отрисовка стандартных полей ScriptableObject (список юнитов и объектов)
//        DrawDefaultInspector();

//        EditorGUILayout.Space();
//        EditorGUILayout.LabelField("Interaction Matrix", EditorStyles.boldLabel);

//        if (matrixSO.units == null || matrixSO.gridObjects == null)
//        {
//            EditorGUILayout.HelpBox("Заполните списки юнитов и объектов", MessageType.Warning);
//            return;
//        }

//        // Если размеры списков юнитов и объектов изменились, обновляем матрицу
//        if (matrixSO.interactionMatrix.GetLength(0) != matrixSO.units.Length ||
//            matrixSO.interactionMatrix.GetLength(1) != matrixSO.gridObjects.Length)
//        {
//            matrixSO.InitializeMatrix();
//        }

//        // Визуализируем таблицу (матрицу) с галочками
//        EditorGUILayout.BeginVertical();

//        // Отображение заголовков объектов на сетке (столбцы)
//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("Units \\ Objects", GUILayout.Width(120));
//        foreach (var gridObject in matrixSO.gridObjects)
//        {
//            EditorGUILayout.LabelField(gridObject.name, GUILayout.Width(80));
//        }
//        EditorGUILayout.EndHorizontal();

//        // Отображение строк для юнитов и ячеек с галочками
//        for (int unitIndex = 0; unitIndex < matrixSO.units.Length; unitIndex++)
//        {
//            EditorGUILayout.BeginHorizontal();

//            // Отображаем имя юнита в начале строки
//            EditorGUILayout.LabelField(matrixSO.units[unitIndex].name, GUILayout.Width(120));

//            // Для каждого объекта отображаем галочку
//            for (int objectIndex = 0; objectIndex < matrixSO.gridObjects.Length; objectIndex++)
//            {
//                matrixSO.interactionMatrix[unitIndex, objectIndex] = EditorGUILayout.Toggle(matrixSO.interactionMatrix[unitIndex, objectIndex], GUILayout.Width(80));
//            }

//            EditorGUILayout.EndHorizontal();
//        }

//        EditorGUILayout.EndVertical();

//        // Обновление объекта, если были изменения
//        if (GUI.changed)
//        {
//            EditorUtility.SetDirty(matrixSO);
//        }
//    }
//}
