using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Meta.Data.GUI
{
    [CustomEditor(typeof(Metadata)), CanEditMultipleObjects]
    public class MetadataEditor : Editor
    {
        private ReorderableList orderList;

        private void OnEnable()
        {
            orderList = new(serializedObject, serializedObject.FindProperty("_params"), true, true, true, true)
            {
                drawElementCallback = DrawListItems,
                drawHeaderCallback = DrawHeader
            };
        }

        public override void OnInspectorGUI () 
        {
            DrawDefaultInspector();
            serializedObject.Update();
            orderList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = orderList.serializedProperty.GetArrayElementAtIndex(index);

            EditorGUI.PropertyField(
            new Rect(rect.x, rect.y, 90, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("id"), GUIContent.none
            );

            EditorGUI.PropertyField(
            new Rect(rect.x + 92, rect.y, rect.width - 92, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("parameter"), GUIContent.none
            );
        }

        void DrawHeader(Rect rect)
        {
            string name = "Parameter";
            EditorGUI.LabelField(rect, name);
        }
    }
}