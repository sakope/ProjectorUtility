using UnityEngine;
using UnityEngine.UI;
using System;

namespace UIComponent
{
    public class NumericItem : MonoBehaviour
    {
        [SerializeField]
        private Text label;
        [SerializeField]
        private InputField inputField;
        public int Value { get { return int.Parse(inputField.text); } set { this.inputField.text = value.ToString(); } }
        public void Initialize (string label, int max)
        {
            this.label.text = label;
            this.Value = max;
        }
        public void OnValueChangedHandller (Action<int> handller)
        {
            inputField.onValueChanged.AddListener(e => OnValueChanged(e, handller));
        }
        private void OnValueChanged (string maxCount, Action<int> handller)
        {
            if (string.IsNullOrEmpty(maxCount) == false)
            {
                handller(int.Parse(maxCount));
            } else
            {
                this.Value = 0;
            }
        }
    }
}
