using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using UIComponent;
using Common;

namespace ProjectorUtility.View
{
    public class CommonSettingView : MonoBehaviour
    {
        public InputField numOfCol, numOfRow;
        public SliderUI   blacknessUI, curveUI;
        public Toggle     symmetryToggle, lerpedInputModeToggle;
        public Button     saveButton, discardButton;
    }
}