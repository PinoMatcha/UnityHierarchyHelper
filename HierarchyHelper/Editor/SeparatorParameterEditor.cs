using UnityEditor;
using UnityEngine;

namespace PMP.HierarchyHelper {
    [CustomEditor(typeof(SeparatorParameter))]
    public class SeparatorParameterEditor : Editor {

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            SeparatorParameter data = target as SeparatorParameter;

            if (data != null) {
                GUILayout.Space(10);
                data.SetBackgroundColor(EditorGUILayout.ColorField("背景色", data.GetBackgroundColor()));
                GUILayout.Space(2);
                data.SetTextColor(EditorGUILayout.ColorField("文字色", data.GetTextColor()));
            }

            if (EditorGUI.EndChangeCheck()) {
                EditorApplication.RepaintHierarchyWindow();
                EditorUtility.SetDirty(data);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnDestroy() {
            if (!Application.isEditor || ((SeparatorParameter)target) != null) return;
        }

    }
}