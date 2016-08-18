using UnityEngine;
using UnityEngine.UI;
using System;

namespace ProjectorUtility.View
{
    public class RectMaskSettingView : MonoBehaviour
    {
        [Serializable] public struct RectMaskSet
        {
            public InputField xInput, yInput, widthInput, heightInput;
        }

        public RectMaskSet[] rectMaskInputs = new RectMaskSet[MAX_RECTMASKS];
        public Button saveButton, discardButton;

        public const int MAX_RECTMASKS = 4;

        void OnValidate()
        {
            if (rectMaskInputs.Length != MAX_RECTMASKS)
            {
                Debug.LogWarning("Do not resize rect mask array");
                Array.Resize(ref rectMaskInputs, MAX_RECTMASKS);
            }
        }
    }
}