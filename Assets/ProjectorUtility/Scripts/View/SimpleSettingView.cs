using UnityEngine;
using UnityEngine.UI;
using UIComponent;

namespace ProjectorUtility.View
{
    public class SimpleSettingView : MonoBehaviour
    {
        public SliderUI blendWidthUI, blendCurveUI, blendAlphaUI, blendOffsetUI;
        public Toggle   twoProjectionToggle;
        public Button   saveButton, discardButton; 
    }
}