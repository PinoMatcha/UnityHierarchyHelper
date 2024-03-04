using UnityEngine;

namespace PMP.HierarchyHelper {
    [RequireComponent(typeof(Transform))]
    public class SeparatorParameter : MonoBehaviour {

        // 横幅をフルにするか
        [SerializeField] bool usefullWidth = false;
        // 背景色
        [SerializeField] Color bgColor = new Color(0.50f, 0.86f, 1.00f);
        // 文字色
        [SerializeField] Color textColor = new Color(0.043f, 0.043f, 0.043f);
        // ツールチップ
        [SerializeField, TextArea] string tooltipText = "";

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

        /// <summary>
        /// Tooltipの文字を設定します。
        /// </summary>
        public void SetTooltipText(string msg) => tooltipText = msg;

        /// <summary>
        /// Tooltipの文字を返します。
        /// </summary>
        public string GetTooltipText() => tooltipText;

        /// <summary>
        /// 横幅をフルにするかどうかを設定します。
        /// </summary>
        public bool SetUseFullWidthState(bool newState) => usefullWidth = newState;

        /// <summary>
        /// 横幅をフルにするかどうかを返します。
        /// </summary>
        public bool GetUseFullWidth() => usefullWidth;
    }
}
