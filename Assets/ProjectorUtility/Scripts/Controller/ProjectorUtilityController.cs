using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

using UniRx;
using Common;

namespace ProjectorUtility.Controller
{
    using Model;
    using View;

    /// <summary>
    /// compute buffer
    /// </summary>
    struct ProjectorUtilityBuffer
    {
        public uint screenID;
        public float overlapTop, overlapBottom, overlapLeft, overlapRight;
        public float uvShiftX, uvShiftY;
        public float maskTop, maskBottom, maskLeft, maskRight;

        public ProjectorUtilityBuffer(uint screenID, float overlapTop, float overlapBottom, float overlapLeft, float overlapRight, float uvShiftX, float uvShiftY, float maskTop, float maskBottom, float maskLeft, float maskRight)
        {
            this.screenID = screenID;
            this.overlapTop = overlapTop;
            this.overlapBottom = overlapBottom;
            this.overlapLeft = overlapLeft;
            this.overlapRight = overlapRight;
            this.uvShiftX = uvShiftX;
            this.uvShiftY = uvShiftY;
            this.maskTop = maskTop;
            this.maskBottom = maskBottom;
            this.maskLeft = maskLeft;
            this.maskRight = maskRight;
        }
    }
    
    /// <summary>
    /// Handle views and models
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class ProjectorUtilityController : SingletonMonoBehaviour<ProjectorUtilityController>
    {
        #region variables

        [Serializable] public struct TabContentsSet
        {
            public Button     tab;
            public GameObject contents;
        }
        [Serializable] public struct KeyViewModalSet
        {
            public KeyCode    key;
            public GameObject modal;
        }
        [SerializeField] Shader              _shader;
        [SerializeField] TabContentsSet      _commonTabContents;
        [SerializeField] TabContentsSet      _screenTabContents;
        [SerializeField] RectMaskSettingView _rectMaskSettingView;
        [SerializeField] SimpleSettingView   _simpleSettingView;
        [SerializeField] Transform           _tabParent;
        [SerializeField] Transform           _contentsParent;
        [SerializeField] KeyViewModalSet     _simpleMode;
        [SerializeField] KeyViewModalSet     _advancedMode;
        [SerializeField] KeyViewModalSet     _rectMaskMode;

        CommonSettingEntity       _commonSettingEntity;
        CommonSettingView         _commonSettingView;
        RectMaskSettingEntity     _rectMaskSettingEntity;
        List<ScreenSettingEntity> _screenSettingEntities = new List<ScreenSettingEntity>();
        List<ScreenSettingView>   _screenSettingViews    = new List<ScreenSettingView>();
        List<TabContentsSet>      _tabs = new List<TabContentsSet>();
        List<KeyViewModalSet>     _keyViewModalSets = new List<KeyViewModalSet>();

        ComputeBuffer _computeBuffer;
        Camera        _camera;
        Material      _mat;
        Mesh          _maskMesh;
        List<Mesh>    _maskingMeshes = new List<Mesh>();

        int _colScreens, _rowScreens;

        const int MAX_COL = 30;
        const int MAX_ROW = 30;

        public int NumOfScreen { get; private set; }
        public CommonSettingEntity GetCommonSettingEntity { get { return _commonSettingEntity; } }
        public RectMaskSettingEntity GetRectMaskSettingEntity { get { return _rectMaskSettingEntity; } }
        public List<ScreenSettingEntity> GetScreenSettingEntities { get { return _screenSettingEntities; } }

        public float NormalizedMinUpperBlendHeight { get; private set; }
        public float NormalizedMinLowerBlendHeight { get; private set; }
        public float NormalizedMinLeftBlendWidth   { get; private set; }
        public float NormalizedMinRightBlendWidth  { get; private set; }
        
        #endregion


        #region API

        /// <summary>
        /// Normalized total blend height of specific column.
        /// </summary>
        /// <param name="col">screen colmun number</param>
        /// <returns>Return normalized total blend height of specific column</returns>
        public float NormalizedBlendHeight(int col)
        {
            float tbh = 0f;
            if (_colScreens == 1 || col < 1)
                col = 1;
            else if (col > _colScreens)
                col = _colScreens;
            for (int i = col - 1; i <= col - 1 + _colScreens * (_rowScreens - 1); i += _colScreens)
            {
                tbh += _screenSettingEntities[i].TopBlend.Value + _screenSettingEntities[i].BottomBlend.Value;
            }
            return tbh;
        }

        /// <summary>
        /// Normalized total blend width of specific row.
        /// </summary>
        /// <param name="row">screen row number</param>
        /// <returns>Return normalized total blend width of specific row</returns>
        public float NormalizedBlendWidth(int row)
        {
            float tbw = 0f;
            if (_rowScreens == 1 || row < 1)
                row = 1;
            else if (row > _rowScreens)
                row = _rowScreens;
            for (int i = _colScreens * row - _colScreens ; i < _colScreens * row; i++)
            {
                tbw += _screenSettingEntities[i].LeftBlend.Value + _screenSettingEntities[i].RightBlend.Value;
            }
            return tbw;
        }

        /// <summary>
        /// Total screen blending width of specific column.
        /// </summary>
        /// <param name="col">screen column number</param>
        /// <returns>Return total screen blending width of specific column</returns>
        public float BlendingHeight(int col)
        {
            return NormalizedBlendHeight(col) * Screen.height;
        }

        /// <summary>
        /// Total screen blend height of specific row.
        /// </summary>
        /// <param name="row">screen row number</param>
        /// <returns>Return total screen blending height of specific row</returns>
        public float BlendingWidth(int row)
        {
            return NormalizedBlendWidth(row) * Screen.width;
        }
        
        /// <summary>
        /// Narrowest upper blend height in screen space coord.
        /// </summary>
        /// <returns>Narrowest upper blend height</returns>
        public float MinUpperBlendHeight()
        {
            return NormalizedMinUpperBlendHeight * Screen.height;
        }

        /// <summary>
        /// Narrowest lower blend height in screen space coord.
        /// </summary>
        /// <returns>Narrowest lower blend height</returns>
        public float MinLowerBlendHeight()
        {
            return NormalizedMinLowerBlendHeight * Screen.height;
        }

        /// <summary>
        /// Narrowest left blend width in screen space coord.
        /// </summary>
        /// <returns>Narrowest left blend width</returns>
        public float MinLeftBlendWidth()
        {
            return NormalizedMinLeftBlendWidth * Screen.width;
        }

        /// <summary>
        /// Narrowest right blend width in screen space coord.
        /// </summary>
        /// <returns>Narrowest right blend width</returns>
        public float MinRightBlendWidth()
        {
            return NormalizedMinRightBlendWidth * Screen.width;
        }

        /// <summary>
        /// Return adjusted viewport position for blend and uv shift.
        /// If lerped sensor mode is true, return lerped adjusted value for each col, row and uv shift.
        /// If not, return adjust value for each projectors blends and shift without lerp. 
        /// </summary>
        /// <param name="position">Viewport position</param>
        /// <returns>Adjusted viewport position</returns>
        public Vector2 GetAdjustedPosition(Vector2 position)
        {
            if(NumOfScreen == 1)
            {
                return position;
            }

            // Inverse y to match shader uv coord.
            Vector2 adjustPosition = new Vector2(position.x, 1f - position.y);

            int currentCol = Mathf.FloorToInt(adjustPosition.x / (1.0f / _colScreens));
            int currentRow = Mathf.FloorToInt(adjustPosition.y / (1.0f / _rowScreens));
            int screenID   = currentCol + currentRow * _colScreens;

            if (screenID >= _colScreens * _rowScreens)
            {
                return Vector2.one;
            }

            int leftOverlapCount  = Mathf.FloorToInt((float)_colScreens / 2f) - currentCol;
            int rightOverlapCount = currentCol + 1 - Mathf.CeilToInt((float)_colScreens / 2f);
            int upperOverlapCount = Mathf.FloorToInt((float)_rowScreens / 2f) - currentRow;
            int lowerOverlapCount = currentRow + 1 - Mathf.CeilToInt((float)_rowScreens / 2f);

            if (_commonSettingEntity.LerpedInputMode.Value)
            {
                float upperBlends = 0f, lowerBlends = 0f, leftBlends = 0f, rightBlends = 0f;

                if (leftOverlapCount > 0)
                {
                    for (int i = 0; i < leftOverlapCount; i++)
                    {
                        if (_colScreens % 2 != 0 || _colScreens / 2 != currentCol + 1 + i)
                        {
                            leftBlends += _screenSettingEntities[screenID + i + 1].LeftBlend.Value;
                        }
                        leftBlends += _screenSettingEntities[screenID + i].RightBlend.Value;
                    }
                }
                if (rightOverlapCount > 0)
                {
                    for (int i = 0; i < rightOverlapCount; i++)
                    {
                        if (_colScreens % 2 != 0 || _colScreens / 2 != _colScreens - currentCol + i)
                        {
                            rightBlends += _screenSettingEntities[screenID - i - 1].RightBlend.Value;
                        }
                        rightBlends += _screenSettingEntities[screenID - i].LeftBlend.Value;
                    }
                }
                if (upperOverlapCount > 0)
                {
                    for (int i = 0; i < upperOverlapCount; i++)
                    {
                        if (_rowScreens % 2 != 0 || _rowScreens / 2 != currentRow + 1 + i)
                        {
                            upperBlends += _screenSettingEntities[screenID + (i + 1) * _colScreens].TopBlend.Value;
                        }
                        upperBlends += _screenSettingEntities[screenID + i * _colScreens].BottomBlend.Value;
                    }
                }
                if (lowerOverlapCount > 0)
                {
                    for (int i = 0; i < lowerOverlapCount; i++)
                    {
                        if (_rowScreens % 2 != 0 || _rowScreens / 2 != _rowScreens - currentRow + i)
                        {
                            lowerBlends += _screenSettingEntities[screenID - (i + 1) * _colScreens].BottomBlend.Value;
                        }
                        lowerBlends += _screenSettingEntities[screenID - i * _colScreens].TopBlend.Value;
                    }
                }

                adjustPosition.x = Mathf.Lerp(leftBlends,  1f - rightBlends, adjustPosition.x);
                adjustPosition.y = Mathf.Lerp(upperBlends, 1f - lowerBlends, adjustPosition.y);
            }
            else
            {
                if (leftOverlapCount > 0)
                {
                    for (int i = 0; i < leftOverlapCount; i++)
                    {
                        if (_colScreens % 2 != 0 || _colScreens / 2 != currentCol + 1 + i)
                        {
                            adjustPosition.x += _screenSettingEntities[screenID + i + 1].LeftBlend.Value;
                        }
                        adjustPosition.x += _screenSettingEntities[screenID + i].RightBlend.Value;
                    }
                }
                if (rightOverlapCount > 0)
                {
                    for (int i = 0; i < rightOverlapCount; i++)
                    {
                        if (_colScreens % 2 != 0 || _colScreens / 2 != _colScreens - currentCol + i)
                        {
                            adjustPosition.x -= _screenSettingEntities[screenID - i - 1].RightBlend.Value;
                        }
                        adjustPosition.x -= _screenSettingEntities[screenID - i].LeftBlend.Value;
                    }
                }
                if (upperOverlapCount > 0)
                {
                    for (int i = 0; i < upperOverlapCount; i++)
                    {
                        if (_rowScreens % 2 != 0 || _rowScreens / 2 != currentRow + 1 + i)
                        {
                            adjustPosition.y += _screenSettingEntities[screenID + (i + 1) * _colScreens].TopBlend.Value;
                        }
                        adjustPosition.y += _screenSettingEntities[screenID + i * _colScreens].BottomBlend.Value;
                    }
                }
                if (lowerOverlapCount > 0)
                {
                    for (int i = 0; i < lowerOverlapCount; i++)
                    {
                        if (_rowScreens % 2 != 0 || _rowScreens / 2 != _rowScreens - currentRow + i)
                        {
                            adjustPosition.y -= _screenSettingEntities[screenID - (i + 1) * _colScreens].BottomBlend.Value;
                        }
                        adjustPosition.y -= _screenSettingEntities[screenID - i * _colScreens].TopBlend.Value;
                    }
                }
            }

            adjustPosition += _screenSettingEntities[screenID].uvShift.Value;

            //inverse y to match viewport coord.
            adjustPosition = new Vector2(adjustPosition.x, 1 - adjustPosition.y);

            return adjustPosition;
        }

        #endregion


        #region initialize

        protected override void Awake()
        {
            base.Awake();
            _mat = new Material(_shader);
            _camera = GetComponent<Camera>();

            _keyViewModalSets.Add(_simpleMode);
            _keyViewModalSets.Add(_advancedMode);
            _keyViewModalSets.Add(_rectMaskMode);

            _commonSettingEntity   = new CommonSettingEntity();
            _commonSettingView     = _commonTabContents.contents.GetComponent<CommonSettingView>();
            _rectMaskSettingEntity = new RectMaskSettingEntity();

            _tabs.Add(_commonTabContents);

            BuildCommonSetting();
            BuildRectMaskSetting();
            BuildScreenSetting();
            BuildSimpleSetting();
            SetTabSystem();

            NormalizedMinUpperBlendHeight = 0f;
            NormalizedMinLowerBlendHeight = 0f;
            NormalizedMinLeftBlendWidth   = 0f;
            NormalizedMinRightBlendWidth  = 0f;
        }

        #endregion


        #region build views and models

        /// <summary>
        /// Build common setting view and connect view <-> model reactives.
        /// </summary>
        void BuildCommonSetting()
        {
            //From model to view reactives
            _commonSettingEntity.NumOfColProjectors.Subscribe(v => _commonSettingView.numOfCol.text = v.ToString());
            _commonSettingEntity.NumOfRowProjectors.Subscribe(v => _commonSettingView.numOfRow.text = v.ToString());
            _commonSettingEntity.Blackness.Subscribe(v => _commonSettingView.blacknessUI.SetVal(v));
            _commonSettingEntity.Curve.Subscribe(v => _commonSettingView.curveUI.SetVal(v));
            _commonSettingEntity.Brightness.Subscribe(v => _commonSettingView.brightnessUI.SetVal(v));
            _commonSettingEntity.Symmetry.Subscribe(v => _commonSettingView.symmetryToggle.isOn = v);
            _commonSettingEntity.LerpedInputMode.Subscribe(v => _commonSettingView.lerpedInputModeToggle.isOn = v);

            //From view to model reactives
            _commonSettingView.numOfCol.OnValueChangedAsObservable().Where(s => int.Parse(s) < MAX_COL).Subscribe(s => _commonSettingEntity.NumOfColProjectors.Value = int.Parse(s));
            _commonSettingView.numOfRow.OnValueChangedAsObservable().Where(s => int.Parse(s) < MAX_ROW).Subscribe(s => _commonSettingEntity.NumOfRowProjectors.Value = int.Parse(s));
            _commonSettingView.blacknessUI.slider.OnValueChangedAsObservable().Subscribe(v => _commonSettingEntity.Blackness.Value = v);
            _commonSettingView.curveUI.slider.OnValueChangedAsObservable().Subscribe(v => _commonSettingEntity.Curve.Value = v);
            _commonSettingView.brightnessUI.slider.OnValueChangedAsObservable().Subscribe(v => _commonSettingEntity.Brightness.Value = v);
            _commonSettingView.symmetryToggle.OnValueChangedAsObservable().Subscribe(v => _commonSettingEntity.Symmetry.Value = v);
            _commonSettingView.lerpedInputModeToggle.OnValueChangedAsObservable().Subscribe(v => _commonSettingEntity.LerpedInputMode.Value = v);
            _commonSettingView.saveButton.OnClickAsObservable().Subscribe(_ => SaveAndCloseAdvancedMode());
            _commonSettingView.discardButton.OnClickAsObservable().Subscribe(_ => DiscardAndCloseAdvancedMode());
            _commonSettingView.gammaButton.OnClickAsObservable().Subscribe(_ => _commonSettingEntity.Curve.Value = CommonSettingEntity.GAMMA_CURVE);

            //From model to core reactive (skip initialize).
            _commonSettingEntity.NumOfColProjectors.SkipLatestValueOnSubscribe().Subscribe(n => BuildScreenSetting());
            _commonSettingEntity.NumOfRowProjectors.SkipLatestValueOnSubscribe().Subscribe(n => BuildScreenSetting());
            _commonSettingEntity.Blackness.SkipLatestValueOnSubscribe().Subscribe(n => UpdateBlend());
            _commonSettingEntity.Curve.SkipLatestValueOnSubscribe().Subscribe(n => UpdateBlend());
            _commonSettingEntity.Brightness.SkipLatestValueOnSubscribe().Subscribe(n => UpdateBlend());
        }

        void BuildRectMaskSetting()
        {
            for (int i = 0; i < RectMaskSettingEntity.MAX_RECTMASKS; i++)
            {
                //NOTE:Escape for linq invoke timing.
                var id = i;

                //From model to view reactives (with initialize).
                _rectMaskSettingEntity.RectMaskX[id].Subscribe(v => _rectMaskSettingView.rectMaskInputs[id].xInput.text = v.ToString());
                _rectMaskSettingEntity.RectMaskY[id].Subscribe(v => _rectMaskSettingView.rectMaskInputs[id].yInput.text = v.ToString());
                _rectMaskSettingEntity.RectMaskWidth[id].Subscribe(v => _rectMaskSettingView.rectMaskInputs[id].widthInput.text = v.ToString());
                _rectMaskSettingEntity.RectMaskHeight[id].Subscribe(v => _rectMaskSettingView.rectMaskInputs[id].heightInput.text = v.ToString());

                //From view to model reactives
                _rectMaskSettingView.rectMaskInputs[id].xInput.OnValueChangedAsObservable().Subscribe(v => _rectMaskSettingEntity.RectMaskX[id].Value = (v == "" || v == null) ? 0 : float.Parse(v));
                _rectMaskSettingView.rectMaskInputs[id].yInput.OnValueChangedAsObservable().Subscribe(v => _rectMaskSettingEntity.RectMaskY[id].Value = (v == "" || v == null) ? 0 : float.Parse(v));
                _rectMaskSettingView.rectMaskInputs[id].widthInput.OnValueChangedAsObservable().Subscribe(v => _rectMaskSettingEntity.RectMaskWidth[id].Value = (v == "" || v == null) ? 0 : float.Parse(v));
                _rectMaskSettingView.rectMaskInputs[id].heightInput.OnValueChangedAsObservable().Subscribe(v => _rectMaskSettingEntity.RectMaskHeight[id].Value = (v == "" || v == null) ? 0 : float.Parse(v));
                _rectMaskSettingView.saveButton.OnClickAsObservable().Subscribe(_ => SaveAndCloseRectMaskMode());
                _rectMaskSettingView.discardButton.OnClickAsObservable().Subscribe(_ => DiscardAndCloseRectMaskMode());

                //From model to core reactive (skip initialize).
                _rectMaskSettingEntity.RectMaskX[id].SkipLatestValueOnSubscribe().Subscribe(v => UpdateMaskMesh());
                _rectMaskSettingEntity.RectMaskY[id].SkipLatestValueOnSubscribe().Subscribe(v => UpdateMaskMesh());
                _rectMaskSettingEntity.RectMaskWidth[id].SkipLatestValueOnSubscribe().Subscribe(v => UpdateMaskMesh());
                _rectMaskSettingEntity.RectMaskHeight[id].SkipLatestValueOnSubscribe().Subscribe(v => UpdateMaskMesh());
            }
        }

        /// <summary>
        /// Build screen setting tabs, views, models and connect view <-> model reactives.
        /// </summary>
        void BuildScreenSetting()
        {
            //if number of projectors is 0, force change 1.
            if (_commonSettingEntity.NumOfColProjectors.Value < 1) _commonSettingEntity.NumOfColProjectors.Value = 1;
            if (_commonSettingEntity.NumOfRowProjectors.Value < 1) _commonSettingEntity.NumOfRowProjectors.Value = 1;

            _colScreens = _commonSettingEntity.NumOfColProjectors.Value;
            _rowScreens = _commonSettingEntity.NumOfRowProjectors.Value;
            NumOfScreen = _colScreens * _rowScreens;

            if(NumOfScreen > _screenSettingEntities.Count)
            {
                //Add tab, view and entity.
                for (int i = _screenSettingEntities.Count; i < NumOfScreen; i++)
                {
                    //Setup tab system
                    TabContentsSet tabContentsSet = new TabContentsSet();
                    tabContentsSet.tab = Instantiate(_screenTabContents.tab) as Button;
                    tabContentsSet.tab.GetComponentInChildren<Text>().text = (i + 1).ToString();
                    tabContentsSet.tab.transform.SetParent(_tabParent, false);
                    tabContentsSet.tab.transform.SetAsLastSibling();
                    tabContentsSet.contents = Instantiate(_screenTabContents.contents) as GameObject;
                    tabContentsSet.contents.transform.SetParent(_contentsParent, false);
                    tabContentsSet.contents.transform.SetAsLastSibling();
                    _tabs.Add(tabContentsSet);

                    //Create screen setting view model instance
                    ScreenSettingView   screenSettingView   = tabContentsSet.contents.GetComponent<ScreenSettingView>();
                    ScreenSettingEntity screenSettingEntity = new ScreenSettingEntity(i);

                    //From model to other reactives (with initialize)
                    screenSettingEntity.TopBlend.Subscribe(v => screenSettingView.topBlendUI.SetVal(v));
                    screenSettingEntity.BottomBlend.Subscribe(v => screenSettingView.bottomBlendUI.SetVal(v));
                    screenSettingEntity.LeftBlend.Subscribe(v => screenSettingView.leftBlendUI.SetVal(v));
                    screenSettingEntity.RightBlend.Subscribe(v => screenSettingView.rightBlendUI.SetVal(v));
                    screenSettingEntity.topMask.Subscribe(v => screenSettingView.topMaskUI.SetVal(v));
                    screenSettingEntity.bottomMask.Subscribe(v => screenSettingView.bottomMaskUI.SetVal(v));
                    screenSettingEntity.leftMask.Subscribe(v => screenSettingView.leftMaskUI.SetVal(v));
                    screenSettingEntity.rightMask.Subscribe(v => screenSettingView.rightMaskUI.SetVal(v));
                    screenSettingEntity.topLeftMask.Subscribe(v => { screenSettingView.topLeftMaskXUI.SetVal(v.x); screenSettingView.topLeftMaskYUI.SetVal(v.y); });
                    screenSettingEntity.topRightMask.Subscribe(v => { screenSettingView.topRightMaskXUI.SetVal(v.x); screenSettingView.topRightMaskYUI.SetVal(v.y); });
                    screenSettingEntity.bottomLeftMask.Subscribe(v => { screenSettingView.bottomLeftMaskXUI.SetVal(v.x); screenSettingView.bottomLeftMaskYUI.SetVal(v.y); });
                    screenSettingEntity.bottomRightMask.Subscribe(v => { screenSettingView.bottomRightMaskXUI.SetVal(v.x); screenSettingView.bottomRightMaskYUI.SetVal(v.y); });
                    screenSettingEntity.uvShift.Subscribe(v => { screenSettingView.uvShiftX.SetVal(v.x); screenSettingView.uvShiftY.SetVal(v.y); });

                    //From view to model reactives
                    screenSettingView.topBlendUI.slider.OnValueChangedAsObservable().Subscribe(v => screenSettingEntity.TopBlend.Value = v);
                    screenSettingView.bottomBlendUI.slider.OnValueChangedAsObservable().Subscribe(v => screenSettingEntity.BottomBlend.Value = v);
                    screenSettingView.leftBlendUI.slider.OnValueChangedAsObservable().Subscribe(v => screenSettingEntity.LeftBlend.Value = v);
                    screenSettingView.rightBlendUI.slider.OnValueChangedAsObservable().Subscribe(v => screenSettingEntity.RightBlend.Value = v);
                    screenSettingView.topMaskUI.slider.OnValueChangedAsObservable().Subscribe(v => screenSettingEntity.topMask.Value = v);
                    screenSettingView.bottomMaskUI.slider.OnValueChangedAsObservable().Subscribe(v => screenSettingEntity.bottomMask.Value = v);
                    screenSettingView.leftMaskUI.slider.OnValueChangedAsObservable().Subscribe(v => screenSettingEntity.leftMask.Value = v);
                    screenSettingView.rightMaskUI.slider.OnValueChangedAsObservable().Subscribe(v => screenSettingEntity.rightMask.Value = v);
                    screenSettingView.topLeftMaskXUI.slider.OnValueChangedAsObservable().Subscribe(v => screenSettingEntity.topLeftMask.Value = new Vector2(v, screenSettingEntity.topLeftMask.Value.y));
                    screenSettingView.topLeftMaskYUI.slider.OnValueChangedAsObservable().Subscribe(v => screenSettingEntity.topLeftMask.Value = new Vector2(screenSettingEntity.topLeftMask.Value.x, v));
                    screenSettingView.topRightMaskXUI.slider.OnValueChangedAsObservable().Subscribe(v => screenSettingEntity.topRightMask.Value = new Vector2(v, screenSettingEntity.topRightMask.Value.y));
                    screenSettingView.topRightMaskYUI.slider.OnValueChangedAsObservable().Subscribe(v => screenSettingEntity.topRightMask.Value = new Vector2(screenSettingEntity.topRightMask.Value.x, v));
                    screenSettingView.bottomLeftMaskXUI.slider.OnValueChangedAsObservable().Subscribe(v => screenSettingEntity.bottomLeftMask.Value = new Vector2(v, screenSettingEntity.bottomLeftMask.Value.y));
                    screenSettingView.bottomLeftMaskYUI.slider.OnValueChangedAsObservable().Subscribe(v => screenSettingEntity.bottomLeftMask.Value = new Vector2(screenSettingEntity.bottomLeftMask.Value.x, v));
                    screenSettingView.bottomRightMaskXUI.slider.OnValueChangedAsObservable().Subscribe(v => screenSettingEntity.bottomRightMask.Value = new Vector2(v, screenSettingEntity.bottomRightMask.Value.y));
                    screenSettingView.bottomRightMaskYUI.slider.OnValueChangedAsObservable().Subscribe(v => screenSettingEntity.bottomRightMask.Value = new Vector2(screenSettingEntity.bottomRightMask.Value.x, v));
                    screenSettingView.uvShiftX.slider.OnValueChangedAsObservable().Subscribe(v => screenSettingEntity.uvShift.Value = new Vector2(v, screenSettingEntity.uvShift.Value.y));
                    screenSettingView.uvShiftY.slider.OnValueChangedAsObservable().Subscribe(v => screenSettingEntity.uvShift.Value = new Vector2(screenSettingEntity.uvShift.Value.x, v));

                    //From model to other reactives (skip initialize)
                    var id = i;
                    screenSettingEntity.TopBlend.SkipLatestValueOnSubscribe().Subscribe(v => {
                        if (_commonSettingEntity.Symmetry.Value && id >= _colScreens) _screenSettingEntities[id - _colScreens].BottomBlend.Value = v;
                        UpdateBlend();
                    });
                    screenSettingEntity.BottomBlend.SkipLatestValueOnSubscribe().Subscribe(v => {
                        if (_commonSettingEntity.Symmetry.Value && id < NumOfScreen - _colScreens) _screenSettingEntities[id + _colScreens].TopBlend.Value = v;
                        UpdateBlend();
                    });
                    screenSettingEntity.LeftBlend.SkipLatestValueOnSubscribe().Subscribe(v => {
                        if (_commonSettingEntity.Symmetry.Value && (float)id % (float)_colScreens != 0) _screenSettingEntities[id - 1].RightBlend.Value = v;
                        UpdateBlend();
                    });
                    screenSettingEntity.RightBlend.SkipLatestValueOnSubscribe().Subscribe(v => {
                        if (_commonSettingEntity.Symmetry.Value && (float)(id + 1) % (float)_colScreens != 0) _screenSettingEntities[id + 1].LeftBlend.Value = v;
                        UpdateBlend();
                    });
                    screenSettingEntity.topMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlend());
                    screenSettingEntity.bottomMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlend());
                    screenSettingEntity.leftMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlend());
                    screenSettingEntity.rightMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlend());
                    screenSettingEntity.topLeftMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateMaskMesh());
                    screenSettingEntity.topRightMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateMaskMesh());
                    screenSettingEntity.bottomLeftMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateMaskMesh());
                    screenSettingEntity.bottomRightMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateMaskMesh());
                    screenSettingEntity.uvShift.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlend());

                    _screenSettingEntities.Add(screenSettingEntity);
                    _screenSettingViews.Add(screenSettingView);
                }
                SetTabSystem();
            }
            else if (NumOfScreen < _screenSettingEntities.Count)
            {
                //Reduce tab, view and entity.
                for (int i = _screenSettingEntities.Count; i > NumOfScreen; i--)
                {
                    Destroy(_tabs[i].tab.gameObject);
                    Destroy(_tabs[i].contents.gameObject);
                    _tabs.RemoveAt(i);
                    _screenSettingEntities.RemoveAt(_screenSettingEntities.Count - 1);
                }
                SetTabSystem();
            }
            UpdateBlend();
            UpdateMaskMesh();
        }

        void BuildSimpleSetting()
        {
            //From view to model reactives
            _simpleSettingView.twoProjectionToggle.OnValueChangedAsObservable().Subscribe(v => {
                _commonSettingEntity.NumOfRowProjectors.Value = 1;
                _commonSettingEntity.NumOfColProjectors.Value = (v) ? 2 : 3;
            });
            _simpleSettingView.blendWidthUI.slider.OnValueChangedAsObservable().Subscribe(v => {
                for (int i = 0; i < (_colScreens - 1); i++)
                {
                    _screenSettingEntities[i].RightBlend.Value = v;
                }
            });
            _simpleSettingView.blendCurveUI.slider.OnValueChangedAsObservable().Subscribe(v => _commonSettingEntity.Curve.Value = v);
            _simpleSettingView.blendOffsetUI.slider.OnValueChangedAsObservable().Subscribe(v => _commonSettingEntity.Blackness.Value = v);
            _simpleSettingView.blendAlphaUI.slider.OnValueChangedAsObservable().Subscribe(v => _commonSettingEntity.Brightness.Value = v);
            _simpleSettingView.saveButton.OnClickAsObservable().Subscribe(_ => SaveAndCloseSimpleMode());
            _simpleSettingView.discardButton.OnClickAsObservable().Subscribe(_ => DiscardAndCloseSimpleMode());
        }

        void CalculateNarrowestBlend()
        {
            NormalizedMinLeftBlendWidth = NormalizedMinRightBlendWidth = NormalizedMinUpperBlendHeight = NormalizedMinLowerBlendHeight = 1;

            if (NumOfScreen != _colScreens * _rowScreens) return;
            if (_screenSettingEntities.Count != NumOfScreen) return;

            for (int i = 0; i < NumOfScreen; i++)
            {
                if (i % _colScreens == 0)
                {
                    if (NormalizedMinLeftBlendWidth >= _screenSettingEntities[i].LeftBlend.Value) NormalizedMinLeftBlendWidth = _screenSettingEntities[i].LeftBlend.Value;
                }
                if ((i + 1) % _colScreens == 0)
                {
                    if (NormalizedMinRightBlendWidth >= _screenSettingEntities[i].RightBlend.Value) NormalizedMinRightBlendWidth = _screenSettingEntities[i].RightBlend.Value;
                }
                if (i < _colScreens)
                {
                    if (NormalizedMinUpperBlendHeight >= _screenSettingEntities[i].TopBlend.Value) NormalizedMinUpperBlendHeight = _screenSettingEntities[i].TopBlend.Value;
                }
                if (i >= NumOfScreen - _colScreens)
                {
                    if (NormalizedMinLowerBlendHeight >= _screenSettingEntities[i].BottomBlend.Value) NormalizedMinLowerBlendHeight = _screenSettingEntities[i].BottomBlend.Value;
                }
            }
        }

        #endregion


        #region execute blend

        /// <summary>
        /// refine blend, mask and uv shift.
        /// </summary>
        void UpdateBlend()
        {
            if (_computeBuffer != null)
            {
                _computeBuffer.Release();
            }

            _computeBuffer = new ComputeBuffer(NumOfScreen, Marshal.SizeOf(typeof(ProjectorUtilityBuffer)));
            ProjectorUtilityBuffer[] buf = new ProjectorUtilityBuffer[_computeBuffer.count];

            _screenSettingEntities.ForEach(entity =>
            {
                buf[entity.ID] = new ProjectorUtilityBuffer(
                    (uint)entity.ID,
                    entity.TopBlend.Value,
                    entity.BottomBlend.Value,
                    entity.LeftBlend.Value,
                    entity.RightBlend.Value,
                    entity.uvShift.Value.x,
                    entity.uvShift.Value.y,
                    entity.topMask.Value,
                    entity.bottomMask.Value,
                    entity.leftMask.Value,
                    entity.rightMask.Value
                    );
            });

            _computeBuffer.SetData(buf);

            _mat.SetBuffer("buf", _computeBuffer);

            _mat.SetInt("_numOfColPrjctrs", _commonSettingEntity.NumOfColProjectors.Value);
            _mat.SetInt("_numOfRowPrjctrs", _commonSettingEntity.NumOfRowProjectors.Value);
            _mat.SetFloat("_blackness", _commonSettingEntity.Blackness.Value);
            _mat.SetFloat("_power", _commonSettingEntity.Curve.Value);
            _mat.SetFloat("_brightness", _commonSettingEntity.Brightness.Value);

            CalculateNarrowestBlend();
        }

        void UpdateMaskMesh()
        {
            _maskingMeshes.Clear();
            int colScreens = _commonSettingEntity.NumOfColProjectors.Value;
            int rowScreens = _commonSettingEntity.NumOfRowProjectors.Value;
            float hrztlLength = 1.0f / colScreens;
            float vrtclLength = 1.0f / rowScreens;

            _screenSettingEntities.ForEach(entity =>
            {
                var mesh = new Mesh();
                var topLeftPos     = new Vector2(hrztlLength * ((float)entity.ID % (float)colScreens), 1.0f - vrtclLength * Mathf.Floor((float)entity.ID / (float)colScreens));
                var topRightPos    = new Vector2(topLeftPos.x + hrztlLength, topLeftPos.y);
                var bottomLeftPos  = new Vector2(topLeftPos.x, topLeftPos.y - vrtclLength);
                var bottomRightPos = new Vector2(topLeftPos.x + hrztlLength, topLeftPos.y - vrtclLength);

                mesh.vertices = new Vector3[]
                {
                    //top left corner
                    _camera.ViewportToWorldPoint(new Vector3(topLeftPos.x, topLeftPos.y, _camera.nearClipPlane)),
                    _camera.ViewportToWorldPoint(new Vector3(entity.topLeftMask.Value.x, topLeftPos.y, _camera.nearClipPlane)),
                    _camera.ViewportToWorldPoint(new Vector3(topLeftPos.x, topLeftPos.y - entity.topLeftMask.Value.y, _camera.nearClipPlane)),
                    //top right corner
                    _camera.ViewportToWorldPoint(new Vector3(topRightPos.x - entity.topRightMask.Value.x, topRightPos.y, _camera.nearClipPlane)),
                    _camera.ViewportToWorldPoint(new Vector3(topRightPos.x, topRightPos.y, _camera.nearClipPlane)),
                    _camera.ViewportToWorldPoint(new Vector3(topRightPos.x, topRightPos.y - entity.topRightMask.Value.y, _camera.nearClipPlane)),
                    //bottom left corner
                    _camera.ViewportToWorldPoint(new Vector3(bottomLeftPos.x, bottomLeftPos.y + entity.bottomLeftMask.Value.y, _camera.nearClipPlane)),
                    _camera.ViewportToWorldPoint(new Vector3(bottomLeftPos.x + entity.bottomLeftMask.Value.x, bottomLeftPos.y, _camera.nearClipPlane)),
                    _camera.ViewportToWorldPoint(new Vector3(bottomLeftPos.x, bottomLeftPos.y, _camera.nearClipPlane)),
                    //bottom right corner
                    _camera.ViewportToWorldPoint(new Vector3(bottomRightPos.x, bottomRightPos.y + entity.bottomRightMask.Value.y, _camera.nearClipPlane)),
                    _camera.ViewportToWorldPoint(new Vector3(bottomRightPos.x, bottomRightPos.y, _camera.nearClipPlane)),
                    _camera.ViewportToWorldPoint(new Vector3(bottomRightPos.x - entity.bottomRightMask.Value.x, bottomRightPos.y, _camera.nearClipPlane))
                };

                mesh.SetIndices(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, MeshTopology.Triangles, 0);
                _maskingMeshes.Add(mesh);
            });

            for (int i = 0; i < Model.RectMaskSettingEntity.MAX_RECTMASKS; i++)
            {
                if (_rectMaskSettingEntity.RectMaskWidth[i].Value == 0f || _rectMaskSettingEntity.RectMaskHeight[i].Value == 0f) continue;

                Mesh mesh = new Mesh();
                mesh.vertices = new Vector3[]
                {
                    _camera.ViewportToWorldPoint(new Vector3(_rectMaskSettingEntity.RectMaskX[i].Value, _rectMaskSettingEntity.RectMaskY[i].Value + _rectMaskSettingEntity.RectMaskHeight[i].Value, _camera.nearClipPlane)),
                    _camera.ViewportToWorldPoint(new Vector3(_rectMaskSettingEntity.RectMaskX[i].Value + _rectMaskSettingEntity.RectMaskWidth[i].Value, _rectMaskSettingEntity.RectMaskY[i].Value + _rectMaskSettingEntity.RectMaskHeight[i].Value, _camera.nearClipPlane)),
                    _camera.ViewportToWorldPoint(new Vector3(_rectMaskSettingEntity.RectMaskX[i].Value + _rectMaskSettingEntity.RectMaskWidth[i].Value, _rectMaskSettingEntity.RectMaskY[i].Value, _camera.nearClipPlane)),
                    _camera.ViewportToWorldPoint(new Vector3(_rectMaskSettingEntity.RectMaskX[i].Value, _rectMaskSettingEntity.RectMaskY[i].Value, _camera.nearClipPlane)),
                };
                mesh.SetIndices(new int[] { 0, 1, 2, 0, 2, 3}, MeshTopology.Triangles, 0);
                _maskingMeshes.Add(mesh);
            }

            _maskMesh = MeshUtility.BatchMesh(_maskingMeshes);
        }

        #endregion


        #region save and load

        /// <summary>
        /// Advance mode save and close.
        /// </summary>
        public void SaveAndCloseAdvancedMode()
        {
            _screenSettingEntities.ForEach(s => s.Save());
            _commonSettingEntity.Save();
            _advancedMode.modal.SetActive(false);
        }

        /// <summary>
        /// Advance mode load and close.
        /// </summary>
        public void DiscardAndCloseAdvancedMode()
        {
            _screenSettingEntities.ForEach(s => s.Load());
            _commonSettingEntity.Load();
            _advancedMode.modal.SetActive(false);
        }

        /// <summary>
        /// Advance mode save and close.
        /// </summary>
        public void SaveAndCloseSimpleMode()
        {
            _screenSettingEntities.ForEach(s => s.Save());
            _commonSettingEntity.Save();
            _simpleMode.modal.SetActive(false);
        }

        /// <summary>
        /// Advance mode load and close.
        /// </summary>
        public void DiscardAndCloseSimpleMode()
        {
            _screenSettingEntities.ForEach(s => s.Load());
            _commonSettingEntity.Load();
            _simpleMode.modal.SetActive(false);
        }

        /// <summary>
        /// Rect mask mode save and close.
        /// </summary>
        public void SaveAndCloseRectMaskMode()
        {
            _rectMaskSettingEntity.Save();
            _rectMaskMode.modal.SetActive(false);
        }

        /// <summary>
        /// Rect mask mode load and close.
        /// </summary>
        public void DiscardAndCloseRectMaskMode()
        {
            _rectMaskSettingEntity.Load();
            _rectMaskMode.modal.SetActive(false);
        }

        #endregion


        #region tab system

        /// <summary>
        /// Initialize tabs.
        /// </summary>
        void SetTabSystem()
        {
            if (_tabs != null)
            {
                _tabs.ForEach(pair =>
                {
                    pair.tab.OnClickAsObservable().Subscribe(_ =>
                    {
                        _tabs.ForEach(p =>
                        {
                            p.contents.gameObject.SetActive(false);
                            p.tab.interactable = true;
                        });
                        pair.contents.gameObject.SetActive(true);
                        pair.tab.interactable = false;
                    });
                    EnableTabContents(pair, false);
                });
                EnableTabContents(_tabs[0], true);
            }
        }
        
        /// <summary>
        /// Enable or disable tab.
        /// </summary>
        /// <param name="pair"></param>
        /// <param name="active"></param>
        void EnableTabContents(TabContentsSet pair, bool active)
        {
            pair.tab.interactable = !active;
            pair.contents.gameObject.SetActive(active);
        }

        #endregion


        #region simple mode

        /// <summary>
        /// Convert common and screen setting entity to simple view
        /// </summary>
        void ConvertAdvanceEntityToFitSimpleView()
        {
            _commonSettingEntity.NumOfRowProjectors.Value = 1;
            if (_commonSettingEntity.NumOfColProjectors.Value == 3)
            {
                _simpleSettingView.twoProjectionToggle.isOn = false;
                _commonSettingEntity.NumOfColProjectors.Value = 3;
                _screenSettingEntities[0].LeftBlend.Value  = 0;
                _screenSettingEntities[2].RightBlend.Value = 0;
            }
            else
            {
                _simpleSettingView.twoProjectionToggle.isOn = true;
                _commonSettingEntity.NumOfColProjectors.Value = 2;
                _screenSettingEntities[0].LeftBlend.Value  = 0;
                _screenSettingEntities[1].RightBlend.Value = 0;
            }
            _screenSettingEntities.ForEach(s => {
                s.topLeftMask.Value     = Vector2.zero;
                s.topRightMask.Value    = Vector2.zero;
                s.bottomLeftMask.Value  = Vector2.zero;
                s.bottomRightMask.Value = Vector2.zero;
            });
            _commonSettingEntity.Symmetry.Value = true;
            _simpleSettingView.blendWidthUI.SetVal(_screenSettingEntities[0].RightBlend.Value);
            _simpleSettingView.blendCurveUI.SetVal(_commonSettingEntity.Curve.Value);
            _simpleSettingView.blendAlphaUI.SetVal(_commonSettingEntity.Brightness.Value);
            _simpleSettingView.blendOffsetUI.SetVal(_commonSettingEntity.Blackness.Value);
        }

        #endregion


        #region unity builtin

        void OnDestroy()
        {
            if(_mat)
            {
                DestroyImmediate(_mat);
            }
            _computeBuffer.Release();
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (_mat != null)
            {
                Graphics.Blit(src, dest, _mat, 0);
            }
        }

        void OnPostRender()
        {
            if(_maskMesh != null)
            {
                _mat.SetPass(1);
                Graphics.DrawMeshNow(_maskMesh, _camera.cameraToWorldMatrix);
            }
        }

        void Start()
        {
            _keyViewModalSets.ForEach(e => e.modal.SetActive(false));
        }

        void Update()
        {
            if (!Input.anyKey) return;

            for (int i = 0; i < _keyViewModalSets.Count; i++)
            {
                if (Input.GetKeyDown(_keyViewModalSets[i].key) == true)
                {
                    if (_keyViewModalSets[i].modal.activeSelf)
                    {
                        if (_keyViewModalSets[i].key == _advancedMode.key)
                            DiscardAndCloseAdvancedMode();
                        else if (_keyViewModalSets[i].key == _rectMaskMode.key)
                            DiscardAndCloseRectMaskMode();
                        else if (_keyViewModalSets[i].key == _simpleMode.key)
                            DiscardAndCloseSimpleMode();
                        break;
                    }
                    if (_keyViewModalSets.Any(set => set.modal.activeSelf)) break;
                    _keyViewModalSets[i].modal.SetActive(true);
                    if (_keyViewModalSets[i].key == _simpleMode.key) ConvertAdvanceEntityToFitSimpleView();
                }
            }
        }

        #endregion
    }
    

    /// <summary>
    /// Simple mesh utility
    /// </summary>
    public static class MeshUtility
    {
        /// <summary>
        /// Simple mesh batcher
        /// </summary>
        /// <param name="meshes"></param>
        /// <returns>batched mesh</returns>
        public static Mesh BatchMesh(List<Mesh> meshes)
        {
            var mesh = new Mesh();

            int totalVertices = 0;
            int totalIndices  = 0;

            Vector3[] batchedVertices;
            int[]     batchedIndices;

            if (meshes == null || meshes.Count == 0)
            {
                Debug.LogWarning("Mesh is null");
                return mesh;
            }

            meshes.ForEach(m => {
                totalVertices += m.vertexCount;
                totalIndices  += m.GetIndices(0).Length;
            });

            batchedVertices = new Vector3[totalVertices];
            batchedIndices  = new int[totalIndices];

            for (int vertexOffset = 0, indexOffset = 0, i = 0; i < meshes.Count; i++)
            {
                var sourceMesh  = meshes[i];
                var sourceIndex = sourceMesh.GetIndices(0);
                Array.Copy(sourceMesh.vertices, 0, batchedVertices, vertexOffset, sourceMesh.vertices.Length);

                for (int j = 0; j < sourceIndex.Length; j++)
                {
                    batchedIndices[indexOffset + j] = vertexOffset + sourceIndex[j];
                }

                vertexOffset += sourceMesh.vertexCount;
                indexOffset  += sourceIndex.Length;
            }

            mesh.vertices = batchedVertices;
            mesh.SetIndices(batchedIndices, MeshTopology.Triangles, 0);
            mesh.Optimize();
            mesh.hideFlags = HideFlags.DontSave;

            return mesh;
        }
    }
}
