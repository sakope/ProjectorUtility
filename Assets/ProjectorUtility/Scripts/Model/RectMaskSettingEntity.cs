namespace ProjectorUtility.Model
{
    using UniRx;
    using XmlStorage;

    /// <summary>
    /// Rect mask setting model.
    /// Handle load and save.
    /// </summary>
    public class RectMaskSettingEntity : IProjectorSettingEntity
    {
        public ReactiveProperty<float>[] RectMaskX      { get; private set; }
        public ReactiveProperty<float>[] RectMaskY      { get; private set; }
        public ReactiveProperty<float>[] RectMaskWidth  { get; private set; }
        public ReactiveProperty<float>[] RectMaskHeight { get; private set; }

        string _rectMaskXPropPrefix      = "rectMaskX";
        string _rectMaskYPropPrefix      = "rectMaskY";
        string _rectMaskWidthPropPrefix  = "rectMaskWidth";
        string _rectMaskHeightPropPrefix = "rectMaskHeight";

        public const int MAX_RECTMASKS = 4;

        /// <summary>
        /// Constructor.
        /// </summary>
        public RectMaskSettingEntity()
        {
            RectMaskX      = new ReactiveProperty<float>[MAX_RECTMASKS];
            RectMaskY      = new ReactiveProperty<float>[MAX_RECTMASKS];
            RectMaskWidth  = new ReactiveProperty<float>[MAX_RECTMASKS];
            RectMaskHeight = new ReactiveProperty<float>[MAX_RECTMASKS];

            InitialLoad();
        }

        /// <summary>
        /// Initialize and Load rect mask settings.
        /// </summary>
        private void InitialLoad()
        {
            var currentAggregationKey = XmlStorage.CurrentAggregationName;
            XmlStorage.ChangeAggregation(CommonSettingEntity.XMLAggregationKey);

            for (int i = 0; i < MAX_RECTMASKS; i++)
            {
                RectMaskX[i]      = new ReactiveProperty<float>(XmlStorage.Get<float>(_rectMaskXPropPrefix + i.ToString(), 0f));
                RectMaskY[i]      = new ReactiveProperty<float>(XmlStorage.Get<float>(_rectMaskYPropPrefix + i.ToString(), 0f));
                RectMaskWidth[i]  = new ReactiveProperty<float>(XmlStorage.Get<float>(_rectMaskWidthPropPrefix + i.ToString(), 0f));
                RectMaskHeight[i] = new ReactiveProperty<float>(XmlStorage.Get<float>(_rectMaskHeightPropPrefix + i.ToString(), 0f));
            }

            XmlStorage.ChangeAggregation(currentAggregationKey);
        }

        /// <summary>
        /// Load rect mask settings.
        /// </summary>
        public void Load()
        {
            var currentAggregationKey = XmlStorage.CurrentAggregationName;
            XmlStorage.ChangeAggregation(CommonSettingEntity.XMLAggregationKey);

            for (int i = 0; i < MAX_RECTMASKS; i++)
            {
                RectMaskX[i].Value      = XmlStorage.Get<float>(_rectMaskXPropPrefix + i.ToString(), 0f);
                RectMaskY[i].Value      = XmlStorage.Get<float>(_rectMaskYPropPrefix + i.ToString(), 0f);
                RectMaskWidth[i].Value  = XmlStorage.Get<float>(_rectMaskWidthPropPrefix + i.ToString(), 0f);
                RectMaskHeight[i].Value = XmlStorage.Get<float>(_rectMaskHeightPropPrefix + i.ToString(), 0f);
            }

            XmlStorage.ChangeAggregation(currentAggregationKey);
        }

        /// <summary>
        /// Save rect mask settings.
        /// </summary>
        public void Save()
        {
            var currentAggregationKey = XmlStorage.CurrentAggregationName;
            XmlStorage.ChangeAggregation(CommonSettingEntity.XMLAggregationKey);
            XmlStorage.FileName = CommonSettingEntity.XMLAggregationKey;

            for (int i = 0; i < MAX_RECTMASKS; i++)
            {
                XmlStorage.Set<float>(_rectMaskXPropPrefix + i.ToString(), RectMaskX[i].Value);
                XmlStorage.Set<float>(_rectMaskYPropPrefix + i.ToString(), RectMaskY[i].Value);
                XmlStorage.Set<float>(_rectMaskWidthPropPrefix + i.ToString(), RectMaskWidth[i].Value);
                XmlStorage.Set<float>(_rectMaskHeightPropPrefix + i.ToString(), RectMaskHeight[i].Value);
            }

            XmlStorage.Save();

            XmlStorage.ChangeAggregation(currentAggregationKey);
        }
    }
}