using System;

namespace FunkyTrinity.ability.Abilities
{
    public static class AbilitiyExtensions
    {
        public static double SecondsFromLastUse(this Ability ability)
        {
            return DateTime.Now.Subtract(ability.LastUsed).TotalSeconds;
        }

        //public static bool HasDelayExpired(this Ability ability)
        //{
        //    return ability.SecondsFromLastUse() >= ability.DelayInSeconds;
        //}
    }
}
