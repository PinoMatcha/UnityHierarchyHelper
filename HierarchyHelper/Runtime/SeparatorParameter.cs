using UnityEngine;

namespace PMP.HierarchyHelper {
    [RequireComponent(typeof(Transform))]
    public class SeparatorParameter : MonoBehaviour {

        // �w�i�F
        [SerializeField] Color bgColor = new Color(0.50f, 0.80f, 1.00f);
        // �����F
        [SerializeField] Color textColor = new Color(0.043f, 0.043f, 0.043f);

        /// <summary>
        /// �w�i�F���㏑�����܂��B
        /// </summary>
        public void SetBackgroundColor(Color newColor) => bgColor = newColor;

        /// <summary>
        /// �����F���㏑�����܂��B
        /// </summary>
        public void SetTextColor(Color newColor) => textColor = newColor;

        /// <summary>
        /// �w�i�F��Ԃ��܂��B
        /// </summary>
        public Color GetBackgroundColor() => bgColor;

        /// <summary>
        /// �����F��Ԃ��܂��B
        /// </summary>
        public Color GetTextColor() => textColor;

        /// <summary>
        /// ������2�F����Z�����F��Ԃ��܂��B
        /// </summary>
        /// <param name="baseColor">�x�[�X�̐F</param>
        /// <param name="multiplyColor">��Z����F</param>
        public Color BlendMultiply(Color baseColor, Color multiplyColor) {
            return baseColor * multiplyColor;
        }
    }
}