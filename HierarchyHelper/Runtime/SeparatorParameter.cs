using UnityEngine;

namespace PMP.HierarchyHelper {
    [RequireComponent(typeof(Transform))]
    public class SeparatorParameter : MonoBehaviour {

        // 背景色
        [SerializeField] Color bgColor = new Color(0.50f, 0.80f, 1.00f);
        // 文字色
        [SerializeField] Color textColor = new Color(0.043f, 0.043f, 0.043f);

        /// <summary>
        /// 背景色を上書きします。
        /// </summary>
        public void SetBackgroundColor(Color newColor) => bgColor = newColor;

        /// <summary>
        /// 文字色を上書きします。
        /// </summary>
        public void SetTextColor(Color newColor) => textColor = newColor;

        /// <summary>
        /// 背景色を返します。
        /// </summary>
        public Color GetBackgroundColor() => bgColor;

        /// <summary>
        /// 文字色を返します。
        /// </summary>
        public Color GetTextColor() => textColor;

        /// <summary>
        /// 引数の2色を乗算した色を返します。
        /// </summary>
        /// <param name="baseColor">ベースの色</param>
        /// <param name="multiplyColor">乗算する色</param>
        public Color BlendMultiply(Color baseColor, Color multiplyColor) {
            return baseColor * multiplyColor;
        }
    }
}