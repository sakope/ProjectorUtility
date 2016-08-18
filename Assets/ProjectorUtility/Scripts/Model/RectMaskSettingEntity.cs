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
        //public ReactiveProperty<float> RectMask1X      { get; private set; }
        //public ReactiveProperty<float> RectMask1Y      { get; private set; }
        //public ReactiveProperty<float> RectMask1Width  { get; private set; }
        //public ReactiveProperty<float> RectMask1Height { get; private set; }
        //public ReactiveProperty<float> RectMask2X      { get; private set; }
        //public ReactiveProperty<float> RectMask2Y      { get; private set; }
        //public ReactiveProperty<float> RectMask2Width  { get; private set; }
        //public ReactiveProperty<float> RectMask2Height { get; private set; }
        //public ReactiveProperty<float> RectMask3X      { get; private set; }
        //public ReactiveProperty<float> RectMask3Y      { get; private set; }
        //public ReactiveProperty<float> RectMask3Width  { get; private set; }
        //public ReactiveProperty<float> RectMask3Height { get; private set; }
        //public ReactiveProperty<float> RectMask4X      { get; private set; }
        //public ReactiveProperty<float> RectMask4Y      { get; private set; }
        //public ReactiveProperty<float> RectMask4Width  { get; private set; }
        //public ReactiveProperty<float> RectMask4Height { get; private set; }

        string _rectMaskXPropPrefix      = "rectMaskX";
        string _rectMaskYPropPrefix      = "rectMaskY";
        string _rectMaskWidthPropPrefix  = "rectMaskWidth";
        string _rectMaskHeightPropPrefix = "rectMaskHeight";

        public const int MAX_RECTMASKS = 4;

        //string _rectMask1XProp      = "rectMask1X";
        //string _rectMask1YProp      = "rectMask1Y";
        //string _rectMask1WidthProp  = "rectMask1Width";
        //string _rectMask1HeightProp = "rectMask1Height";
        //string _rectMask2XProp      = "rectMask2X";
        //string _rectMask2YProp      = "rectMask2Y";
        //string _rectMask2WidthProp  = "rectMask2Width";
        //string _rectMask2HeightProp = "rectMask2Height";
        //string _rectMask3XProp      = "rectMask3X";
        //string _rectMask3YProp      = "rectMask3Y";
        //string _rectMask3WidthProp  = "rectMask3Width";
        //string _rectMask3HeightProp = "rectMask3Height";
        //string _rectMask4XProp      = "rectMask4X";
        //string _rectMask4YProp      = "rectMask4Y";
        //string _rectMask4WidthProp  = "rectMask4Width";
        //string _rectMask4HeightProp = "rectMask4Height";

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

            //RectMask1X      = new ReactiveProperty<float>(XmlStorage.Get<float>(_rectMask1XProp, 0f));
            //RectMask1Y      = new ReactiveProperty<float>(XmlStorage.Get<float>(_rectMask1YProp, 0f));
            //RectMask1Width  = new ReactiveProperty<float>(XmlStorage.Get<float>(_rectMask1WidthProp, 0f));
            //RectMask1Height = new ReactiveProperty<float>(XmlStorage.Get<float>(_rectMask1HeightProp, 0f));
            //RectMask2X      = new ReactiveProperty<float>(XmlStorage.Get<float>(_rectMask2XProp, 0f));
            //RectMask2Y      = new ReactiveProperty<float>(XmlStorage.Get<float>(_rectMask2YProp, 0f));
            //RectMask2Width  = new ReactiveProperty<float>(XmlStorage.Get<float>(_rectMask2WidthProp, 0f));
            //RectMask2Height = new ReactiveProperty<float>(XmlStorage.Get<float>(_rectMask2HeightProp, 0f));
            //RectMask3X      = new ReactiveProperty<float>(XmlStorage.Get<float>(_rectMask3XProp, 0f));
            //RectMask3Y      = new ReactiveProperty<float>(XmlStorage.Get<float>(_rectMask3YProp, 0f));
            //RectMask3Width  = new ReactiveProperty<float>(XmlStorage.Get<float>(_rectMask3WidthProp, 0f));
            //RectMask3Height = new ReactiveProperty<float>(XmlStorage.Get<float>(_rectMask3HeightProp, 0f));
            //RectMask4X      = new ReactiveProperty<float>(XmlStorage.Get<float>(_rectMask4XProp, 0f));
            //RectMask4Y      = new ReactiveProperty<float>(XmlStorage.Get<float>(_rectMask4YProp, 0f));
            //RectMask4Width  = new ReactiveProperty<float>(XmlStorage.Get<float>(_rectMask4WidthProp, 0f));
            //RectMask4Height = new ReactiveProperty<float>(XmlStorage.Get<float>(_rectMask4HeightProp, 0f));

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

            //RectMask1X.Value      = XmlStorage.Get<float>(_rectMask1XProp, 0f);
            //RectMask1Y.Value      = XmlStorage.Get<float>(_rectMask1YProp, 0f);
            //RectMask1Width.Value  = XmlStorage.Get<float>(_rectMask1WidthProp, 0f);
            //RectMask1Height.Value = XmlStorage.Get<float>(_rectMask1HeightProp, 0f);
            //RectMask2X.Value      = XmlStorage.Get<float>(_rectMask2XProp, 0f);
            //RectMask2Y.Value      = XmlStorage.Get<float>(_rectMask2YProp, 0f);
            //RectMask2Width.Value  = XmlStorage.Get<float>(_rectMask2WidthProp, 0f);
            //RectMask2Height.Value = XmlStorage.Get<float>(_rectMask2HeightProp, 0f);
            //RectMask3X.Value      = XmlStorage.Get<float>(_rectMask3XProp, 0f);
            //RectMask3Y.Value      = XmlStorage.Get<float>(_rectMask3YProp, 0f);
            //RectMask3Width.Value  = XmlStorage.Get<float>(_rectMask3WidthProp, 0f);
            //RectMask3Height.Value = XmlStorage.Get<float>(_rectMask3HeightProp, 0f);
            //RectMask4X.Value      = XmlStorage.Get<float>(_rectMask4XProp, 0f);
            //RectMask4Y.Value      = XmlStorage.Get<float>(_rectMask4YProp, 0f);
            //RectMask4Width.Value  = XmlStorage.Get<float>(_rectMask4WidthProp, 0f);
            //RectMask4Height.Value = XmlStorage.Get<float>(_rectMask4HeightProp, 0f);

            XmlStorage.ChangeAggregation(currentAggregationKey);
        }

        /// <summary>
        /// Save rect mask settings.
        /// </summary>
        public void Save()
        {
            XmlStorage.FileName = CommonSettingEntity.XMLAggregationKey;
            var currentAggregationKey = XmlStorage.CurrentAggregationName;
            XmlStorage.ChangeAggregation(CommonSettingEntity.XMLAggregationKey);

            for (int i = 0; i < MAX_RECTMASKS; i++)
            {
                XmlStorage.Set<float>(_rectMaskXPropPrefix + i.ToString(), RectMaskX[i].Value);
                XmlStorage.Set<float>(_rectMaskYPropPrefix + i.ToString(), RectMaskY[i].Value);
                XmlStorage.Set<float>(_rectMaskWidthPropPrefix + i.ToString(), RectMaskWidth[i].Value);
                XmlStorage.Set<float>(_rectMaskHeightPropPrefix + i.ToString(), RectMaskHeight[i].Value);
            }

            //XmlStorage.Set<float>(_rectMask1XProp, RectMask1X.Value);
            //XmlStorage.Set<float>(_rectMask1YProp, RectMask1Y.Value);
            //XmlStorage.Set<float>(_rectMask1WidthProp, RectMask1Width.Value);
            //XmlStorage.Set<float>(_rectMask1HeightProp, RectMask1Height.Value);
            //XmlStorage.Set<float>(_rectMask2XProp, RectMask2X.Value);
            //XmlStorage.Set<float>(_rectMask2YProp, RectMask2Y.Value);
            //XmlStorage.Set<float>(_rectMask2WidthProp, RectMask2Width.Value);
            //XmlStorage.Set<float>(_rectMask2HeightProp, RectMask2Height.Value);
            //XmlStorage.Set<float>(_rectMask3XProp, RectMask3X.Value);
            //XmlStorage.Set<float>(_rectMask3YProp, RectMask3Y.Value);
            //XmlStorage.Set<float>(_rectMask3WidthProp, RectMask3Width.Value);
            //XmlStorage.Set<float>(_rectMask3HeightProp, RectMask3Height.Value);
            //XmlStorage.Set<float>(_rectMask4XProp, RectMask4X.Value);
            //XmlStorage.Set<float>(_rectMask4YProp, RectMask4Y.Value);
            //XmlStorage.Set<float>(_rectMask4WidthProp, RectMask4Width.Value);
            //XmlStorage.Set<float>(_rectMask4HeightProp, RectMask4Height.Value);
            XmlStorage.Save();

            XmlStorage.ChangeAggregation(currentAggregationKey);
        }
    }
}