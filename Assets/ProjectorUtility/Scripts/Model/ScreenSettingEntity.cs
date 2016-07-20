using UnityEngine;

using UniRx;
using Utility;

namespace ProjectorUtility.Model
{
    /// <summary>
    /// Screen setting model.
    /// Handle load and save.
    /// </summary>
    public class ScreenSettingEntity : IProjectorSettingEntity
    {
        public int ID { get; private set; }

        public ReactiveProperty<float> TopBlend { get; private set; }
        public ReactiveProperty<float> BottomBlend { get; private set; }
        public ReactiveProperty<float> LeftBlend { get; private set; }
        public ReactiveProperty<float> RightBlend { get; private set; }
        public ReactiveProperty<float> topMask { get; private set; }
        public ReactiveProperty<float> bottomMask { get; private set; }
        public ReactiveProperty<float> leftMask { get; private set; }
        public ReactiveProperty<float> rightMask { get; private set; }
        public ReactiveProperty<Vector2> topLeftMask { get; private set; }
        public ReactiveProperty<Vector2> topRightMask { get; private set; }
        public ReactiveProperty<Vector2> bottomLeftMask { get; private set; }
        public ReactiveProperty<Vector2> bottomRightMask { get; private set; }
        public ReactiveProperty<Vector2> uvShift { get; private set; }

        string _topBlendProp, _bottomBlendProp, _leftBlendProp, _rightBlendProp,
               _topMaskProp, _bottomMaskProp, _leftMaskProp, _rightMaskProp,
               _topLeftMaskProp, _topRightMaskProp, _bottomLeftMaskProp, _bottomRightMaskProp, _uvShiftProp;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id"></param>
        public ScreenSettingEntity(int id)
        {
            ID = id;

            string suffix        = ID.ToString();
            _topBlendProp        = "topBlendProp" + suffix;
            _bottomBlendProp     = "bottomBlendProp" + suffix;
            _leftBlendProp       = "leftBlendProp" + suffix;
            _rightBlendProp      = "rightBlendProp" + suffix;
            _topMaskProp         = "topMaskProp" + suffix;
            _bottomMaskProp      = "bottomMaskProp" + suffix;
            _leftMaskProp        = "leftMaskProp" + suffix;
            _rightMaskProp       = "rightMaskProp" + suffix;
            _topLeftMaskProp     = "topLeftMaskProp" + suffix;
            _topRightMaskProp    = "topRightMaskProp" + suffix;
            _bottomLeftMaskProp  = "bottomLeftMaskProp" + suffix;
            _bottomRightMaskProp = "bottomMaskProp" + suffix;
            _uvShiftProp         = "uvShiftProp" + suffix;

            InitialLoad();
        }

        /// <summary>
        /// Initialize and load screen settings.
        /// </summary>
        private void InitialLoad()
        {
            TopBlend        = new ReactiveProperty<float>(XmlSaver.Get<float>(_topBlendProp, 0f));
            BottomBlend     = new ReactiveProperty<float>(XmlSaver.Get<float>(_bottomBlendProp, 0f));
            LeftBlend       = new ReactiveProperty<float>(XmlSaver.Get<float>(_leftBlendProp, 0f));
            RightBlend      = new ReactiveProperty<float>(XmlSaver.Get<float>(_rightBlendProp, 0f));
            topMask         = new ReactiveProperty<float>(XmlSaver.Get<float>(_topMaskProp, 0f));
            bottomMask      = new ReactiveProperty<float>(XmlSaver.Get<float>(_bottomMaskProp, 0f));
            leftMask        = new ReactiveProperty<float>(XmlSaver.Get<float>(_leftMaskProp, 0f));
            rightMask       = new ReactiveProperty<float>(XmlSaver.Get<float>(_rightMaskProp, 0f));
            topLeftMask     = new ReactiveProperty<Vector2>(XmlSaver.Get<Vector2>(_topLeftMaskProp, Vector2.zero));
            topRightMask    = new ReactiveProperty<Vector2>(XmlSaver.Get<Vector2>(_topRightMaskProp, Vector2.zero));
            bottomLeftMask  = new ReactiveProperty<Vector2>(XmlSaver.Get<Vector2>(_bottomLeftMaskProp, Vector2.zero));
            bottomRightMask = new ReactiveProperty<Vector2>(XmlSaver.Get<Vector2>(_bottomRightMaskProp, Vector2.zero));
            uvShift         = new ReactiveProperty<Vector2>(XmlSaver.Get<Vector2>(_uvShiftProp, Vector2.zero));
        }

        /// <summary>
        /// Load settings.
        /// </summary>
        public void Load()
        {
            TopBlend.Value        = XmlSaver.Get<float>(_topBlendProp, 0f);
            BottomBlend.Value     = XmlSaver.Get<float>(_bottomBlendProp, 0f);
            LeftBlend.Value       = XmlSaver.Get<float>(_leftBlendProp, 0f);
            RightBlend.Value      = XmlSaver.Get<float>(_rightBlendProp, 0f);
            topMask.Value         = XmlSaver.Get<float>(_topMaskProp, 0f);
            bottomMask.Value      = XmlSaver.Get<float>(_bottomMaskProp, 0f);
            leftMask.Value        = XmlSaver.Get<float>(_leftMaskProp, 0f);
            rightMask.Value       = XmlSaver.Get<float>(_rightMaskProp, 0f);
            topLeftMask.Value     = XmlSaver.Get<Vector2>(_topLeftMaskProp, Vector2.zero);
            topRightMask.Value    = XmlSaver.Get<Vector2>(_topRightMaskProp, Vector2.zero);
            bottomLeftMask.Value  = XmlSaver.Get<Vector2>(_bottomLeftMaskProp, Vector2.zero);
            bottomRightMask.Value = XmlSaver.Get<Vector2>(_bottomRightMaskProp, Vector2.zero);
            uvShift.Value         = XmlSaver.Get<Vector2>(_uvShiftProp, Vector2.zero);
        }

        /// <summary>
        /// Save settings.
        /// </summary>
        public void Save()
        {
            XmlSaver.Set<float>(_topBlendProp, TopBlend.Value);
            XmlSaver.Set<float>(_bottomBlendProp, BottomBlend.Value);
            XmlSaver.Set<float>(_leftBlendProp, LeftBlend.Value);
            XmlSaver.Set<float>(_rightBlendProp, RightBlend.Value);
            XmlSaver.Set<float>(_topMaskProp, topMask.Value);
            XmlSaver.Set<float>(_bottomMaskProp, bottomMask.Value);
            XmlSaver.Set<float>(_leftMaskProp, leftMask.Value);
            XmlSaver.Set<float>(_rightMaskProp, rightMask.Value);
            XmlSaver.Set<Vector2>(_topLeftMaskProp, topLeftMask.Value);
            XmlSaver.Set<Vector2>(_topRightMaskProp, topRightMask.Value);
            XmlSaver.Set<Vector2>(_bottomLeftMaskProp, bottomLeftMask.Value);
            XmlSaver.Set<Vector2>(_bottomRightMaskProp, bottomRightMask.Value);
            XmlSaver.Save();
        }
    } 
}
