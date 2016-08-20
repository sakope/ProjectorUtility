using UnityEngine;
using UnityEngine.UI;

namespace UIComponent
{
    [RequireComponent(typeof(InputField))]
    public class NumericUpDownInputUI : MonoBehaviour
    {
        [SerializeField]
        float upDownStep = 0.0001f;

        InputField inputField;

        void Awake()
        {
            inputField = GetComponent<InputField>();
            inputField.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>(s => { if (s == "" || s == null) inputField.text = "0"; }));
        }

        void Update()
        {
            if (inputField.isFocused == false) return;

            if (Input.GetKey(KeyCode.UpArrow) == true)
            {
                float value;
                inputField.text = ((float.TryParse(inputField.text, out value) ? value : 0) + upDownStep).ToString();
            }
            else if (Input.GetKey(KeyCode.DownArrow) == true)
            {
                float value;
                inputField.text = ((float.TryParse(inputField.text, out value) ? value : 0) - upDownStep).ToString();
            }
        }
    }
}