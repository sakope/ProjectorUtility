using UnityEngine;
using System.Collections;

using UIComponent;

namespace ProjectorUtility.View
{
    public class ScreenSettingView : MonoBehaviour
    {
        public SliderUI topBlendUI, bottomBlendUI, leftBlendUI, rightBlendUI,
                        topMaskUI, bottomMaskUI, leftMaskUI, rightMaskUI,
                        topLeftMaskXUI, topLeftMaskYUI, topRightMaskXUI, topRightMaskYUI, bottomLeftMaskXUI, bottomLeftMaskYUI, bottomRightMaskXUI, bottomRightMaskYUI,
                        uvShiftX, uvShiftY;
    }
}