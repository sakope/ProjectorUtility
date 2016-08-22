namespace ProjectorUtility.Model
{
    using UniRx;
    using XmlStorage;

    /// <summary>
    /// Global mask setting model.
    /// Handle load and save.
    /// </summary>
    public class GlobalMaskSettingEntity : IProjectorSettingEntity
    {
        public ReactiveProperty<float> TopMask { get; private set; }
        public ReactiveProperty<float> BottomMask { get; private set; }
        public ReactiveProperty<float> LeftMask { get; private set; }
        public ReactiveProperty<float> RightMask { get; private set; }

        string _topMaskProp    = "topMaskProp";
        string _bottomMaskProp = "bottomMaskProp";
        string _leftMaskProp   = "leftMaskProp";
        string _rightMaskProp  = "rightMaskProp";
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public GlobalMaskSettingEntity()
        {
            InitialLoad();
        }

        /// <summary>
        /// Initialize and load global mask settings.
        /// </summary>
        public void InitialLoad()
        {
            var currentAggregationKey = XmlStorage.CurrentAggregationName;
            XmlStorage.ChangeAggregation(CommonSettingEntity.XMLAggregationKey);

            TopMask    = new ReactiveProperty<float>(XmlStorage.Get<float>(_topMaskProp, 0f));
            BottomMask = new ReactiveProperty<float>(XmlStorage.Get<float>(_bottomMaskProp, 0f));
            LeftMask   = new ReactiveProperty<float>(XmlStorage.Get<float>(_leftMaskProp, 0f));
            RightMask  = new ReactiveProperty<float>(XmlStorage.Get<float>(_rightMaskProp, 0f));

            XmlStorage.ChangeAggregation(currentAggregationKey);
        }

        /// <summary>
        /// Load global mask settings.
        /// </summary>
        public void Load()
        {
            var currentAggregationKey = XmlStorage.CurrentAggregationName;
            XmlStorage.ChangeAggregation(CommonSettingEntity.XMLAggregationKey);

            TopMask.Value    = XmlStorage.Get<float>(_topMaskProp, 0f);
            BottomMask.Value = XmlStorage.Get<float>(_bottomMaskProp, 0f);
            LeftMask.Value   = XmlStorage.Get<float>(_leftMaskProp, 0f);
            RightMask.Value  = XmlStorage.Get<float>(_rightMaskProp, 0f);

            XmlStorage.ChangeAggregation(currentAggregationKey);
        }

        /// <summary>
        /// Save gloabl mask settings.
        /// </summary>
        public void Save()
        {
            XmlStorage.FileName = CommonSettingEntity.XMLAggregationKey;
            var currentAggregationKey = XmlStorage.CurrentAggregationName;
            XmlStorage.ChangeAggregation(CommonSettingEntity.XMLAggregationKey);

            XmlStorage.Set<float>(_topMaskProp, TopMask.Value);
            XmlStorage.Set<float>(_bottomMaskProp, BottomMask.Value);
            XmlStorage.Set<float>(_leftMaskProp, LeftMask.Value);
            XmlStorage.Set<float>(_rightMaskProp, RightMask.Value);
            XmlStorage.Save();

            XmlStorage.ChangeAggregation(currentAggregationKey);
        }
    }
}
