using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

namespace UIComponent
{
	public class SliderUI : MonoBehaviour
	{
		public Slider slider;
		public InputField inputField;
        public float upDownStep = 0.0001f;
		private bool up,down;

        public void SliderOnValueChaged(float val)
		{
			inputField.text = val.ToString ();
		}
		public void InputFieldOnValueChaged()
		{
			if (inputField.text == "" || inputField.text == null) inputField.text = "0";
			slider.value = float.Parse (inputField.text);
		}
		public void SetVal(float val)
		{
			slider.value = val;
			SliderOnValueChaged(val);
		}
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) == true)
            {
                up = true;
            }
            if (Input.GetKeyUp(KeyCode.UpArrow) == true)
            {
                up = false;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) == true)
            {
                down = true;
            }
            if (Input.GetKeyUp(KeyCode.DownArrow) == true)
            {
                down = false;
            }
            if (inputField.isFocused == true)
            {
                if (up == true)
                {
					var upValue = float.Parse(inputField.text) + upDownStep;
					if(slider.maxValue > upValue)
					{
						SetVal(upValue);
					}else
					{
						SetVal(slider.maxValue);
					}
                }
                if (down == true)
                {
					var downValue = float.Parse(inputField.text) - upDownStep;
					if(slider.minValue < downValue)
					{
						SetVal(downValue);
					}else
					{
						SetVal(slider.minValue);
					}
                }
            }
        }
	}
}
