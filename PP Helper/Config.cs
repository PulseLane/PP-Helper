using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PP_Helper
{
    class Config
    {
        public static BS_Utils.Utilities.Config config = new BS_Utils.Utilities.Config("PP Helper");
        public static float defaultAcc = 80.0f;
        public static float starRange = 0.25f;

        public static void Read()
        {
            defaultAcc = config.GetFloat("PP Helper", "defaultAcc", 80f, true);
            starRange = config.GetFloat("PP Helper", "starRange", 0.5f, true);
        }

        public static void Write()
        {
            config.SetFloat("PP Helper", "defaultAcc", defaultAcc);
            config.SetFloat("PP Helper", "starRange", starRange);
        }
    }
}
