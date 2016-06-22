using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;

using UniRx;
using Common;

using ProjectorUtility.Model;
using ProjectorUtility.View;
using ProjectorUtility.Core;

namespace ProjectorUtility.Controller
{
    /// <summary>
    /// Handle views and models
    /// </summary>
    public class ProjectorUtilityController : SingletonMonoBehaviour<ProjectorUtilityController>
    {
        #region variables

        [Serializable] public struct TabContentsSet
        {
            public Button     tab;
            public GameObject contents;
        }
        [SerializeField] TabContentsSet _commonTabContents;
        [SerializeField] TabContentsSet _screenTabContents;
        [SerializeField] Transform      _tabParent;
        [SerializeField] Transform      _contentsParent;

        CommonSettingEntity _commonSettingEntity;
        CommonSettingView   _commonSettingView;
        List<ScreenSettingEntity> _screenSettingEntities = new List<ScreenSettingEntity>();
        List<ScreenSettingView>   _screenSettingViews    = new List<ScreenSettingView>();
        List<TabContentsSet> _tabs = new List<TabContentsSet>();

        const int MAX_COL = 30;
        const int MAX_ROW = 30;

        public int NumOfScreen { get; private set; }
        public CommonSettingEntity GetCommonSettingEntity { get { return _commonSettingEntity; } }
        public List<ScreenSettingEntity> GetScreenSettingEntities { get { return _screenSettingEntities; } }

        #endregion


        #region API

        /// <summary>
        /// Normalized total blend width.
        /// </summary>
        /// <returns>Return normalized total blend width</returns>
        public float NormalizedBlendWidth()
        {
            float tbw = 0f;
            _screenSettingEntities.ForEach(e => tbw += e.LeftBlend.Value + e.RightBlend.Value);
            return tbw;
        }

        /// <summary>
        /// normalized total blend height. 
        /// </summary>
        /// <returns>Return normalized total blend height</returns>
        public float NormalizedBlendHeight()
        {
            float tbh = 0f;
            _screenSettingEntities.ForEach(e => tbh += e.TopBlend.Value + e.BottomBlend.Value);
            return tbh;
        }

        /// <summary>
        /// Total screen blending width.
        /// </summary>
        /// <returns>Return total screen blending width</returns>
        public float BlendingHeight()
        {
            return NormalizedBlendHeight() * Screen.width;
        }

        /// <summary>
        /// Total screen blend height.
        /// </summary>
        /// <returns>Return total screen blending height</returns>
        public float BlendingWidth()
        {
            return NormalizedBlendWidth() * Screen.height;
        }

        /// <summary>
        /// Return adjusted viewport position for blend and uv shift. 
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

            int   colProjectors         = _commonSettingEntity.NumOfColProjectors.Value;
            int   rowProjectors         = _commonSettingEntity.NumOfRowProjectors.Value;
            int   currentCol            = Mathf.FloorToInt(adjustPosition.x / (1.0f / colProjectors));
            int   currentRow            = Mathf.FloorToInt(adjustPosition.y / (1.0f / rowProjectors));
            int   screenID              = currentCol + currentRow * colProjectors;

            int leftOverlapCount  = Mathf.FloorToInt((float)colProjectors / 2f) - currentCol;
            int rightOverlapCount = currentCol + 1 - Mathf.CeilToInt((float)colProjectors / 2f);
            int upperOverlapCount = Mathf.FloorToInt((float)rowProjectors / 2f) - currentRow;
            int lowerOverlapCount = currentRow + 1 - Mathf.CeilToInt((float)rowProjectors / 2f);

            if (leftOverlapCount > 0)
            {
                for (int i = 0; i < leftOverlapCount; i++)
                {
                    if (colProjectors % 2 != 0 || colProjectors / 2 != currentCol + 1 + i)
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
                    if (colProjectors % 2 != 0 || colProjectors / 2 != colProjectors - currentCol + i)
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
                    if (rowProjectors % 2 != 0 || rowProjectors / 2 != currentRow + 1 + i)
                    {
                        adjustPosition.y += _screenSettingEntities[screenID + (i + 1) * colProjectors].TopBlend.Value;
                    }
                    adjustPosition.y += _screenSettingEntities[screenID + i * colProjectors].BottomBlend.Value;
                }
            }
            if (lowerOverlapCount > 0)
            {
                for (int i = 0; i < lowerOverlapCount; i++)
                {
                    if(rowProjectors % 2 != 0 || rowProjectors / 2 != rowProjectors - currentRow + i)
                    {
                        adjustPosition.y -= _screenSettingEntities[screenID - (i + 1) * colProjectors].BottomBlend.Value;
                    }
                    adjustPosition.y -= _screenSettingEntities[screenID - i * colProjectors].TopBlend.Value;
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
        }

        void Start()
        {
            _commonSettingEntity = new CommonSettingEntity();
            _commonSettingView   = _commonTabContents.contents.GetComponent<CommonSettingView>();

            _tabs.Add(_commonTabContents);

            BuildCommonSetting();
            BuildScreenSetting();
            SetTabSystem();
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
            _commonSettingEntity.ApplyAll.Subscribe(v => _commonSettingView.applyAllToggle.isOn = v);

            //From view to model reactives
            _commonSettingView.numOfCol.OnValueChangedAsObservable().Where(s => int.Parse(s) < MAX_COL).Subscribe(s => _commonSettingEntity.NumOfColProjectors.Value = int.Parse(s));
            _commonSettingView.numOfRow.OnValueChangedAsObservable().Where(s => int.Parse(s) < MAX_ROW).Subscribe(s => _commonSettingEntity.NumOfRowProjectors.Value = int.Parse(s));
            _commonSettingView.blacknessUI.slider.OnValueChangedAsObservable().Subscribe(v => _commonSettingEntity.Blackness.Value = v);
            _commonSettingView.curveUI.slider.OnValueChangedAsObservable().Subscribe(v => _commonSettingEntity.Curve.Value = v);
            _commonSettingView.applyAllToggle.OnValueChangedAsObservable().Subscribe(v => _commonSettingEntity.ApplyAll.Value = v);
            _commonSettingView.saveButton.OnClickAsObservable().Subscribe(_ => SaveAllData());
            _commonSettingView.discardButton.OnClickAsObservable().Subscribe(_ => LoadAllData());

            //From model to core reactive (skip initialize).
            _commonSettingEntity.NumOfColProjectors.SkipLatestValueOnSubscribe().Subscribe(n => BuildScreenSetting());
            _commonSettingEntity.NumOfRowProjectors.SkipLatestValueOnSubscribe().Subscribe(n => BuildScreenSetting());
            _commonSettingEntity.Blackness.SkipLatestValueOnSubscribe().Subscribe(n => UpdateBlend());
            _commonSettingEntity.Curve.SkipLatestValueOnSubscribe().Subscribe(n => UpdateBlend());
            _commonSettingEntity.ApplyAll.SkipLatestValueOnSubscribe().Subscribe(n => UpdateBlendMaster());
        }

        /// <summary>
        /// Build screen setting tabs, views, models and connect view <-> model reactives.
        /// </summary>
        void BuildScreenSetting()
        {
            //if number of projectors is 0, force change 1.
            if (_commonSettingEntity.NumOfColProjectors.Value < 1) _commonSettingEntity.NumOfColProjectors.Value = 1;
            if (_commonSettingEntity.NumOfRowProjectors.Value < 1) _commonSettingEntity.NumOfRowProjectors.Value = 1;

            NumOfScreen = _commonSettingEntity.NumOfColProjectors.Value * _commonSettingEntity.NumOfRowProjectors.Value;

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

                    //From model to view reactives
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

                    //From model to core reactives (skip initialize)
                    if (i == 0)
                    {
                        screenSettingEntity.TopBlend.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlendMaster());
                        screenSettingEntity.BottomBlend.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlendMaster());
                        screenSettingEntity.LeftBlend.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlendMaster());
                        screenSettingEntity.RightBlend.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlendMaster());
                        screenSettingEntity.topMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlendMaster());
                        screenSettingEntity.bottomMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlendMaster());
                        screenSettingEntity.leftMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlendMaster());
                        screenSettingEntity.rightMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlendMaster());
                        screenSettingEntity.topLeftMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlendMaster());
                        screenSettingEntity.topRightMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlendMaster());
                        screenSettingEntity.bottomLeftMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlendMaster());
                        screenSettingEntity.bottomRightMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlendMaster());
                        screenSettingEntity.uvShift.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlendMaster());
                    }
                    else
                    {
                        screenSettingEntity.TopBlend.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlend());
                        screenSettingEntity.BottomBlend.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlend());
                        screenSettingEntity.LeftBlend.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlend());
                        screenSettingEntity.RightBlend.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlend());
                        screenSettingEntity.topMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlend());
                        screenSettingEntity.bottomMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlend());
                        screenSettingEntity.leftMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlend());
                        screenSettingEntity.rightMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlend());
                        screenSettingEntity.topLeftMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlend());
                        screenSettingEntity.topRightMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlend());
                        screenSettingEntity.bottomLeftMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlend());
                        screenSettingEntity.bottomRightMask.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlend());
                        screenSettingEntity.uvShift.SkipLatestValueOnSubscribe().Subscribe(v => UpdateBlend());
                    }

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
        }

        #endregion


        #region execute blend

        /// <summary>
        /// refine blend, mask and uv shift.
        /// </summary>
        void UpdateBlend()
        {
            //if(NumOfScreen == 0 && _screenSettingModelViewMap.Count == 0)
            //{
            //    return;
            //}
            ProjectorUtilityBlender.Instance.SetBuffer();
        }

        /// <summary>
        /// reafine all blend, mask and uv shift using screen 1 setting.
        /// </summary>
        void UpdateBlendMaster()
        {
            if(_commonSettingEntity.ApplyAll.Value)
            {
                for (int i = 1; i < _screenSettingEntities.Count; i++)
                {
                    _screenSettingEntities[i].TopBlend.Value = _screenSettingEntities[0].TopBlend.Value;
                    _screenSettingEntities[i].BottomBlend.Value = _screenSettingEntities[0].BottomBlend.Value;
                    _screenSettingEntities[i].LeftBlend.Value = _screenSettingEntities[0].LeftBlend.Value;
                    _screenSettingEntities[i].RightBlend.Value = _screenSettingEntities[0].RightBlend.Value;
                    _screenSettingEntities[i].topMask.Value = _screenSettingEntities[0].topMask.Value;
                    _screenSettingEntities[i].bottomMask.Value = _screenSettingEntities[0].bottomMask.Value;
                    _screenSettingEntities[i].leftMask.Value = _screenSettingEntities[0].leftMask.Value;
                    _screenSettingEntities[i].rightMask.Value = _screenSettingEntities[0].rightMask.Value;
                    _screenSettingEntities[i].topLeftMask.Value = _screenSettingEntities[0].topLeftMask.Value;
                    _screenSettingEntities[i].topRightMask.Value = _screenSettingEntities[0].topRightMask.Value;
                    _screenSettingEntities[i].bottomLeftMask.Value = _screenSettingEntities[0].bottomLeftMask.Value;
                    _screenSettingEntities[i].bottomRightMask.Value = _screenSettingEntities[0].bottomRightMask.Value;
                    _screenSettingEntities[i].uvShift.Value = _screenSettingEntities[0].uvShift.Value;
                }
            }
            UpdateBlend();
        }

        #endregion


        #region save and load
        
        /// <summary>
        /// Save all data.
        /// </summary>
        public void SaveAllData()
        {
            _screenSettingEntities.ForEach(s => s.Save());
            _commonSettingEntity.Save();
        }

        /// <summary>
        /// Load all data.
        /// </summary>
        public void LoadAllData()
        {
            _screenSettingEntities.ForEach(s => s.Load());
            _commonSettingEntity.Load();
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
    } 
}
