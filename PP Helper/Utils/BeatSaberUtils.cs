using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PP_Helper.Utils
{
    public static class BeatSaberUtils
    {
        public static bool HasPositiveModifiers(GameplayModifiers modifiers)
        {
            return modifiers.disappearingArrows ||
                   modifiers.ghostNotes ||
                   modifiers.fastNotes;
        }

        public static float GetModifiedAcc(float accuracy, GameplayModifiersModelSO modifiersModel, GameplayModifiers modifiers)
        {
            return modifiersModel.GetTotalMultiplier(modifiers) * accuracy;
        }
    }
}
