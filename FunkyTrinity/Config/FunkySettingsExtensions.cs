using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FunkyTrinity
{
    public static class FunkySettingsExtensions
    {
        public static bool ShouldBotStop(this FunkyTrinity.Funky.Settings_Funky settings)
        {
            if (!settings.BotStopTime.HasValue || DateTime.Now < settings.BotStopTime)
            {
                return false;
            }

            return true;
        }
    }
}
