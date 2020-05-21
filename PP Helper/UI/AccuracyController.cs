using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PP_Helper
{
    public class AccuracyController : ListSettingsController
    {
        protected override void ApplyValue(int idx)
        {
        }

        protected override bool GetInitValues(out int idx, out int numberOfElements)
        {
            numberOfElements = 200;
            idx = 170; // 85%
            return true;
        }

        protected override string TextForValue(int idx)
        {
            return (0.5f * idx).ToString() + "%";
        }
    }
}
