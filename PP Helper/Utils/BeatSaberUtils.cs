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
                   modifiers.songSpeed == GameplayModifiers.SongSpeed.Faster;
        }

        public static float GetModifiedAcc(float accuracy, GameplayModifiersModelSO modifiersModel, GameplayModifiers modifiers)
        {
            return modifiersModel.GetTotalMultiplier(modifiers) * accuracy;
        }

        public static GameplayModifiers RemovePositiveModifiers(GameplayModifiers modifiers)
        {
            GameplayModifiers newModifiers = new GameplayModifiers(modifiers);
            newModifiers.disappearingArrows = false;
            newModifiers.ghostNotes = false;
            newModifiers.songSpeed = newModifiers.songSpeed.Equals(GameplayModifiers.SongSpeed.Faster) ? GameplayModifiers.SongSpeed.Normal : newModifiers.songSpeed;
            return newModifiers;
        }
    }
}
