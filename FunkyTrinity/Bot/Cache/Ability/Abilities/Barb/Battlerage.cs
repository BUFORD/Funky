using System;
using Zeta.Internals.Actors;

namespace FunkyTrinity.ability.Abilities.Barb
{
    public class Battlerage : Ability, IAbility
    {
        public Battlerage()
            : base()
        {
        }

        public override SNOPower Power
        {
            get { return SNOPower.Barbarian_BattleRage; }
        }

        public override int RuneIndex { get { return Bot.Class.RuneIndexCache.ContainsKey(this.Power) ? Bot.Class.RuneIndexCache[this.Power] : -1; } }

        public override void Initialize()
        {
            ExecutionType = AbilityUseType.Buff;
            WaitVars = new WaitLoops(1, 1, true);
            Cost = 20;
            //DelayInSeconds = 69;
            IsBuff = true;
            UseageType = AbilityUseage.Anywhere;
            Priority = AbilityPriority.High;
            PreCastConditions = (AbilityConditions.CheckEnergy | AbilityConditions.CheckPlayerIncapacitated | AbilityConditions.CheckExisitingBuff);
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
