using CountersPlus.Config;
using System;
using System.Reflection;

namespace PP_Helper.Counters
{
    class ConfigLoader : CountersPlus.Config.ConfigLoader
    {
        internal static BS_Utils.Utilities.Config config = new BS_Utils.Utilities.Config("CountersPlus");
        public static MainConfigModel LoadPPConfig()
        {
            MainConfigModel model = new MainConfigModel();
            model = DeserializeFromConfig(model, model.DisplayName);
            try
            {
                model.ppConfig = DeserializeFromConfig(model.ppConfig, model.ppConfig.DisplayName);
            }
            catch (Exception e)
            {
                if (e.GetType() != typeof(NullReferenceException)) Logger.log.Error(e.ToString());
            }
            return model;
        }
    }

    public class MainConfigModel : CountersPlus.Config.MainConfigModel
    {
        public PPConfigModel ppConfig = new PPConfigModel();
    }

    public sealed class PPConfigModel : ConfigModel
    {
        public PPConfigModel()
        {
            DisplayName = "PPHelper Counter";
            Enabled = true; Position = ICounterPositions.BelowCombo; Distance = 3;
        } // Default Values
    }

    // CountersPlus ConfigModel is internal-set so need to redefine here
    public abstract class ConfigModel
    {
        public string DisplayName { get; internal set; } // DisplayName should not be changed once set.
        public bool Enabled;
        public ICounterPositions Position;
        public int Distance;

        public void Save()
        {
            MemberInfo[] infos = GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
            foreach (MemberInfo info in infos)
            {
                if (info.MemberType != MemberTypes.Field) continue;
                FieldInfo finfo = (FieldInfo)info;
                ConfigLoader.config.SetString(DisplayName, info.Name, finfo.GetValue(this).ToString());
            }
        }
    }
}
