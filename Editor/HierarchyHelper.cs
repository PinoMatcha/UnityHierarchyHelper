using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
#endif

namespace PMP.HierarchyHelper {
    public class HierarchyHelper : Editor {

        // 現在のイベント
        internal static Event currentEvent;

        [MenuItem("GameObject/PM Presents/Hierarchy Helper/Create Header", false, 20)]
        [MenuItem("Tools/PM Presents/Hierarchy Helper/Create Header", false, 20)]
        static void CreateHeader() {
            var go = new GameObject("Header");
            go.transform.position = new Vector3();
            go.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            go.transform.localScale = Vector3.one;
            go.transform.hideFlags = HideFlags.HideInInspector;
            var param = go.AddComponent<HeaderParameter>();

            param.Reset();

            Undo.RegisterCreatedObjectUndo(go, $"Create Header '{go.name}'");

            Selection.activeGameObject = go;

            Debug.Log("[PM Presents/Hierarchy Helper] Header を作成しました。");
        }


        [MenuItem("GameObject/PM Presents/Hierarchy Helper/Create Separator", false, 20)]
        [MenuItem("Tools/PM Presents/Hierarchy Helper/Create Separator", false, 20)]
        static void CreateSeparator() {
            var go = new GameObject("/=Separator");
            go.transform.position = new Vector3();
            go.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            go.transform.localScale = Vector3.one;
            go.transform.hideFlags = HideFlags.HideInInspector;

            Undo.RegisterCreatedObjectUndo(go, $"Create Separator '{go.name}'");

            Debug.Log("[PM Presents/Hierarchy Helper] Separator を作成しました。");
        }

        internal const int GLOBAL_SPACE_OFFSET_LEFT = 16 * 2;

        internal const int ROW_HEIGHT = 16;
        internal const int STRIPE_OFFSET_X = 3;
        internal const int STRIPE_OFFSET_Y = -4;
        internal const int STRIPE_BAR_WIDTH = 2;
        internal static readonly Color STRIPE_COLOR = new Color(0, 0, 0, 0.08f);

        private static Color TREE_COLOR_LIGHT = Color.black;
        private static Color TREE_COLOR_DARK = Color.black;
        internal static Color TREE_COLOR {
            get {
                if (!EditorGUIUtility.isProSkin) {
                    if (TREE_COLOR_LIGHT == Color.black) {
                        TREE_COLOR_LIGHT = new Color(0.90f, 0.90f, 0.90f, 1.00f);
                    }
                    return TREE_COLOR_LIGHT;
                } else {
                    if (TREE_COLOR_DARK == Color.black) {
                        TREE_COLOR_DARK = new Color(0.50f, 0.50f, 0.50f, 1.00f);
                    }
                    return TREE_COLOR_DARK;
                }
            }
        }

        // 1x1のホワイトピクセル
        private static Texture2D pixelWhite => Texture2D.whiteTexture;

        static Color GetColor(string htmlColor) {
            if (!ColorUtility.TryParseHtmlString(htmlColor, out var color)) throw new ArgumentException();
            return color;
        }

        public static Color HighlightBackgroundInactive {
            get {
                if (EditorGUIUtility.isProSkin) return GetColor("#4D4D4D");
                else return GetColor("#AEAEAE");
            }
        }

        public static Color HighlightBackground {
            get {
                if (EditorGUIUtility.isProSkin) return GetColor("#2C5D87");
                else return GetColor("#3A72B0");
            }
        }

        public static Color WindowBackground {
            get {
                if (EditorGUIUtility.isProSkin) return GetColor("#383838");
                else return GetColor("#C8C8C8");
            }
        }

        [InitializeOnLoadMethod]
        private static void Initialize() {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyOnGUI;
            EditorApplication.update += EditorUpdate;
        }

        private static void EditorUpdate() {
            // イベント更新
            currentEvent = Event.current;

            var activeObject = Selection.activeGameObject;
            if (activeObject) {
                bool isHeader = activeObject.GetComponent<HeaderParameter>() != null;
                Tools.hidden = isHeader;
            }
        }

        private static Color GetBackgroundColor(Rect srcRect, int instanceID) {
            Color result;
            var isHover = srcRect.Contains(currentEvent.mousePosition);

            if (Selection.Contains(instanceID)) {
                if (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.titleContent.text == "Hierarchy") {
                    result = HighlightBackground;
                } else {
                    result = HighlightBackgroundInactive;
                }
            } else if (isHover) {
                result = HighlightBackgroundInactive;
            } else {
                result = WindowBackground;
            }

            return result;
        }

        private static void TrySetGameObjectActive(GameObject gameObject, bool active) {
            Undo.RecordObject(gameObject, $"{(active ? "Activate" : "Deactivate")} GameObject '{gameObject.name}'");
            gameObject.SetActive(active);
            EditorUtility.SetDirty(gameObject);
        }

        static bool clicked = false;
        static Vector2 lmbDownPos;

        private static void DrawHeader(string name, Rect selectionRect, HeaderParameter param, bool hasParent, bool hasChildren) {

            // 文字色  
            Color textColor = param.GetTextColor();
            // 背景色
            Color bgColor = param.GetBackgroundColor();

            if (EditorApplication.isPlayingOrWillChangePlaymode) {
                textColor = param.BlendMultiply(textColor, GUI.color);
                bgColor = param.BlendMultiply(bgColor, GUI.color);
            }

            Rect rect = EditorGUIUtility.PixelsToPoints(RectFromLeft(selectionRect, Screen.width));
            rect.y = selectionRect.y;
            rect.height = selectionRect.height;
            if (!param.GetUseFullWidth()) {
                if (hasParent || hasChildren) {
                    rect.width = selectionRect.width + 16;
                    rect.x = selectionRect.x;
                } else {
                    rect = RectFromLeft(rect, rect.width - GLOBAL_SPACE_OFFSET_LEFT);
                    rect.x = rect.x + GLOBAL_SPACE_OFFSET_LEFT;
                }
            }

            Color guiColor = GUI.color;
            GUI.color = bgColor;
            GUI.DrawTexture(rect, pixelWhite, ScaleMode.StretchToFill);

            var content = new GUIContent($"- {name} -");
            string tooltip = param.GetTooltipText();
            if (tooltip != "") content.tooltip = tooltip;
            GUI.color = textColor;
            var labelStyles = new GUIStyle(Styles.Header);
            labelStyles.alignment = TextAnchor.MiddleCenter;
            GUI.Label(rect, content, labelStyles);

            // リセット
            GUI.color = guiColor;
        }

        private static void DrawSeparator(Rect srcRect, int instanceID) {
            // Draw background
            EditorGUI.DrawRect(srcRect, GetBackgroundColor(srcRect, instanceID));

            var rect = srcRect;
            rect.y += srcRect.height * 0.5f;
            rect.xMax += 14;
            rect.height = 1f;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f));
        }

        static Rect RectFromLeft(Rect rect, float width) {
            rect.xMin = 0;
            rect.width = width;
            return rect;
        }

        internal static class Styles {
            internal static GUIStyle Header = new GUIStyle(TreeBoldLabel) {
                richText = true,
                normal = new GUIStyleState() { textColor = Color.white }
            };

            internal static GUIStyle TreeBoldLabel {
                get { return TreeView.DefaultStyles.boldLabel; }
            }
        }

        static float GetStartX(Rect srcRect, int nestLevel) {
            return GLOBAL_SPACE_OFFSET_LEFT + 16 + STRIPE_OFFSET_X + (srcRect.height - 2) * nestLevel;
        }

        public static void DrawFullVerticalLine(Rect srcRect, int nestLevel) {
            DrawHalfVerticalLine(srcRect, true, nestLevel);
            DrawHalfVerticalLine(srcRect, false, nestLevel);
        }

        public static void DrawHalfVerticalLine(Rect srcRect, bool startsOnTop, int nestLevel) {
            float x = GetStartX(srcRect, nestLevel);
            float y = startsOnTop ? srcRect.y : (srcRect.y + srcRect.height / 2f);
            float w = STRIPE_BAR_WIDTH;
            float h = srcRect.height / 2f;
            EditorGUI.DrawRect(new Rect(x, y, w, h), TREE_COLOR);
        }

        public static void DrawHorizontalLine(Rect srcRect, int nestLevel, bool hasChilds) {
            float x = GetStartX(srcRect, nestLevel);
            float y = srcRect.y + srcRect.height / 2f;
            float w = srcRect.height + (hasChilds ? -8 : 2);
            float h = STRIPE_BAR_WIDTH;
            EditorGUI.DrawRect(new Rect(x, y, w, h), TREE_COLOR);
        }

        static void DrawStripedLines(int index, Rect selectionRect) {
            if (index % 2 == 0) return;

            var xMax = selectionRect.xMax;

            selectionRect.x = 32;
            selectionRect.xMax = xMax + 16;

            EditorGUI.DrawRect(selectionRect, STRIPE_COLOR);
        }

        private static void HierarchyOnGUI(int instanceID, Rect selectionRect) {

            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            currentEvent = Event.current;

            // Sceneはreturn
            if (gameObject == null) return;

            #region Draw Header

            HeaderParameter isHeader = gameObject.GetComponent<HeaderParameter>();
            if (isHeader) {
                DrawHeader(gameObject.name, selectionRect, isHeader, gameObject.transform.parent, gameObject.transform.childCount > 0);
                gameObject.transform.hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector;
                SceneVisibilityManager svmIns = SceneVisibilityManager.instance;
                if (svmIns.IsHidden(gameObject) || svmIns.IsPickingDisabled(gameObject)) {
                    svmIns.Show(gameObject, true);
                    svmIns.EnablePicking(gameObject, true);
                }
            }

            #endregion

            #region Draw Separator

            bool isSeparator = gameObject.name.StartsWith("/=Separator");
            if (isSeparator) {
                DrawSeparator(selectionRect, instanceID);
            }

            #endregion

            #region Draw Stripe

            var index = (int)(selectionRect.y + STRIPE_OFFSET_Y) / ROW_HEIGHT;
            if (!isHeader) DrawStripedLines(index, selectionRect);

            #endregion

            #region Draw Tree

            bool hasParent = gameObject.transform.parent != null;
            bool hasChildren = gameObject.transform.childCount > 0;

            if (hasParent) {
                int nestLevel = -1;
                Transform checkTrns = gameObject.transform;
                List<Transform> parentsList = new List<Transform>();
                while (checkTrns.parent != null) {
                    nestLevel++;
                    parentsList.Add(checkTrns.parent);
                    checkTrns = checkTrns.parent;
                }

                if (!isHeader && !isSeparator) DrawHorizontalLine(selectionRect, nestLevel, hasChildren);
                if (gameObject.transform.GetSiblingIndex() < gameObject.transform.parent.childCount - 1) {
                    DrawFullVerticalLine(selectionRect, nestLevel);
                } else {
                    DrawHalfVerticalLine(selectionRect, true, nestLevel);
                }

                for (int i = nestLevel; i > 0; i--) {
                    if (parentsList == null || parentsList.Count == 0) continue;

                    var parent = parentsList[nestLevel - i];
                    var prevParent = parentsList[nestLevel - (i - 1)];
                    if (parent != null && prevParent != null) {
                        if (parent.GetSiblingIndex() < prevParent.childCount - 1) {
                            DrawFullVerticalLine(selectionRect, i - 1);
                        }
                    }
                }

                parentsList.Clear();
            }

            #endregion

            #region Draw Toggle

            if (!isHeader && !isSeparator) {
                var rect = selectionRect;
                rect.x = rect.xMax + 1f;
                rect.width = 16f;

                var active = GUI.Toggle(rect, gameObject.activeSelf, string.Empty);
                if (currentEvent.button == 0) {
                    if (active != gameObject.activeSelf) {
                        if (Selection.gameObjects.Length > 0) {
                            foreach (var go in Selection.gameObjects) {
                                TrySetGameObjectActive(go, active);
                            }
                        } else {
                            TrySetGameObjectActive(gameObject, active);
                        }
                    }
                }

                bool lmbDown = currentEvent.button == 0 && currentEvent.type == EventType.MouseDown;

                if (lmbDown) {
                    lmbDownPos = currentEvent.mousePosition;
                    clicked = true;
                }

                if (rect.Contains(lmbDownPos)) {
                    if (clicked) {
                        clicked = false;
                        Debug.Log(gameObject.name);
                    }
                }
            }

            #endregion

            // Repaint
            EditorApplication.RepaintHierarchyWindow();
        }
    }
}
