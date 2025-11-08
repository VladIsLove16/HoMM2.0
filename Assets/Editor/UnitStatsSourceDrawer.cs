using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(UnitStatsSource))]
public class UnitStatsSourceDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var useInlineProp = property.FindPropertyRelative("UseInline");
        var referenceProp = property.FindPropertyRelative("Reference");
        var inlineProp = property.FindPropertyRelative("Inline");

        float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // UseInline
        if (useInlineProp != null && useInlineProp.boolValue)
        {
            if (inlineProp != null)
            {
                height += EditorGUI.GetPropertyHeight(inlineProp, true);
            }
        }
        else
        {
            if (referenceProp != null)
            {
                height += EditorGUI.GetPropertyHeight(referenceProp, true);
            }
        }

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        using (new EditorGUI.PropertyScope(position, label, property))
        {
            var useInlineProp = property.FindPropertyRelative("UseInline");
            var referenceProp = property.FindPropertyRelative("Reference");
            var inlineProp = property.FindPropertyRelative("Inline");

            // Draw label as foldout-like header
            position = EditorGUI.PrefixLabel(position, label);

            // Layout rects
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var y = position.y;
            var contentRect = new Rect(position.x, y, position.width, lineHeight);

            // UseInline toggle
            if (useInlineProp != null)
            {
                useInlineProp.boolValue = EditorGUI.ToggleLeft(contentRect, "Use Inline", useInlineProp.boolValue);
            }
            y += lineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Draw either Reference or Inline
            if (useInlineProp != null && useInlineProp.boolValue)
            {
                var h = EditorGUI.GetPropertyHeight(inlineProp, true);
                var r = new Rect(position.x, y, position.width, h);
                EditorGUI.PropertyField(r, inlineProp, includeChildren: true);
            }
            else
            {
                var h = EditorGUI.GetPropertyHeight(referenceProp, true);
                var r = new Rect(position.x, y, position.width, h);
                EditorGUI.PropertyField(r, referenceProp, new GUIContent("Reference"), includeChildren: true);
            }
        }
    }
}

