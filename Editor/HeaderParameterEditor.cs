using UnityEditor;
using UnityEngine;

namespace PMP.HierarchyHelper {
    [CanEditMultipleObjects]
    [CustomEditor(typeof(HeaderParameter))]
    public class HeaderParameterEditor : Editor {

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            HeaderParameter data = target as HeaderParameter;

            if (data != null) {
                GUILayout.Space(10);
                data.SetBackgroundColor(EditorGUILayout.ColorField("背景色", data.GetBackgroundColor()));
                GUILayout.Space(2);
                data.SetTextColor(EditorGUILayout.ColorField("文字色", data.GetTextColor()));
                GUILayout.Space(4);
                data.SetUseFullWidthState(EditorGUILayout.ToggleLeft("横幅を最大で表示するか", data.GetUseFullWidth()));
                GUILayout.Space(4);
                EditorGUILayout.LabelField("ツールチップ");
                string tooltipText = data.GetTooltipText();
                data.SetTooltipText(EditorGUILayout.TextArea(tooltipText,
                    new GUIStyle(EditorStyles.textArea) {
                        wordWrap = true,
                    }));
            }

            if (EditorGUI.EndChangeCheck()) {
                EditorApplication.RepaintHierarchyWindow();
                EditorUtility.SetDirty(data);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnDestroy() {
            if (!Application.isEditor || ((HeaderParameter)target) != null) return;
        }

    }
}
