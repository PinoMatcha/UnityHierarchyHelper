using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
#endif

namespace PMP.HierarchyHelper {
    public class HierarchyHelper : Editor {

        // 現在のイベント
        internal static Event currentEvent;

        [MenuItem("GameObject/PM Presents/Hierarchy Helper/Create Hierarchy Separator", false, 20)]
        [MenuItem("Tools/PM Presents/Hierarchy Helper/Create Hierarchy Separator", false, 20)]
        static void CreateHierarchySeparator() {
            var go = new GameObject("Separator");
            go.transform.position = new Vector3();
            go.SetActive(false);
            var sParam = go.AddComponent<SeparatorParameter>();

            if (!EditorGUIUtility.isProSkin) {
                sParam.SetBackgroundColor(new Color(0.60f, 0.80f, 0.95f));
                sParam.SetTextColor(new Color(0.043f, 0.043f, 0.043f));
            } else {
                sParam.SetBackgroundColor(new Color(0.30f, 0.60f, 0.75f));
                sParam.SetTextColor(new Color(0.8235295f, 0.8235295f, 0.8235295f));
            }

            Selection.activeGameObject = go;

            Debug.Log("[PM Presents] Hierarchy Separator を作成しました。");
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
        private static Texture2D _pixelWhite;
        private static Texture2D pixelWhite {
            get {
                if (_pixelWhite == null) {
                    _pixelWhite = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    _pixelWhite.SetPixel(0, 0, Color.white);
                    _pixelWhite.Apply();
                }

                return _pixelWhite;
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
                bool isSeparator = activeObject.GetComponent<SeparatorParameter>() != null;
                Tools.hidden = isSeparator;
            }
        }

        private static void HierarchyOnGUI(int instanceID, Rect selectionRect) {

            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            // Sceneはreturn
            if (gameObject == null) return;

            #region Draw Separator

            SeparatorParameter sParam = gameObject.GetComponent<SeparatorParameter>();
            if (sParam) {
                DrawSeparator(gameObject.name, selectionRect, sParam);
                gameObject.hideFlags = HideFlags.None;
                SceneVisibilityManager svmIns = SceneVisibilityManager.instance;
                if (svmIns.IsHidden(gameObject) || svmIns.IsPickingDisabled(gameObject)) {
                    svmIns.Show(gameObject, true);
                    svmIns.EnablePicking(gameObject, true);
                }
            }

            #endregion

            #region Draw Stripe

            // 仕切り用のオブジェクトでなければ行を色分け
            var index = (int)(selectionRect.y + STRIPE_OFFSET_Y) / ROW_HEIGHT;
            if (!sParam) DrawStripedLines(index, selectionRect);

            #endregion

            #region Draw Tree

            bool hasParent = gameObject.transform.parent != null;
            bool hasChilds = gameObject.transform.childCount > 0;

            if (hasParent) {
                int nestLevel = -1;
                Transform checkTrns = gameObject.transform;
                List<Transform> parentsList = new List<Transform>();
                while (checkTrns.parent != null) {
                    nestLevel++;
                    parentsList.Add(checkTrns.parent);
                    checkTrns = checkTrns.parent;
                }

                DrawHorizontalLine(selectionRect, nestLevel, hasChilds);
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

            // Repaint
            EditorApplication.RepaintHierarchyWindow();
        }

        private static void DrawSeparator(string name, Rect selectionRect, SeparatorParameter param) {

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
            rect.x += GLOBAL_SPACE_OFFSET_LEFT;
            rect.width -= GLOBAL_SPACE_OFFSET_LEFT;

            Color guiColor = GUI.color;
            GUI.color = bgColor;
            GUI.DrawTexture(rect, pixelWhite, ScaleMode.StretchToFill);

            var content = new GUIContent($"- {name.ToString()} -");
            rect.x += (rect.width - Styles.Header.CalcSize(content).x) / 2;
            GUI.color = textColor;
            GUI.Label(rect, content, Styles.Header);
            GUI.color = guiColor;
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
    }
}