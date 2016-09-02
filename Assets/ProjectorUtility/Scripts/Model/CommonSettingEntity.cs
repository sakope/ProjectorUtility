namespace ProjectorUtility.Model
{
    using UniRx;
    using XmlStorage;

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
        public ReactiveProperty<float> Brightness { get; private set; }
        public ReactiveProperty<bool>  Symmetry { get; private set; }

        string _numOfColProjectorsProp = "numOfColProjectors";
        string _numOfRowProjectorsProp = "numOfRowProjectors";
        string _blacknessProp          = "blackness";
        string _curveProp              = "curve";
        string _brightnessProp         = "brightness";
        string _symmetry               = "symmetry";

        public const float GAMMA_CURVE = 2.2f;

        public const string XMLAggregationKey = "Blending";

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
            var currentAggregationKey = XmlStorage.CurrentAggregationName;
            XmlStorage.ChangeAggregation(XMLAggregationKey);

            NumOfColProjectors = new ReactiveProperty<int>(XmlStorage.Get<int>(_numOfColProjectorsProp, 1));
            NumOfRowProjectors = new ReactiveProperty<int>(XmlStorage.Get<int>(_numOfRowProjectorsProp, 1));
            Blackness          = new ReactiveProperty<float>(XmlStorage.Get<float>(_blacknessProp, 1.0f));
            Curve              = new ReactiveProperty<float>(XmlStorage.Get<float>(_curveProp, GAMMA_CURVE));
            Brightness         = new ReactiveProperty<float>(XmlStorage.Get<float>(_brightnessProp, 1.0f));
            Symmetry           = new ReactiveProperty<bool>(XmlStorage.Get<bool>(_symmetry, true));

            XmlStorage.ChangeAggregation(currentAggregationKey);
        }

        /// <summary>
        /// Load common settings.
        /// </summary>
        public void Load()
        {
            var currentAggregationKey = XmlStorage.CurrentAggregationName;
            XmlStorage.ChangeAggregation(XMLAggregationKey);

            NumOfColProjectors.Value = XmlStorage.Get<int>(_numOfColProjectorsProp, 1);
            NumOfRowProjectors.Value = XmlStorage.Get<int>(_numOfRowProjectorsProp, 1);
            Blackness.Value          = XmlStorage.Get<float>(_blacknessProp, 1.0f);
            Curve.Value              = XmlStorage.Get<float>(_curveProp, GAMMA_CURVE);
            Brightness.Value         = XmlStorage.Get<float>(_brightnessProp, 1.0f);
            Symmetry.Value           = XmlStorage.Get<bool>(_symmetry, true);

            XmlStorage.ChangeAggregation(currentAggregationKey);
        }

        /// <summary>
        /// Save common settings.
        /// </summary>
        public void Save()
        {
            XmlStorage.FileName = XMLAggregationKey;
            var currentAggregationKey = XmlStorage.CurrentAggregationName;
            XmlStorage.ChangeAggregation(XMLAggregationKey);

            XmlStorage.Set<int>(_numOfColProjectorsProp, NumOfColProjectors.Value);
            XmlStorage.Set<int>(_numOfRowProjectorsProp, NumOfRowProjectors.Value);
            XmlStorage.Set<float>(_blacknessProp, Blackness.Value);
            XmlStorage.Set<float>(_curveProp, Curve.Value);
            XmlStorage.Set<float>(_brightnessProp, Brightness.Value);
            XmlStorage.Set<bool>(_symmetry, Symmetry.Value);
            XmlStorage.Save();

            XmlStorage.ChangeAggregation(currentAggregationKey);
        }
    }
}