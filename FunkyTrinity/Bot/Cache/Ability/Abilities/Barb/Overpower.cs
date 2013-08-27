using System;
using Zeta.Internals.Actors;

namespace FunkyTrinity.ability.Abilities.Barb
{
    public class Overpower : Ability, IAbility
    {
        public Overpower()
            : base()
        {
        }

        public override SNOPower Power
        {
            get { return SNOPower.Barbarian_Overpower; }
        }

        public override int RuneIndex { get { return Bot.Class.RuneIndexCache.ContainsKey(this.Power) ? Bot.Class.RuneIndexCache[this.Power] : -1; } }

        public override void Initialize()
        {
            ExecutionType = AbilityUseType.Self;
            WaitVars = new WaitLoops(4, 4, true);
            Cost = 0;
            UseageType = AbilityUseage.Anywhere;
            Priority = AbilityPriority.Low;
            PreCastConditions = (AbilityConditions.CheckRecastTimer |
                                 AbilityConditions.CheckCanCast | AbilityConditions.CheckPlayerIncapacitated);
            ClusterConditions = new ClusterConditions(5d, 7, 1, false);
            Fcriteria = new Func<bool>(() =>
            {
                return true;
            });
        }
        public override void InitCriteria()
        {
            base.AbilityTestConditions = new AbilityUsablityTests(this);
        }
        #region IAbility
        public override int GetHashCode()
        {
            return (int)this.Power;
        }
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types. 
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }
            else
            {
                Ability p = (Ability)obj;
                return this.Power == p.Power;
            }
        }


        #endregion
    }
}
