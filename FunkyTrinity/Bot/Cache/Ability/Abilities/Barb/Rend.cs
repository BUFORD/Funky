using System;
using Zeta.Internals.Actors;

namespace FunkyTrinity.ability.Abilities.Barb
{
    public class Rend : Ability, IAbility
    {
        public Rend()
            : base()
        {
        }

        public override SNOPower Power
        {
            get { return SNOPower.Barbarian_Rend; }
        }

        public override int RuneIndex { get { return Bot.Class.RuneIndexCache.ContainsKey(this.Power) ? Bot.Class.RuneIndexCache[this.Power] : -1; } }

        public override void Initialize()
        {
            ExecutionType = AbilityUseType.Self;
            WaitVars = new WaitLoops(3, 3, true);
            Cost = 20;
            UseageType = AbilityUseage.Combat;
            Priority = AbilityPriority.High;
            PreCastConditions = (AbilityConditions.CheckRecastTimer | AbilityConditions.CheckEnergy |
                                                     AbilityConditions.CheckCanCast | AbilityConditions.CheckPlayerIncapacitated);
            TargetUnitConditionFlags = new UnitTargetConditions(TargetProperties.None, 10);
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
