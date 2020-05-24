using BeatSaberMarkupLanguage.Attributes;

namespace PP_Helper.UI
{
    class Settings : PersistentSingleton<Settings>
    {
        [UIValue("defaultAcc")]
        public float defaultAcc
        {
            get => Config.defaultAcc;
            set
            {
                Config.defaultAcc = value;
                Config.Write();
            }
        }
        [UIValue("starRange")]
        public float starRange
        {
            get => Config.starRange;
            set
            {
                Config.starRange = value;
                Config.Write();
            }
        }
    }
}
