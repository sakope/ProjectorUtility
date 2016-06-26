﻿using UniRx;
using Utility;

namespace ProjectorUtility.Model
{
    /// <summary>
    /// Common setting model.
    /// Handle load and save.
    /// </summary>
    public class CommonSettingEntity : IProjectorSettingEntity
    {
        public ReactiveProperty<int>   NumOfColProjectors { get; private set; }
        public ReactiveProperty<int>   NumOfRowProjectors { get; private set; }
        public ReactiveProperty<float> Blackness { get; private set; }
        public ReactiveProperty<float> Curve { get; private set; }
        public ReactiveProperty<bool>  ApplyAll { get; private set; }
        public ReactiveProperty<bool>  LerpedInputMode { get; private set; }

        string _numOfColProjectorsProp = "numOfColProjectors";
        string _numOfRowProjectorsProp = "numOfRowProjectors";
        string _blacknessProp          = "blackness";
        string _curveProp              = "curve";
        string _applyAllProp           = "applyAll";
        string _lerpedInputModeProp    = "sensorMode";

        /// <summary>
        /// Constructor.
        /// </summary>
        public CommonSettingEntity()
        {
            InitialLoad();
        }

        /// <summary>
        /// Initialize and Load common settings.
        /// </summary>
        private void InitialLoad()
        {
            NumOfColProjectors = new ReactiveProperty<int>(XmlSaver.Get<int>(_numOfColProjectorsProp, 1));
            NumOfRowProjectors = new ReactiveProperty<int>(XmlSaver.Get<int>(_numOfRowProjectorsProp, 1));
            Blackness          = new ReactiveProperty<float>(XmlSaver.Get<float>(_blacknessProp, 1.0f));
            Curve              = new ReactiveProperty<float>(XmlSaver.Get<float>(_curveProp, 1.0f));
            ApplyAll           = new ReactiveProperty<bool>(XmlSaver.Get<bool>(_applyAllProp, false));
            LerpedInputMode    = new ReactiveProperty<bool>(XmlSaver.Get<bool>(_lerpedInputModeProp, true));
        }

        /// <summary>
        /// Load common settings.
        /// </summary>
        public void Load()
        {
            NumOfColProjectors.Value = XmlSaver.Get<int>(_numOfColProjectorsProp, 1);
            NumOfRowProjectors.Value = XmlSaver.Get<int>(_numOfRowProjectorsProp, 1);
            Blackness.Value          = XmlSaver.Get<float>(_blacknessProp, 1.0f);
            Curve.Value              = XmlSaver.Get<float>(_curveProp, 1.0f);
            ApplyAll.Value           = XmlSaver.Get<bool>(_applyAllProp, false);
            LerpedInputMode.Value    = XmlSaver.Get<bool>(_lerpedInputModeProp, false);
        }

        /// <summary>
        /// Save common settings.
        /// </summary>
        public void Save()
        {
            XmlSaver.Set<int>(_numOfColProjectorsProp, NumOfColProjectors.Value);
            XmlSaver.Set<int>(_numOfRowProjectorsProp, NumOfRowProjectors.Value);
            XmlSaver.Set<float>(_blacknessProp, Blackness.Value);
            XmlSaver.Set<float>(_curveProp, Curve.Value);
            XmlSaver.Set<bool>(_applyAllProp, ApplyAll.Value);
            XmlSaver.Set<bool>(_lerpedInputModeProp, LerpedInputMode.Value);
            XmlSaver.Save();
        }
    }
}